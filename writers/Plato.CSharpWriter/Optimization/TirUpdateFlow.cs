// Walks one function body once and decides which SetAt calls may write in place (the analysis of --in-place).
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Which standard-library calls the in-place pass relies on, found by declaring library and
/// signature and not by name alone: a user function called <c>SetAt</c> is not the update.
/// </summary>
public sealed class TirArrayIdentity
{
    private static readonly HashSet<string> FreshProducers = new(StringComparer.Ordinal)
    {
        "MapRange", "Map", "Append", "Prepend", "Concatenate", "Repeat",
    };

    private readonly Compilation _compilation;

    public TirArrayIdentity(Compilation compilation) => _compilation = compilation;

    private bool DeclaredIn(FunctionDef function, string library)
        => function?.OwnerType != null && function.OwnerType.Name == library
           && _compilation.LibraryDefinitionsByName.TryGetValue(library, out var def)
           && ReferenceEquals(def, function.OwnerType);

    /// <summary>The foundation's <c>SetAt(Array, Integer, value)</c>, whose receiver is an array.</summary>
    public bool IsSetAt(TirCall call)
        => call.Name == "SetAt" && call.Args.Count == 3 && call.Callee is { Name: "SetAt" } callee
           && callee.Parameters.Count == 3 && DeclaredIn(callee, "Primitives")
           && TirRewrite.IsListType(TirRewrite.StripCoerce(call.Args[0])?.Type);

    /// <summary>A call to an array producer of the foundation that always builds a new array:
    /// each is a <c>MapRange</c> in its Plato body, and <c>MapRange</c> is the one constructor.</summary>
    public bool IsFreshProducer(TirCall call)
        => call.Name != null && FreshProducers.Contains(call.Name) && call.Callee is { } callee
           && TirRewrite.IsListType(call.Type)
           && (callee.Name == "MapRange"
               ? TirIntrinsicIdentity.IsIntrinsic(callee, _compilation, "MapRange")
               : callee.Name == call.Name && DeclaredIn(callee, "Primitives"));

    /// <summary>The intrinsic <c>Count</c> or <c>At</c> on an array: reads the array and keeps no reference to it.</summary>
    public bool IsObserver(TirCall call)
        => call.Callee is { } callee && callee.Name is "Count" or "At"
           && TirIntrinsicIdentity.IsIntrinsic(callee, _compilation, callee.Name);
}

/// <summary>
/// The analysis behind <see cref="TirInPlaceUpdates"/>. One post-order walk numbers every node with a
/// time (children before the node, in <see cref="TirNode.Children"/> order, which is also the order C#
/// evaluates them), notes the loops, statements and lambdas around it, and records for each array
/// variable its declaration, its assignments and every use, classified as a borrow (the intrinsic
/// <c>Count</c> and <c>At</c>, and the source of a lowered loop: they read and keep nothing), an update
/// (the receiver of a <c>SetAt</c>) or an escape (anything else: the value may then be kept by
/// whoever received it). <see cref="CanWriteInPlace"/> applies the rule written in the README.
/// </summary>
public sealed class TirUpdateFlow
{
    /// <summary>The time span of one statement; a use is "after" it when its time exceeds <see cref="End"/>.</summary>
    public sealed class Interval { public int Start; public int End; }

    public enum UseKind { Borrow, Update, Escape }

    public sealed class Use
    {
        public UseKind Kind;
        public int Leaf;        // time of the variable reference itself
        public int Consumer;    // time of the node that takes the value
        public int[] Loops;
        public Site Site;       // for Update: the SetAt call
    }

    public sealed class Site
    {
        public int Index;               // position among all SetAt calls in post-order
        public int Time;
        public VariableDef Var;         // the receiver when it is a local variable
        public bool FreshReceiver;      // the receiver is a new array nothing else names
        public int[] Loops;
        public Interval Statement;
        public bool Threaded;           // the statement is Var = Var.SetAt(...)
        public bool Rejected;           // inside a lambda or an unrecognised construct
    }

    private sealed class Variable
    {
        public int Declarations;
        public bool DeclaredFresh;
        public int[] DeclaredLoops = Array.Empty<int>();
        public bool KeptByNonFresh;     // assigned something that is not provably a new array
        public bool Captured;           // referenced inside a lambda that is not a loop callback
        public readonly List<Use> Uses = new();
    }

    private readonly TirArrayIdentity _identity;
    private readonly Dictionary<VariableDef, Variable> _variables = new();
    private readonly HashSet<string> _freshTemps = new(StringComparer.Ordinal);
    private readonly List<int> _loops = new();
    private readonly Stack<Interval> _statements = new();
    private int _time, _nextLoop, _lambdaDepth, _captureDepth, _opaqueDepth;

    public List<Site> Sites { get; } = new();

    private TirUpdateFlow(TirArrayIdentity identity) => _identity = identity;

    public static TirUpdateFlow Analyze(TirNode body, TirArrayIdentity identity)
    {
        var flow = new TirUpdateFlow(identity);
        flow.FindFreshTemps(body);
        flow.Visit(body, true);
        if (TirRewrite.StripCoerce(body) is TirVariable { Def: { } def })
            flow.Get(def).Uses.Add(new Use { Kind = UseKind.Escape, Leaf = flow._time, Consumer = flow._time, Loops = Array.Empty<int>() });
        return flow;
    }

    /// <summary>The temporaries of lowered array loops that are referenced exactly once: the array they
    /// name is new and one place holds it.</summary>
    private void FindFreshTemps(TirNode body)
    {
        var producing = new HashSet<string>(StringComparer.Ordinal);
        var references = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var n in body.Descendants())
        {
            if (n is TirLoweredLoop ll && TirLoopLowerer.ProducesArray(ll.Kind))
                producing.Add(ll.TempName);
            else if (n is TirTempRef tr)
                references[tr.Name] = references.TryGetValue(tr.Name, out var c) ? c + 1 : 1;
        }
        foreach (var name in producing)
            if (references.TryGetValue(name, out var count) && count == 1)
                _freshTemps.Add(name);
    }

    /// <summary>Whether the value of <paramref name="e"/> is a new array that no other name or field holds.
    /// A <c>SetAt</c> result counts: it is either a copy or the receiver the pass proved dead.</summary>
    private bool IsFresh(TirNode e)
    {
        e = TirRewrite.StripCoerce(e);
        return e switch
        {
            TirArray => true,
            TirTempRef t => _freshTemps.Contains(t.Name),
            TirCall c => TirRewrite.IsListType(c.Type) && (_identity.IsSetAt(c) || _identity.IsFreshProducer(c)),
            _ => false,
        };
    }

    private Variable Get(VariableDef def)
    {
        if (!_variables.TryGetValue(def, out var v))
            _variables[def] = v = new Variable();
        return v;
    }

    private void EnterLoop() => _loops.Add(_nextLoop++);
    private void ExitLoop() => _loops.RemoveAt(_loops.Count - 1);

    private static bool IsTransparent(TirNode n) => n is TirCoerce;

    private bool IsKnown(TirNode n)
        => n is TirCall or TirCoerce or TirConditional or TirNew or TirArray or TirInvoke or TirLiteral
            or TirParameter or TirVariable or TirTypeRef or TirDefault or TirName or TirComponentAccess
            or TirConstructorCall or TirBooleanChain or TirTempRef or TirReturn or TirBlock or TirIf
            or TirLet or TirInlineBinding or TirAssign or TirLoop or TirLambda or TirLoweredLoop or TirInPlaceSet;

    private int Visit(TirNode n, bool statement)
    {
        if (n == null)
            return _time;
        Interval interval = null;
        if (statement)
        {
            interval = new Interval { Start = _time };
            _statements.Push(interval);
        }
        var pending = new List<Use>();
        var opaque = !IsKnown(n);
        if (opaque)
        {
            _opaqueDepth++;
            EnterLoop();
        }

        switch (n)
        {
            case TirCoerce coerce:
                return VisitTransparent(coerce, interval);

            case TirVariable v:
                if (v.Def != null && _captureDepth > 0)
                    Get(v.Def).Captured = true;
                break;

            case TirBlock block:
                foreach (var child in block.Statements)
                    VisitChild(n, 0, child, true, pending);
                break;

            case TirIf iff:
                VisitChild(n, 0, iff.Condition, false, pending);
                VisitChild(n, 1, iff.IfTrue, true, pending);
                VisitChild(n, 2, iff.IfFalse, true, pending);
                break;

            case TirLoop loop:
                EnterLoop();
                VisitChild(n, 0, loop.Condition, false, pending);
                VisitChild(n, 1, loop.Body, true, pending);
                ExitLoop();
                break;

            case TirLambda lambda:
                _lambdaDepth++;
                _captureDepth++;
                EnterLoop();
                VisitChild(n, 0, lambda.Body, true, pending);
                ExitLoop();
                _captureDepth--;
                _lambdaDepth--;
                break;

            case TirLoweredLoop lowered:
                VisitLowered(lowered, pending);
                break;

            case TirLet let:
                VisitChild(n, 0, let.Value, false, pending);
                Declare(let.Def, let.Value);
                break;

            case TirInlineBinding binding:
                // The loop lowerer binds each computed argument to a typed local before the call.
                VisitChild(n, 0, binding.Value, false, pending);
                Declare(binding.Def, binding.Value);
                break;

            case TirAssign assign:
                VisitAssign(assign, statement, pending);
                break;

            default:
            {
                var slot = 0;
                foreach (var child in n.Children)
                    VisitChild(n, slot++, child, false, pending);
                break;
            }
        }

        var time = ++_time;
        foreach (var use in pending)
            use.Consumer = time;
        if (n is TirCall call && _identity.IsSetAt(call))
            AddSite(call, time, pending);
        if (opaque)
        {
            ExitLoop();
            _opaqueDepth--;
        }
        if (statement)
        {
            interval.End = _time;
            _statements.Pop();
        }
        return time;
    }

    private void Declare(VariableDef def, TirNode value)
    {
        if (def == null)
            return;
        var variable = Get(def);
        variable.Declarations++;
        variable.DeclaredFresh = IsFresh(value);
        variable.DeclaredLoops = _loops.ToArray();
    }

    /// <summary>A coercion is not a consumer: the variable below it is used by whoever the coercion feeds.</summary>
    private int VisitTransparent(TirCoerce coerce, Interval interval)
    {
        var time = Visit(coerce.Inner, false);
        if (interval != null)
        {
            interval.End = _time;
            _statements.Pop();
        }
        return time;
    }

    private void VisitAssign(TirAssign assign, bool statement, List<Use> pending)
    {
        if (TirRewrite.StripCoerce(assign.LValue) is TirVariable { Def: { } target })
        {
            Visit(assign.LValue, false);
            var before = Sites.Count;
            VisitChild(assign, 1, assign.RValue, false, pending);
            var variable = Get(target);
            if (!IsFresh(assign.RValue))
                variable.KeptByNonFresh = true;
            if (statement && Sites.Count > before && TirRewrite.StripCoerce(assign.RValue) is TirCall call
                && _identity.IsSetAt(call) && Sites[^1].Var == target)
                Sites[^1].Threaded = true;
        }
        else
        {
            var slot = 0;
            foreach (var child in assign.Children)
                VisitChild(assign, slot++, child, false, pending);
        }
    }

    private void VisitLowered(TirLoweredLoop ll, List<Use> pending)
    {
        var children = ll.Children.ToList();
        var sources = ll.Sources.Count;
        var fnPresent = ll.Fn != null;
        var seedPresent = ll.Seed != null;
        var firstPresent = ll.IncludeFirst != null;
        var callbacksFrom = sources + (fnPresent ? 1 : 0) + (seedPresent ? 1 : 0) + (firstPresent ? 1 : 0);
        for (var i = 0; i < children.Count; i++)
        {
            var isFn = fnPresent && i == sources;
            var isStage = i >= callbacksFrom;
            if (i < sources)
                VisitChild(ll, i, children[i], false, pending, UseKind.Borrow);
            else if ((isFn || isStage) && TirRewrite.StripCoerce(children[i]) is TirLambda callback)
                VisitCallback(callback);
            else
                VisitChild(ll, i, children[i], false, pending);
        }
    }

    /// <summary>A lambda literal given to a lowered loop is written into the loop body and runs there:
    /// it is code in a loop, not a closure that can outlive the statement.</summary>
    private void VisitCallback(TirLambda callback)
    {
        _lambdaDepth++;
        EnterLoop();
        var pending = new List<Use>();
        VisitChild(callback, 0, callback.Body, true, pending);
        ExitLoop();
        _lambdaDepth--;
        _time++;
    }

    private int VisitChild(TirNode parent, int slot, TirNode child, bool statement, List<Use> pending,
        UseKind? forced = null)
    {
        var leaf = Visit(child, statement);
        if (TirRewrite.StripCoerce(child) is TirVariable { Def: { } def })
        {
            var use = new Use { Kind = forced ?? Classify(parent, slot), Leaf = leaf, Loops = _loops.ToArray() };
            Get(def).Uses.Add(use);
            pending.Add(use);
        }
        return leaf;
    }

    private UseKind Classify(TirNode parent, int slot)
    {
        if (parent is TirCall call && slot == 0)
        {
            if (_identity.IsSetAt(call))
                return UseKind.Update;
            if (_identity.IsObserver(call))
                return UseKind.Borrow;
        }
        return UseKind.Escape;
    }

    private void AddSite(TirCall call, int time, List<Use> pending)
    {
        var receiver = TirRewrite.StripCoerce(call.Args[0]);
        var site = new Site
        {
            Index = Sites.Count,
            Time = time,
            Var = (receiver as TirVariable)?.Def,
            FreshReceiver = receiver is not TirVariable && IsFresh(receiver),
            Loops = _loops.ToArray(),
            Statement = _statements.Count > 0 ? _statements.Peek() : new Interval { Start = 0, End = int.MaxValue },
            Rejected = _lambdaDepth > 0 || _opaqueDepth > 0,
        };
        foreach (var use in pending)
            if (use.Kind == UseKind.Update && use.Site == null)
                use.Site = site;
        Sites.Add(site);
    }

    /// <summary>The rule of the README, clause by clause.</summary>
    public bool CanWriteInPlace(Site site)
    {
        if (site.Rejected)
            return false;
        if (site.Var == null)
            return site.FreshReceiver;
        if (!_variables.TryGetValue(site.Var, out var v)
            || v.Declarations != 1 || !v.DeclaredFresh || v.KeptByNonFresh || v.Captured)
            return false;

        // A loop that holds the update but not the declaration runs the update again on the same
        // variable. That is only the next version of the value when the statement assigns the
        // result back (the threaded form); otherwise the second pass reads what the first changed.
        bool Separates(int[] loops) => loops.Any(l => site.Loops.Contains(l) && !v.DeclaredLoops.Contains(l));
        if (!site.Threaded && site.Loops.Any(l => !v.DeclaredLoops.Contains(l)))
            return false;

        foreach (var use in v.Uses)
        {
            if (ReferenceEquals(use.Site, site))
                continue;
            switch (use.Kind)
            {
                case UseKind.Borrow:
                case UseKind.Update:
                    // The old value is read after the write unless the update is threaded
                    // (then the variable already holds the new one) or the read is finished.
                    if (!site.Threaded && (use.Consumer > site.Time || Separates(use.Loops)))
                        return false;
                    break;
                case UseKind.Escape:
                    // Whoever got the array may keep it: nothing may write into it afterwards.
                    if (use.Leaf <= site.Statement.End || Separates(use.Loops))
                        return false;
                    break;
            }
        }
        return true;
    }
}
