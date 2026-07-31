using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Vsg
{
    public class VisualSyntaxGraph
    {
        public string Name { get; set; }
        public Dictionary<Guid, VsgConnection> Connections { get; set; }
        public Dictionary<Guid, VsgNode> Nodes { get; set; }

        public VisualSyntaxGraph() { }
        public VisualSyntaxGraph(string name, IEnumerable<VsgNode> nodes, IEnumerable<VsgConnection> connections)
        {
            Name = name;
            Nodes = nodes.ToDictionary(n => n.Id, n => n);
            Connections = connections.ToDictionary(c => c.Id, c => c);
        }
    }

    public class VsgLayout
    {
        public Dictionary<Guid, SizeAndPos> NodePositions { get; set; }
    }

    public class SizeAndPos
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class VsgConnection
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid Source { get; }
        public Guid Destination { get; }
        public string Type { get; set; }
        public string Kind { get; set; }
        public string Label { get; set; }

        public VsgConnection()
        {
        }

        public VsgConnection(Guid src, Guid dest)
        {
            Source = src;
            Destination = dest;
        }
    }

    public class VsgSocket
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Label { get; }
        public string Type { get; }
        public VsgSocket() : this("") { }
        public VsgSocket(string label, string type = "Any")
        {
            Label = label;
            Type = type;
        }
    }

    public class VsgNode
    {
        public VsgNode() : this("") { }
        public VsgNode(string label, string kind = "Function")
        {
            Label = label;
            Kind = kind;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string Label { get; }
        public string Kind { get; }

        public List<VsgSocket> Inputs { get; } = new List<VsgSocket>();
        public List<VsgSocket> Outputs { get; } = new List<VsgSocket>();

        public VsgSocket MainOutput => Outputs.Count > 0 ? Outputs[0] : null;

        public VsgSocket CreateInputSocket(string name)
        {
            var input = new VsgSocket(name);
            Inputs.Add(input);
            return input;
        }
    }

    public class VsgBuilder
    {
        public Dictionary<ParameterDef, VsgSocket> Parameters { get; }
            = new Dictionary<ParameterDef, VsgSocket>();

        public List<VsgNode> Nodes { get; } = new List<VsgNode>();
        public List<VsgConnection> Connections { get; } = new List<VsgConnection>();

        public VisualSyntaxGraph ToVsg(FunctionDef def)
        {
            // Input nodes.
            var input = CreateNode("Inputs", false);
            foreach (var p in def.Parameters)
            {
                var socket = new VsgSocket(p.Name, p.Type.Name);
                input.Outputs.Add(socket);
                Parameters.Add(p, socket);
            }

            var output = CreateNode("Output", false);
            var finalSocket = new VsgSocket("Result", def.Type.Name);
            output.Inputs.Add(finalSocket);
            Connect(GetSocket(def.Body), finalSocket);
            return new VisualSyntaxGraph(def.Name, Nodes, Connections);
        }

        public VsgNode CreateNode(DefSymbol symbol)
        {
            if (symbol == null)
                return null;

            switch (symbol)
            {
                case FieldDef fieldDefSymbol:
                    return CreateNode(fieldDefSymbol.Function);

                case FunctionGroupDef functionGroupSymbol:
                    // TODO: remove function groups 
                    return CreateNode(functionGroupSymbol.Functions[0]);

                case FunctionDef functionSymbol:
                    return CreateNode(functionSymbol);

                case MethodDef methodDefSymbol:
                    return CreateNode(methodDefSymbol.Function);

                case ParameterDef parameterSymbol:
                    return CreateNode($"{parameterSymbol.Name} as Function");

                case VariableDef variableSymbol:
                    break;
            }

            throw new NotImplementedException();
        }

        public VsgNode CreateNode(Symbol symbol)
        {
            if (symbol == null)
                return null;

            switch (symbol)
            {
               case Assignment assignmentSymbol:
                    break;

                case ConditionalExpression conditionalExpressionSymbol:
                    return CreateNode(conditionalExpressionSymbol);

                case FunctionCall functionCallSymbol:
                    return CreateNode(functionCallSymbol);

                case Literal literalSymbol:
                    return CreateNode(literalSymbol);

                case RefSymbol refSymbol:
                    return CreateNode(refSymbol.Def);

                case Lambda lambdaSymbol:
                    return CreateNode(lambdaSymbol);

                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }

            throw new NotImplementedException();
        }

        public VsgNode CreateNode(Lambda lambda)
        {
            // TODO: implement lambdas
            return null;
        }

        public VsgNode CreateNode(string name, bool hasOutput = true)
        {
            var node = new VsgNode(name);
            if (hasOutput)
                node.Outputs.Add(new VsgSocket("Out"));
            Nodes.Add(node);
            return node;
        }

        public VsgSocket GetSocket(DefSymbol def)
        {
            switch (def)
            {
                case ParameterDef parameterSymbol:
                    return Parameters[parameterSymbol];
            }
            return CreateNode(def)?.MainOutput;
        }

        public VsgSocket GetSocket(Symbol expr)
        {
            switch (expr)
            {
                case RefSymbol refSymbol:
                    return GetSocket(refSymbol?.Def);
            }
            return CreateNode(expr)?.MainOutput;
        }

        public VsgNode CreateNode(Literal lit)
            => CreateNode(lit.Value.ToLiteralString());

        public VsgNode CreateNode(ConditionalExpression symbol)
        {
            var r = CreateNode("If");
            var conditionSocket = r.CreateInputSocket("Condition");
            var ifTrueSocket = r.CreateInputSocket("If true");
            var ifFalseSocket = r.CreateInputSocket("If false");
            Connect(GetSocket(symbol.Condition), conditionSocket);
            Connect(GetSocket(symbol.IfTrue), ifTrueSocket);
            Connect(GetSocket(symbol.IfFalse), ifFalseSocket);
            return r;
        }

        public VsgNode CreateNode(FunctionCall symbol)
        {
            if (symbol.Function is RefSymbol rs && rs.Def is ParameterDef ps)
            {
                var f = CreateNode("Lambda");
                var inputs = symbol.Args.Select(GetSocket).ToList();
                for (var i = 0; i < inputs.Count; ++i)
                {
                    f.Inputs.Add(new VsgSocket($"Input{i}"));
                    Connect(inputs[i], f.Inputs[i]);
                }

                return f;
            }
            else
            {
                var f = CreateNode(symbol.Function);
                if (f == null)
                    f = CreateNode("Unknown");
                var inputSockets = symbol.Args.Select(GetSocket).ToList();

                while (f.Inputs.Count < inputSockets.Count)
                    f.Inputs.Add(new VsgSocket($"Input {f.Inputs.Count}"));

                for (var i = 0; i < inputSockets.Count; ++i)
                {
                    Connect(inputSockets[i], f.Inputs[i]);
                }

                return f;
            }
        }

        public VsgConnection Connect(VsgSocket source, VsgSocket target)
        {
            if (source == null || target == null)
                return null;
            var r = new VsgConnection(source.Id, target.Id);
            Connections.Add(r);
            return r;
        }

        public VsgNode CreateNode(FunctionDef def)
        {
            var r = CreateNode(def.Name);
            foreach (var p in def.Parameters)
            {
                r.CreateInputSocket(p.Name);
            }

            return r;
        }
    }
}