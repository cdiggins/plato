using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plato.Compiler
{
    public class VisualSyntaxGraph
    {
        public string Name { get; }
        public Dictionary<Guid, VsgConnection> Connections { get; }
        public Dictionary<Guid, VsgNode> Nodes { get; }

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
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid Source { get; set; }
        public Guid Destination { get; set; }
        public string Type { get; set; }
        public string Kind { get; set; }
        public string Label { get; set; }

        public VsgConnection(Guid src, Guid dest)
        {
            Source = src;
            Destination = dest;
        }
    }

    public class VsgSocket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Label { get; set; }
        public string Type { get; set; } 
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

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Label { get; set; }
        public string Kind { get; set; }

        public List<VsgSocket> Inputs { get; set; } = new List<VsgSocket>();
        public List<VsgSocket> Outputs { get; set; } = new List<VsgSocket>();

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
        public Dictionary<ParameterSymbol, VsgSocket> Parameters { get; } 
            = new Dictionary<ParameterSymbol, VsgSocket>();

        public List<VsgNode> Nodes { get; } = new List<VsgNode>();
        public List<VsgConnection> Connections { get; } = new List<VsgConnection>();

        public VisualSyntaxGraph ToVsg(FunctionSymbol symbol)
        {
            // Input nodes.
            var input = CreateNode("Inputs", false);
            foreach (var p in symbol.Parameters)
            {
                var socket = new VsgSocket(p.Name, p.Type.Name);
                input.Outputs.Add(socket);
                Parameters.Add(p, socket);
            }

            var output = CreateNode("Output", false);
            var finalSocket = new VsgSocket("Result", symbol.Type.Name);
            output.Inputs.Add(finalSocket);
            Connect(GetSocket(symbol.Body), finalSocket);
            return new VisualSyntaxGraph(symbol.Name, Nodes, Connections);
        }

        public VsgNode CreateNode(Symbol symbol)
        {
            if (symbol == null)
                return null;

            switch (symbol)
            {
                case ArgumentSymbol argumentSymbol:
                    return CreateNode(argumentSymbol.Original);

                case AssignmentSymbol assignmentSymbol:
                    break;

                case ConditionalExpressionSymbol conditionalExpressionSymbol:
                    return CreateNode(conditionalExpressionSymbol);
                
                case FieldDefSymbol fieldDefSymbol:
                    return CreateNode(fieldDefSymbol.Function);

                case FunctionGroupSymbol functionGroupSymbol:
                    // TODO: remove function groups 
                    return CreateNode(functionGroupSymbol.Functions[0]);
                
                case FunctionSymbol functionSymbol:
                    return CreateNode(functionSymbol);
                
                case MethodDefSymbol methodDefSymbol:
                    return CreateNode(methodDefSymbol.Function);

                case ParameterSymbol parameterSymbol:
                    throw new Exception("Internal error: should have been created by the function");
                
                case PredefinedSymbol predefinedSymbol:
                    break;

                case TypeParameterDefSymbol typeParameterDefSymbol:
                    break;
                
                case TypeDefSymbol typeDefSymbol:
                    return CreateNode(typeDefSymbol.Name);
                
                case VariableSymbol variableSymbol:
                    break;
                
                case DefSymbol defSymbol:
                    break;

                case FunctionCallSymbol functionCallSymbol:
                    return CreateNode(functionCallSymbol);

                case LiteralSymbol literalSymbol:
                    return CreateNode(literalSymbol);

                case NoValueSymbol noValueSymbol:
                    break;
                
                case RefSymbol refSymbol:
                    return CreateNode(refSymbol.Def);

                case TypeRefSymbol typeRefSymbol:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }

            throw new NotImplementedException();
        }

        public VsgNode CreateNode(string name, bool hasOutput = true)
        {
            var node = new VsgNode(name);
            if (hasOutput)
                node.Outputs.Add(new VsgSocket("Out"));
            Nodes.Add(node);
            return node;
        }

        public VsgSocket GetSocket(Symbol symbol)
        {
            switch (symbol)
            {
                case ArgumentSymbol argumentSymbol:
                    return GetSocket(argumentSymbol.Original);
                case RefSymbol refSymbol:
                    return GetSocket(refSymbol?.Def);
                case ParameterSymbol parameterSymbol:
                    return Parameters[parameterSymbol];
            }
            return CreateNode(symbol)?.MainOutput;
        }

        public VsgNode CreateNode(LiteralSymbol lit)
            => CreateNode(lit.Value.ToLiteralString());

        public VsgNode CreateNode(ConditionalExpressionSymbol symbol)
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

        public VsgNode CreateNode(FunctionCallSymbol symbol)
        {
            if (symbol.Function is RefSymbol rs && rs.Def is ParameterSymbol ps)
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
                var inputs = symbol.Args.Select(GetSocket).ToList();
                
                // TODO: this should not happen when the symbol resolver is working properly
                //Debug.Assert(inputs.Count == f.Inputs.Count);
                
                for (var i = 0; i < Math.Min(inputs.Count, f.Inputs.Count); ++i)
                {
                    Connect(inputs[i], f.Inputs[i]);
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

        public VsgNode CreateNode(FunctionSymbol symbol)
        {
            var r = CreateNode(symbol.Name);
            foreach (var p in symbol.Parameters)
            {
                r.CreateInputSocket(p.Name);
            }

            return r;
        }
    }
}