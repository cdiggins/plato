using System.Linq;

namespace PlatoParser
{
    // This is currently a parser for a subset of the C# language 
    // See: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure
    // The goal is to separate this into a C# and A Plato grammar.
    // First step though is to test this on my own source code. 

    public class CSharpGrammar : Grammar
    {
        public CSharpGrammar()
            => WhitespaceRule = WS;

        // Helper functions 
        public Rule List(Rule r, Rule sep = null) => (r + WS + ((sep ?? Comma) + r + WS).ZeroOrMore()).Optional();
        public Rule Delimited(Rule first, Rule middle, Rule last) => first + middle + last;
        public Rule Parenthesized(Rule r) => Delimited(Symbol("("), r, Symbol(")"));
        public Rule ParenthesizedList(Rule r, Rule sep = null) => Parenthesized(List(r, sep));
        public Rule Bracketed(Rule r) => Delimited(Symbol("["), r, Symbol("]"));
        public Rule BracketedList(Rule r, Rule sep = null) => Bracketed(List(r, sep));
        public Rule Keyword(string s) => s + IdentifierChar.NotAt() + WS;

        // TODO: make sure that it isn't part of a longer symbol
        public Rule Symbol(string s) => s + WS; 
        public Rule UntilPast(Rule r) => RepeatUntilPast(AnyChar, r);
        public Rule RepeatUntilPast(Rule repeat, Rule delimiter) => delimiter.NotAt().Then(repeat).ZeroOrMore().Then(delimiter);
        public Rule Symbols(params string[] strings) => Choice(strings.OrderByDescending(x => x.Length).Select(Symbol).ToArray());
        public Rule Keywords(params string[] strings) => Choice(strings.OrderByDescending(x => x.Length).Select(Keyword).ToArray());
        public Rule Braced(Rule r) => Delimited(Symbol("{"), r, Symbol("}"));
        public Rule BracedList(Rule r, Rule sep = null) => Delimited(Symbol("{"), List(r, sep), Symbol("}"));
        public Rule AngledBracketList(Rule r, Rule sep = null) => Delimited(Symbol("<"), List(r, sep), Symbol(">"));

        // Basic 
        public Rule Comma => Token(Symbol(","));
        public Rule AnyChar => Token(AnyCharRule.Default);
        public Rule LowerCaseLetter => Token('a'.To('z'));
        public Rule UpperCaseLetter => Token('A'.To('Z'));
        public Rule Letter => Token(LowerCaseLetter | UpperCaseLetter);
        public Rule Digit => Token('0'.To('9'));
        public Rule DigitOrLetter => Token(Letter | Digit);
        public Rule IdentifierFirstChar => Token('_' | Letter);
        public Rule IdentifierChar => Token(IdentifierFirstChar | Digit);
        public Rule FractionalPart => Token("." + Digits.Optional());
        public Rule HexDigit => Token(Digit | 'a'.To('f') | 'A'.To('F'));
        public Rule BinDigit => Token('0'.To('1'));
        public Rule IntegerSuffix => Token(new[] { "ul", "UL", "u", "U", "l", "L", "lu", "lU", "Lu", "LU" });
        public Rule FloatSuffix => Token("fFdDmM".ToCharSetRule());
        public Rule Sign => Token("+-".ToCharSetRule());
        public Rule ExponentPart => Token("eE".ToCharSetRule() + Sign.Optional() + Digits);
        public Rule Digits => Token(Digit.OneOrMore());
        public Rule SpaceChars => Token(" \t\n\r\0\v\f".ToCharSetRule());
        public Rule Spaces => Token(SpaceChars.OneOrMore());
        public Rule NewLine => Token(new[] { "\r\n", "\n" });
        public Rule UntilNextLine => Token(AnyChar.Except(NewLine).ZeroOrMore().Then(NewLine.Optional()));        
        public Rule SingleLineComment => Token("//" + UntilNextLine);
        public Rule BlockComment => Token("/*" + UntilPast("*/"));
        public Rule Comment => Token(SingleLineComment | BlockComment);
        public Rule WS => Token((Spaces | Comment).ZeroOrMore());

        // Literals 
        public Rule EscapedLiteralChar => Token('\\' + AnyChar); // TODO: handle special codes like \u codes and \x
        public Rule StringLiteralChar => Token(EscapedLiteralChar | AnyChar.Except('"'));
        public Rule CharLiteralChar => Token(EscapedLiteralChar | AnyChar.Except('\''));

        public Rule FloatLiteral => Phrase(Digits + FractionalPart.Optional() + ExponentPart.Optional() + FloatSuffix.Optional());
        public Rule HexLiteral => Phrase(new[] { "0x", "0X" } + HexDigit.OneOrMore() + IntegerSuffix.Optional());
        public Rule BinaryLiteral => Phrase("0b" | "0B" + BinDigit.OneOrMore() + IntegerSuffix.Optional());
        public Rule IntegerLiteral => Phrase(Digits.ThenNot(".fFdDmM".ToCharSetRule()) + IntegerSuffix.Optional());
        public Rule StringLiteral => Phrase('"' + StringLiteralChar.ZeroOrMore() + '"');
        public Rule CharLiteral => Phrase('\'' + CharLiteralChar + '\'');
        public Rule BooleanLiteral => Phrase(Keyword("true") | Keyword("false"));
        public Rule NullLiteral => Phrase(Keyword("null"));
        public Rule ValueLiteral => Phrase(Keyword("value"));

        public Rule Literal => Phrase(
            HexLiteral
            | BinaryLiteral
            | FloatLiteral
            | IntegerLiteral
            | StringLiteral
            | CharLiteral
            | BooleanLiteral
            | NullLiteral
            | ValueLiteral);

        // Operators 
        public Rule BinaryOperator => Phrase(Symbols(
            "+", "-", "*", "/", "%", ">>>", ">>", "<<", "&&", "||", "&", "|", "^",
            "+=", "-=", "*=", "/=", "%=", ">>>=", ">>=", "<<=", "&&=", "||=", "&=", "|=", "^=",
            "=", "<", ">", "<=", "=>", "==", "!=",
            "??", "?="
            ));

        public Rule UnaryOperator => Phrase(Symbols("++", "--", "!", "-", "+", "~"));
        public Rule Indexer => Phrase(Bracketed(Expression));
        public Rule OverloadableOperator => Phrase(Symbols("+", "-", "!", "~", "++", "--", "*", "/", "%", "&", "|", "^", "<<", ">>", ">>>", "==", "!=", "<", ">", "<=", ">="));
        
        public Rule PostfixOperator => Phrase(
            Symbols("!", "?", "++", "--")    
            | MemberAccess
            | ConditionalMemberAccess
            | FunctionArgs
            | Indexer
            | TernaryOperation
            | BinaryOperation
            | IsOperation
            | AsOperation
            );

        // Expressions 
        public Rule Identifier => Phrase(IdentifierFirstChar + IdentifierChar.ZeroOrMore());

        public Rule BinaryOperation => Phrase(BinaryOperator + Expression);
        public Rule TernaryOperation => Phrase(Symbol("?") + Expression + Symbol(":") + Expression);
        public Rule ParenthesizedExpression => Phrase(ParenthesizedList(Expression));

        public Rule ThrowExpression => Phrase(Keyword("throw") + Expression);
        public Rule LambdaParameter => Phrase((TypeExpr + Identifier) | Identifier);
        public Rule LambdaParameters => Phrase(LambdaParameter | ParenthesizedList(LambdaParameter));
        public Rule LambdaBody => Phrase(CompoundStatement | Expression);
        public Rule LambdaExpr => Phrase(LambdaParameters + Symbol("=>") + LambdaBody);
        public Rule MemberAccess => Phrase(Symbol(".") + Identifier);
        public Rule ConditionalMemberAccess => Phrase(Symbol("?.") + Identifier);
        public Rule TypeOf => Phrase(Keyword("typeof") + Parenthesized(TypeExpr));
        public Rule NameOf => Phrase(Keyword("nameof") + Parenthesized(Expression));
        public Rule Default => Phrase(Keyword("default") + Parenthesized(TypeExpr).Optional());
        public Rule InitializerClause => Phrase((Identifier + Symbol("=") + Expression) | Expression);
        public Rule Initializer => Phrase(BracedList(InitializerClause));
        public Rule ArraySizeSpecifier => Phrase(Bracketed(Expression));
        public Rule NewOperation => Phrase(Keyword("new") + TypeExpr + FunctionArgs.Optional() + ArraySizeSpecifier.Optional() + Initializer.Optional());
        public Rule IsOperation => Phrase(Keyword("is") + TypeExpr + Identifier.Optional());
        public Rule AsOperation => Phrase(Keyword("as") + TypeExpr + Identifier.Optional());
        public Rule StringInterpolationContent => Phrase(Braced(Expression) | StringLiteralChar);
        public Rule StringInterpolation => Phrase("$\"" + StringInterpolationContent.ZeroOrMore() + "\"");
        public Rule FunctionArgKeyword => Phrase(Keywords("ref", "out", "in", "params"));
        public Rule FunctionArg => Phrase(FunctionArgKeyword.ZeroOrMore() + Expression);
        public Rule FunctionArgs => Phrase(ParenthesizedList(FunctionArg));

        public Rule LeafExpression => Phrase( 
            LambdaExpr
            | ParenthesizedExpression
            | ThrowExpression
            | Literal
            | TypeOf
            | NameOf
            | Default
            | NewOperation
            | StringInterpolation
            | Identifier
            );

        public Rule Expression => Phrase(Recursive(() =>
            UnaryOperator.ZeroOrMore()
            + LeafExpression
            + PostfixOperator.ZeroOrMore())
            );

        // Statements 
        public Rule EOS => Token(Symbol(";"));
        public Rule ExpressionStatement => Phrase(Expression + EOS);
        public Rule ElseClause => Phrase(Keyword("else") + Statement);
        public Rule IfStatement => Phrase(Keyword("if") + ParenthesizedExpression + Statement + ElseClause.Optional());
        public Rule WhileStatement => Phrase(Keyword("while") + ParenthesizedExpression + Statement);
        public Rule DoWhileStatement => Phrase(Keyword("do") + Statement + Keyword("while") + ParenthesizedExpression + EOS);
        public Rule ReturnStatement => Phrase(Keyword("return") + Expression.Optional() + EOS);
        public Rule BreakStatement => Phrase(Keyword("break") + EOS);
        public Rule YieldStatement => Phrase(Keyword("yield") + Keyword("return") + Expression + EOS);
        public Rule YieldBreakStatement => Phrase(Keyword("yield") + Keyword("break") + EOS);
        public Rule ContinueStatement => Phrase(Keyword("continue") + EOS);

        public Rule CompoundStatement => Phrase(Braced(Statement.ZeroOrMore()));
        public Rule CatchClause => Phrase(Keyword("catch") + Parenthesized(VarDecl) + CompoundStatement);
        public Rule FinallyClause => Phrase(Keyword("finally") + CompoundStatement);
        public Rule CaseClause => Phrase((Keyword("default") | (Keyword("case") + Expression)).Then(Statement));
        public Rule SwitchStatement => Phrase(Keyword("switch") + Braced(CaseClause.ZeroOrMore()));
        public Rule TryStatement => Phrase(Keyword("try") + CompoundStatement + CatchClause.Optional() + FinallyClause.Optional());
        public Rule ForEachStatement => Phrase(Keyword("foreach") + Symbol("(") + VarDecl + Keyword("in") + Expression + Symbol(")") + Statement);

        public Rule InitializationClause => Phrase((VarDecl + Initialization).Optional());
        public Rule InvariantClause => Phrase(Expression.Optional());
        public Rule VariantClause => Phrase(Expression.Optional());
        public Rule ForStatement => Phrase(Keyword("for") + Symbol("(") + InitializationClause + EOS + InvariantClause + EOS + VariantClause + Symbol(")") + Statement);
        public Rule Initialization => Phrase((Symbol("=") + Expression).Optional());
        public Rule VarDecl => Phrase(TypeExpr + Identifier);
        public Rule VarDeclStatement => Phrase(VarDecl + Initialization + EOS);
        public Rule ThrowStatement => Phrase(Keyword("throw") + CompoundStatement +
            CatchClause.Optional() + FinallyClause.Optional());

        public Rule Statement => Phrase(Recursive(() => 
            EOS 
            | CompoundStatement
            | IfStatement 
            | WhileStatement 
            | DoWhileStatement 
            | ReturnStatement 
            | BreakStatement 
            | ContinueStatement 
            | ForStatement 
            | ForEachStatement
            | ThrowStatement
            | VarDeclStatement 
            | TryStatement
            | ExpressionStatement
        )); 

        public Rule QualifiedIdentifier => Phrase(List(Identifier, Symbol(".")));
        public Rule Static => Phrase(Keyword("static").Optional());
        public Rule UsingDirective => Phrase(Keyword("using") + Static + QualifiedIdentifier + EOS);

        public Rule Modifier => Phrase(Keywords("static", "sealed", "partial", "readonly", "const", "ref", "abstract", "virtual"));
        public Rule AccessSpecifier => Phrase(Keywords("public", "private", "protected", "internal"));
        public Rule ModifiersAndSpecifiers => (Modifier | AccessSpecifier).ZeroOrMore();
        public Rule Attribute => Phrase(Bracketed(List(Identifier + FunctionArgs.Optional())));
        public Rule AttributeList => Phrase(Attribute.ZeroOrMore());
        public Rule DeclarationPreamble => Phrase(AttributeList + ModifiersAndSpecifiers);
        public Rule TypeVariance => Phrase(Keywords("in", "out").Optional());
        public Rule TypeParameter => Phrase(TypeVariance + Identifier);
        public Rule TypeParameterList => Phrase(AngledBracketList(TypeParameter).Optional());
        public Rule BaseClassList => Phrase((Symbol(":") + List(TypeExpr)).Optional());
        public Rule Constraint => Phrase(Keyword("class") | Keyword("struct") | TypeExpr); 
        public Rule ConstraintClause => Phrase(Keyword("where") + Identifier + Symbol(":") + TypeExpr);
        public Rule ConstraintList => Phrase(ConstraintClause.ZeroOrMore());

        public Rule Kind => Phrase(Keywords("class", "struct", "interface"));
        public Rule TypeDeclaration => Phrase(Recursive(() => Kind + Identifier + TypeParameterList + BaseClassList + ConstraintList + Braced(MemberDeclaration.ZeroOrMore())));
        public Rule TypeDeclarationWithPreamble => Phrase(DeclarationPreamble + TypeDeclaration);

        public Rule FunctionParameterKeywords => Phrase(Keywords("ref", "out", "in", "params").Optional());
        public Rule DefaultValue => Phrase((Symbol("=") + Expression).Optional());
        public Rule FunctionParameter => Phrase(AttributeList + FunctionParameterKeywords + TypeExpr + Identifier + DefaultValue);
        public Rule FunctionParameterList => Phrase(ParenthesizedList(FunctionParameter));
        public Rule ExpressionBody => Phrase(Symbol("=>") + Expression);
        public Rule FunctionBody => Phrase(Recursive(() => ExpressionBody | CompoundStatement));
        public Rule BaseCall => Phrase(Keyword("base") + ParenthesizedExpression);
        public Rule ThisCall => Phrase(Keyword("this") + ParenthesizedExpression);
        public Rule BaseOrThisCall => Phrase((Symbol(":") + (BaseCall | ThisCall)).Optional());
        public Rule ConstructorDeclaration => Phrase(Identifier + FunctionParameterList + BaseOrThisCall + FunctionBody);
        public Rule MethodDeclaration => Phrase(TypeExpr + Identifier + FunctionParameterList + FunctionBody);
        public Rule FieldDeclaration => Phrase(TypeExpr + Identifier + Initialization);

        public Rule Getter => Phrase(Keyword("get") + FunctionBody);
        public Rule Setter => Phrase(Keyword("set") + FunctionBody);
        public Rule Initter => Phrase(Keyword("init") + FunctionBody);
        public Rule PropertyClauses => Phrase((Getter | Setter | Initter).ZeroOrMore());
        public Rule PropertyBody => Phrase(ExpressionBody | Braced(PropertyClauses));
        public Rule PropertyDeclaration => Phrase(TypeExpr + Identifier + PropertyBody);
        public Rule IndexerDeclaration => Phrase(TypeExpr + Keyword("this") + Bracketed(FunctionParameter) + PropertyBody);
        public Rule OperatorDeclaration => Phrase(TypeExpr + Keyword("operator") + OverloadableOperator + FunctionParameterList + FunctionBody);
        public Rule ImplicitOrExplicit => Phrase(Keywords("implicit", "explicit"));
        public Rule ConverterDeclaration => Phrase(TypeExpr + ImplicitOrExplicit + Keyword("operator") + TypeExpr + FunctionBody);

        public Rule MemberDeclaration => Phrase(DeclarationPreamble
            + ConstructorDeclaration
            | MethodDeclaration
            | FieldDeclaration
            | OperatorDeclaration
            | ConverterDeclaration
            | TypeDeclaration
            | IndexerDeclaration
            | PropertyDeclaration);

        public Rule NamespaceDeclaration => Phrase(Keyword("namespace") + QualifiedIdentifier + Braced(TopDeclaration.ZeroOrMore()));
        public Rule FileScopedNamespace => Phrase(Keyword("namespace") + QualifiedIdentifier + EOS);
        public Rule TopDeclaration => Phrase(Recursive(() => NamespaceDeclaration | TypeDeclarationWithPreamble));
        public Rule File => Phrase(UsingDirective.ZeroOrMore() + FileScopedNamespace.Optional() + TopDeclaration.ZeroOrMore());

        public Rule ArrayRankSpecifier => Phrase(Bracketed(Comma.ZeroOrMore()));
        public Rule ArrayRankSpecifiers => Phrase(ArrayRankSpecifier.ZeroOrMore());
        public Rule TypeArgList => Phrase(AngledBracketList(TypeExpr));
        public Rule Nullable => Phrase(Symbol("?").Optional());
        public Rule TypeExpr => Phrase(Recursive(() => QualifiedIdentifier + TypeArgList.Optional() + ArrayRankSpecifiers));

        // Tokenization pass 
        public Rule OperatorChar => "!%^&|*?+-=/><".ToCharSetRule();
        public Rule OperatorToken => OperatorChar.OneOrMore();
        public Rule Separator => ";,.".ToCharSetRule();
        public Rule Delimiter => "[]{}()".ToCharSetRule();
        //public Rule Token => Phrase(OperatorToken | Identifier | Literal | Comment | Spaces | AnyChar);

        /*
        // Structural pass 
        public Rule TokenGroup => Phrase(Token.ButNot(Delimiter | Separator).OneOrMore()); 
        public Rule Element => Phrase(Structure | TokenGroup);
        public Rule BracedStructure => Phrase("{" + Element.ZeroOrMore() + "}");
        public Rule BracketedStructure => Phrase("[" + Element.ZeroOrMore() + "]");
        public Rule ParenthesizedStructure => Phrase("(" + Element.ZeroOrMore() + ")");
        public Rule Structure => Phrase(Recursive(() => BracketedStructure | ParenthesizedStructure | BracedStructure));
        */

        // Some C# features not supported:
        // goto
        // label
        // LINQ natural query syntax
        // async
        // await
        // fixed
        // lock
        // events
        // finalizer
        // unsafe
        // checked / unchecked
        // pointers
        // stackalloc
        // delegates
        // pattern matching
        // switch expressions
        // preprocessor directives 
        // records
        // with
        // sizeof
        // verbatim strings (to-do)
    }
}