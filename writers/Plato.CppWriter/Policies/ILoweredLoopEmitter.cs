// How one kind of lowered loop is written (the loop-emission registry).
using System;
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Writes the statements of a <see cref="TirLoweredLoop"/> of one or more kinds. The body writer has
    /// already written each source into <see cref="LoopEmission.SourceNames"/>; the emitter declares the
    /// loop's counts and result, and afterwards the body writer wraps an array result with
    /// <c>make_array</c>. A new loop kind is one new emitter added to <see cref="LoweredLoopEmitters"/>.
    /// </summary>
    public interface ILoweredLoopEmitter
    {
        IReadOnlyList<string> Kinds { get; }
        void Emit(LoopEmission e);
    }

    /// <summary>What an emitter may use from the body writer while it writes one loop.</summary>
    public sealed class LoopEmission
    {
        public TirLoweredLoop Loop { get; init; }
        public CppWriter Writer { get; init; }
        public IReadOnlyList<string> SourceNames { get; init; }

        /// <summary>Loop-local names: element count, index and result buffer.</summary>
        public string Count { get; init; }
        public string Index { get; init; }
        public string Buffer { get; init; }
        /// <summary>The name of a non-array result (a reduction's accumulator, a flag).</summary>
        public string Temp { get; init; }
        /// <summary>The count of the source after its stages, or of the index range.</summary>
        public string SourceCount { get; init; }

        /// <summary>The count <c>make_array</c> receives; an emitter that sizes the result differently sets it.</summary>
        public string ArrayCount { get; set; }
        /// <summary>The array <c>make_array</c> wraps; an emitter that fills another one sets it.</summary>
        public string ResultArray { get; set; }

        public Action<string> Write { get; init; }
        public Action<string> WriteLine { get; init; }
        public Action<TirNode> WriteNode { get; init; }
        public Action StartBlock { get; init; }
        public Action EndBlock { get; init; }

        /// <summary>The C++ element type of the result; throws when the loop has none.</summary>
        public Func<string> ElementType { get; init; }
        /// <summary>Writes the source's stages and returns the name of the element they produce.</summary>
        public Func<string> BaseElement { get; init; }
        /// <summary>Writes a <c>for</c> loop over the index with the given count, its body written by the action.</summary>
        public Action<string, Action> For { get; init; }
        /// <summary>Binds the callback's parameters to the given element expressions.</summary>
        public Action<string[]> DeclareParameters { get; init; }
        /// <summary>Writes the callback applied to the given element expressions.</summary>
        public Action<string[]> WriteCallback { get; init; }
        /// <summary>The declaration keyword and loop-local name of a callback parameter.</summary>
        public Func<ParameterDef, string, string> ElementBinding { get; init; }
        public Func<ParameterDef, string> ParameterName { get; init; }

        /// <summary>Allocates the result with <see cref="ArrayCount"/> elements and fills each with the callback applied to the expressions.</summary>
        public void FillArray(Func<string[]> elements)
        {
            WriteLine($"{ElementType()}* {Buffer} = plato::arena_alloc<{ElementType()}>({ArrayCount});");
            For(ArrayCount, () =>
            {
                var expressions = elements();
                DeclareParameters(expressions);
                Write($"{Buffer}[{Index}] = ");
                WriteCallback(expressions);
                WriteLine(";");
            });
        }

        public string SourceCountOf(int source) => $"{SourceNames[source]}.count";
    }

    /// <summary>The emitters by loop kind.</summary>
    public sealed class LoweredLoopEmitters
    {
        private readonly Dictionary<string, ILoweredLoopEmitter> _byKind = new();

        public LoweredLoopEmitters Add(ILoweredLoopEmitter emitter)
        {
            foreach (var kind in emitter.Kinds)
                _byKind.Add(kind, emitter);
            return this;
        }

        public bool TryGet(string kind, out ILoweredLoopEmitter emitter) => _byKind.TryGetValue(kind, out emitter);
    }
}
