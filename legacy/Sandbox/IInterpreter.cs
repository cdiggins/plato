using Plato;

// My idea:
// 1. Create a tokenizer
// 2. Create a parser 
// 3. Create a type-checker 
// 4. Create an evaluator 
// 5. Create an analyzer 
// 6. Create a translator 


namespace PlatoAbstractSyntax
{
    /// <summary>
    /// An interpreter converts an expression language into values 
    /// </summary>
    public interface IInterpreter<TValue, TExpression>
    {
        TValue GetResult(TExpression expression);
        IInterpreter<TValue, TExpression> Evaluate(TExpression expression);
    }

    /// <summary>
    /// An evaluator converts expressions into values, with a context (e.g. an environment) 
    /// </summary>
    public interface IEvaluator<TValue, in TExpression, TContext>
    {
        (TContext, TValue) Evaluate(TContext context, TExpression expression);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ITranslator
    {

    }

    public interface IParser<TToken, TParseNode>
    {
        ISequence<TParseNode> Parse(IArray<TToken> tokens);
    }

    public interface ITokenizer<TInputSymbol, TToken>
    {
        IArray<TToken> Tokenize(IArray<TInputSymbol> input);
    }
    
    public class Parser : IInterpreter<Parser.Node, Tokenizer.Token>
    {
        public class Node
        {
            public System.Collections.Generic.List<Node> Children { get; }
            public System.Collections.Generic.List<Tokenizer.Token> Tokens { get; }
            public string Name { get; }
        }

        public IStack<Node> Stack { get; }
        public Node GetResult(Tokenizer.Token expression)
        {
            throw new System.NotImplementedException();
        }

        public IInterpreter<Node, Tokenizer.Token> Evaluate(Tokenizer.Token expression)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Tokenizer : IInterpreter<Tokenizer.Token, Tokenizer.Cursor>
    {
        public class Token
        {
            public int Pos { get; }
            public int Length { get; }
            public string Type { get; }
        }

        public class Cursor
        {
            public int Pos { get; }
        }

        public string Input { get; }
        public System.Collections.Generic.List<Token> Tokens { get; }
        public Cursor Current { get; }

        public int PositionIndexToTokenIndex(int n)
        {
            for (var i = 0; i < Tokens.Count; i++)
            {
                var token = Tokens[i];
                if (token.Pos > n) return i - 1;
            }
            return Tokens.Count - 1;
        }

        public Token GetResult(Cursor expression)
        {
            var index = PositionIndexToTokenIndex(expression.Pos);
            if (index < 0) return null;
            return Tokens[index];
        }

        public IInterpreter<Token, Cursor> Evaluate(Cursor expression)
        {
            // Advance the tokenizer according to the grammar rules
            // Advances potentially multiple spaces. 
            // Make sure the input is the next spot. 
            throw new System.NotImplementedException();
        }

        public string TokenToString(Token token)
            => Input.Substring(token.Pos, token.Length);
    }

    public class Evaluator<ValueT, ExpressionT>
    {
        public ValueT Eval(ExpressionT expression)
        {
            throw new System.NotImplementedException();
        }
    }
}