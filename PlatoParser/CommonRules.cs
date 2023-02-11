namespace PlatoParser
{
    // This is currently a parser for a subset of the C# language 
    // See: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure
    // The goal is to separate this into a C# and A Plato grammar.
    // First step though is to test this on my own source code. 

    public class CommonRules : Grammar
    {
        // Helper functions 
        public Rule Comma => Symbol(",");
        public Rule List(Rule r, Rule? sep = null) => (r + (sep ?? Comma + r).ZeroOrMore()).Optional();
        public Rule List(Rule r) => List(r, Symbol(","));
        public Rule Delimited(Rule first, Rule middle, Rule last) => first + middle + last;
        public Rule Parenthesized(Rule r) => Delimited(Symbol("("), r, Symbol(")"));
        public Rule ParenthesizedList(Rule r, Rule? sep = null) => Parenthesized(List(r, sep));
        public Rule Bracketed(Rule r) => Delimited(Symbol("["), r, Symbol("]"));
        public Rule BracketedList(Rule r, Rule? sep = null) => Delimited(Symbol("["), List(r, sep), Symbol("]"));
        public Rule Keyword(string s) => s + IdentifierChar.NotAt() + WS;
        public Rule UntilPast(Rule r) => RepeatUntilPast(AnyChar, r);
        public Rule RepeatUntilPast(Rule repeat, Rule delimiter) => delimiter.NotAt().Then(repeat).ZeroOrMore().Then(repeat);
        public Rule Symbol(string s) => s + WS;
        public Rule Symbols(params string[] strings) => Choice(strings.OrderByDescending(x => x.Length).Select(Symbol).ToArray());
        public Rule Keywords(params string[] strings) => Choice(strings.OrderByDescending(x => x.Length).Select(Keyword).ToArray());
        public Rule Braced(Rule r) => Delimited(Symbol("{"), r, Symbol("}"));
        public Rule BracedList(Rule r, Rule? sep = null) => Delimited(Symbol("{"), List(r, sep), Symbol("}"));
        public Rule AngledBracketList(Rule r, Rule? sep = null) => Delimited(Symbol("<"), List(r, sep), Symbol(">"));

        // Basic 
        public Rule AnyChar => AnyCharRule.Default;
        public Rule LowerCaseLetter => 'a'.To('z');
        public Rule UpperCaseLetter => 'A'.To('Z');
        public Rule Letter => LowerCaseLetter | UpperCaseLetter;
        public Rule Digit => '0'.To('9');
        public Rule DigitOrLetter => Letter | Digit;
        public Rule IdentifierFirstChar => '_' | Letter;
        public Rule IdentifierChar => IdentifierFirstChar | Digit;
        public Rule FractionalPart => "." + Optional(Digits);
        public Rule HexDigit => Digit | 'a'.To('f') | 'A'.To('F');
        public Rule BinDigit => '0'.To('1');
        public Rule Hexadecimal => Choice("0x" | "0X" + HexDigit.OneOrMore());
        public Rule Binary => Choice("0b" | "0B" + BinDigit.OneOrMore());
        public Rule IntegerSuffix => Choice("ul", "UL", "u", "U", "l", "L", "lu", "lU", "Lu", "LU");
        public Rule Integer => Node((Hexadecimal | Binary | Digits.ThenNot(".fFdDmM".ToCharSetRule())) + IntegerSuffix.Optional());
        public Rule FloatSuffix => "fFdDmM".ToCharSetRule();
        public Rule Sign => "+-".ToCharSetRule();
        public Rule ExponentPart => "eE".ToCharSetRule() + Sign.Optional() + Digits;
        public Rule Float => Node(Digits + FractionalPart.Optional() + ExponentPart.Optional() + FloatSuffix.Optional());
        public Rule Digits => Digit.OneOrMore();
        public Rule SpaceChars => " \t\n\r\0\v\f".ToCharSetRule();
        public Rule Spaces => SpaceChars.OneOrMore();
        public Rule NewLine => Choice("\r\n", "\n");
        public Rule UntilNextLine => AnyChar.Except(NewLine).ZeroOrMore().Then(NewLine.Optional());
        public Rule SingleLineComment => Sequence("//", UntilNextLine);
        public Rule BlockComment => Sequence("/*", UntilPast("*/"));
        public Rule Comment => SingleLineComment | BlockComment;
        public Rule WS => (Spaces | Comment).ZeroOrMore();

        // Literals 
        public Rule NumberLiteral => (Float | Integer); 
        public Rule EscapedLiteralChar => '\\' + AnyChar; // TODO: handle special codes like \u codes and \x
        public Rule StringLiteralChar => EscapedLiteralChar | AnyChar.Except('"');
        public Rule StringLiteral => Node('"' + StringLiteralChar.ZeroOrMore() + '"');
        public Rule CharLiteralChar => EscapedLiteralChar | AnyChar.Except('\'');
        public Rule CharLiteral => Node('\'' + CharLiteralChar + '\'');
        public Rule BooleanLiteral => Node(Keyword("true") | Keyword("false"));
        public Rule NullLiteral => Node(Keyword("null"));
        public Rule ValueLiteral => Node(Keyword("value"));
        public Rule Literal => Node(NumberLiteral
            | StringLiteral
            | CharLiteral
            | BooleanLiteral
            | NullLiteral
            | ValueLiteral)
            .Then(WS);

        // Operators 
        public Rule CommonArithmeticOperators => Choice("+", "-", "*", "/", "%");
        public Rule IncrementDecrementOperator => Choice("++", "--");
        public Rule ShiftOperators => Choice(">>>", ">>", "<<");
        public Rule BitwiseOperators => Choice("&", "|", "^");
        public Rule BooleanOperators => Choice("&&", "||");
        public Rule ComplementOperator => "~";
        public Rule NotOperator => "!";
        public Rule NegationOperator => "-";
        public Rule PositiveOperator => "+";
        public Rule IncrementOperator => "++";
        public Rule DecrementOperator => "--";
        public Rule Indexer => Bracketed(Expression);
        public Rule BinaryArithmeticOperators => CommonArithmeticOperators | ShiftOperators | BitwiseOperators | BooleanOperators;
        public Rule EqualityOperators => Choice("==", "!=");
        public Rule ComparisonOperators => Choice("<", ">", "<=", ">=");
        public Rule NullCoalescingOperator => "??";
        public Rule BinaryOperator => Node(BinaryArithmeticOperators | EqualityOperators | ComparisonOperators | NullCoalescingOperator);
        public Rule UnaryOperator => Node(IncrementOperator | DecrementOperator | ComplementOperator | NotOperator | NegationOperator | PositiveOperator);
        public Rule OverloadableOperator => Symbols("+", "-", "!", "~", "++", "--", "*", "/", "%", "&", "|", "^", "<<", ">>", ">>>", "==", "!=", "<", ">", "<=", ">=");

        public Rule PostfixOperator => Node(IncrementOperator
            | DecrementOperator
            | NegationOperator // Null ignoring operator
            | MemberAccess
            | ConditionalMemberAccess
            | FunctionArgs
            | Indexer
            | BinaryOperation);

        // Expressions 
        public Rule Identifier => Node(IdentifierFirstChar.Then(IdentifierChar.ZeroOrMore())) + WS;
        public Rule BinaryOperation => Node(BinaryOperator + WS + Expression);
        public Rule UnaryOperation => Node(UnaryOperator + WS + Expression);
        public Rule PostfixOperation => Node(Expression + PostfixOperator + WS);
        public Rule ParenthesizedExpression => Node(ParenthesizedList(Expression));
        public Rule ConditionalOperation => Node(Expression + Symbol("?") + Expression + Symbol(":") + Expression);
        public Rule SimpleAssignment => Node(Expression + Symbol("=") + Expression);
        public Rule CompoundAssignmentOperator => BinaryArithmeticOperators + "=";
        public Rule CompoundAssignment => Expression + CompoundAssignmentOperator + WS + Expression;
        public Rule Assignment => Node(CompoundAssignment | SimpleAssignment);
        public Rule ThrowExpression => Node(Keyword("throw") + Expression);
        public Rule LambdaParameter => Node((TypeExpr + Identifier) | Identifier);
        public Rule LambdaParameters => Node(LambdaParameter | ParenthesizedList(LambdaParameter));
        public Rule LambdaExpr => Node(LambdaParameter + Symbol("=>") + (CompoundStatement | Expression));
        public Rule MemberAccess => Node(Symbol(".") + Identifier);
        public Rule ConditionalMemberAccess => Node(Symbol("?.") + Identifier);
        public Rule TypeOf => Node(Keyword("typeof") + Parenthesized(TypeExpr));
        public Rule NameOf => Node(Keyword("nameof") + Parenthesized(Expression));
        public Rule Default => Node(Keyword("default") + Parenthesized(TypeExpr).Optional());
        public Rule InitializerClause => Node((Identifier + Symbol("=") + Expression) | Expression);
        public Rule Initializer => BracedList(InitializerClause);
        public Rule ArraySizeSpecifier => Node(Bracketed(Expression));
        public Rule New => Node(Keyword("new") + TypeExpr + (FunctionArgs | ArraySizeSpecifier.Optional()) + Initializer.Optional());
        public Rule IsExpression => Node(Keyword("is") + TypeExpr + Identifier.Optional());
        public Rule AsExpression => Node(Keyword("as") + TypeExpr + Identifier.Optional());
        public Rule StringInterpolationContent => Braced(Expression) | StringLiteralChar;
        public Rule StringInterpolation => Node("$\"" + StringInterpolationContent.ZeroOrMore() + "\"");
        public Rule FunctionCall => Node(Expression + FunctionArgs);
        public Rule FunctionArgKeyword => Node(Keywords("ref", "out").ZeroOrMore());
        public Rule FunctionArg => Node(FunctionArgKeyword.ZeroOrMore() + Expression);
        public Rule FunctionArgs => Node(ParenthesizedList(FunctionArg));

        public Rule Expression => Recursive(() => Node(ParenthesizedExpression
            | ThrowExpression
            | Literal
            | TypeOf
            | NameOf
            | Default
            | New
            | UnaryOperation
            | BinaryOperation
            | LambdaExpr
            | PostfixOperation
            | StringInterpolation
            | ConditionalOperation)) + WS;

        // Statements 
        public Rule EOS => Symbol(";");
        public Rule ExpressionStatement => Node(Expression + EOS);
        public Rule ElseClause => Node(Keyword("else") + Statement);
        public Rule IfStatement => Node(Keyword("if") + ParenthesizedExpression + Statement + ElseClause.Optional());
        public Rule WhileStatement => Node(Keyword("while") + ParenthesizedExpression + Statement);
        public Rule DoWhileStatement => Node(Keyword("do") + Statement + Keyword("while") + ParenthesizedExpression);
        public Rule ReturnStatement => Node(Keyword("return") + Expression.Optional() + EOS);
        public Rule BreakStatement => Node(Keyword("break") + EOS);
        public Rule YieldStatement => Node(Keyword("yield") + Keyword("return") + Expression + EOS);
        public Rule YieldBreakStatement => Node(Keyword("yield") + Keyword("break") + EOS);
        public Rule ContinueStatement => Node(Keyword("break") + EOS);
        public Rule CompoundStatement => Node(Braced(Statement.ZeroOrMore()));
        public Rule CatchClause => Node(Keyword("catch") + ParenthesizedExpression + CompoundStatement);
        public Rule FinallyClause => Node(Keyword("finally") + CompoundStatement);
        public Rule CaseClause => Node((Keyword("default") | (Keyword("case") + Expression)).Then(Statement));
        public Rule SwitchStatement => Node(Keyword("switch") + Braced(CaseClause.ZeroOrMore()));
        public Rule TryStatement => Node(Keyword("try") + CompoundStatement + CatchClause.Optional() + FinallyClause.Optional());
        public Rule ForEachStatement => Node(Keyword("foreach") + Symbol("(") + TypeExpr + Keyword("in") + Expression + Symbol(")") + Statement);
        public Rule ForStatement => Node(Keyword("for") + Symbol("(") + VarDeclStatement + Expression + EOS + Expression + Symbol(")") + Statement);
        public Rule Initialization => Node(Optional(Symbol("=") + Expression));
        public Rule VarDeclStatement => Node((Keyword("var") | TypeExpr) + Identifier + Initialization + EOS);
        
        public Rule Statement => Recursive(() => Node(EOS 
            | CompoundStatement
            | IfStatement 
            | WhileStatement 
            | DoWhileStatement 
            | ReturnStatement 
            | BreakStatement 
            | ContinueStatement 
            | ForStatement 
            | ForEachStatement 
            | VarDeclStatement 
            | ExpressionStatement));
        
        public Rule RefKeyword => Keyword("ref");
        public Rule OutKeyword => Keyword("out");
        public Rule ParamsKeyword => Keyword("params");

        public Rule QualifiedIdentifier => Node(List(Identifier, Symbol(".")));
        public Rule Static => Node(Optional(Keyword("static")));
        public Rule UsingDirective => Node(Keyword("using") + Static + QualifiedIdentifier + EOS);

        public Rule Modifier => Node(Keywords("static", "sealed", "partial", "readonly", "const"));
        public Rule AccessSpecifier => Node(Keywords("public", "private", "protected", "internal"));
        public Rule ModifiersAndSpecifiers => (Modifier | AccessSpecifier).ZeroOrMore();
        public Rule Attribute => Bracketed(List(Identifier + FunctionArgs.Optional()));
        public Rule AttributeList => Node(Attribute.ZeroOrMore());
        public Rule DeclarationPreamble => AttributeList + ModifiersAndSpecifiers;
        public Rule TypeVariance => Node(Optional(Keywords("in", "out")));
        public Rule TypeParameter => Node(TypeVariance + Identifier);
        public Rule TypeParameterList => (AngledBracketList(TypeParameter).Optional());
        public Rule BaseClassList => Node((Symbol(":") + List(TypeExpr)).Optional());
        public Rule Constraint => Node(Keyword("class") | Keyword("struct") | TypeExpr); // TODO: many types of constraints are possible 
        public Rule ConstraintClause => Node(Keyword("where") + Identifier + Symbol(":") + TypeExpr);
        public Rule ConstraintList => Node(ConstraintClause.ZeroOrMore());

        public Rule Kind => Node(Keywords("class", "struct", "interface"));
        public Rule TypeDeclaration => Node(Kind + Identifier + TypeParameterList + BaseClassList + ConstraintList + Braced(MemberDeclaration.ZeroOrMore()));

        public Rule TypeDeclarationWithPreamble => Node(DeclarationPreamble + TypeDeclaration);

        public Rule FunctionParameterKeywords => Node(Keywords("ref", "out", "in", "params").Optional());
        public Rule DefaultValue => Node((Symbol("=") + Expression).Optional());
        public Rule FunctionParameter => Node(AttributeList + FunctionParameterKeywords + TypeExpr + Identifier + DefaultValue);
        public Rule FunctionParameterList => Node(ParenthesizedList(FunctionParameter));
        public Rule ExpressionBody => Node(Symbol("=>") + Expression);
        public Rule FunctionBody => Node(ExpressionBody | CompoundStatement);
        public Rule BaseCall => Node(Keyword("base") + ParenthesizedExpression);
        public Rule ThisCall => Node(Keyword("this") + ParenthesizedExpression);
        public Rule BaseOrThisCall => Node((Symbol(":") + (BaseCall | ThisCall)).Optional());
        public Rule ConstructorDeclaration => Node(Identifier + FunctionParameterList + BaseOrThisCall + FunctionBody);
        public Rule MethodDeclaration => Node(TypeExpr + Identifier + FunctionParameterList + FunctionBody);
        public Rule FieldDeclaration => TypeExpr + Identifier + Initialization;

        public Rule Getter => Node(Keyword("get") + FunctionBody);
        public Rule Setter => Node(Keyword("set") + FunctionBody);
        public Rule Initter => Node(Keyword("init") + FunctionBody);
        public Rule PropertyClauses => (Getter | Setter | Initter).ZeroOrMore();
        public Rule PropertyBody => Node(ExpressionBody | Braced(PropertyClauses));
        public Rule PropertyDeclaration => Node(TypeExpr + Identifier + PropertyBody);
        public Rule IndexerDeclaration => Node(TypeExpr + Keyword("this") + Bracketed(FunctionParameter) + PropertyBody);
        public Rule OperatorDeclaration => Node(TypeExpr + Keyword("operator") + OverloadableOperator + FunctionParameterList + FunctionBody);
        public Rule ImplicitOrExplicit => Node(Keywords("implicit", "explicit"));
        public Rule ConverterDeclaration => Node(TypeExpr + ImplicitOrExplicit + Keyword("operator") + TypeExpr + FunctionBody);

        public Rule MemberDeclaration => Node(DeclarationPreamble
            + ConstructorDeclaration
            | MethodDeclaration
            | FieldDeclaration
            | OperatorDeclaration
            | ConverterDeclaration
            | TypeDeclaration
            | IndexerDeclaration
            | PropertyDeclaration);

        public Rule NamespaceDeclaration => Node(Keyword("namespace") + QualifiedIdentifier + Braced(TopDeclaration.ZeroOrMore()));
        public Rule FileScopedNamespace => Node(Keyword("namespace") + QualifiedIdentifier + EOS);
        public Rule TopDeclaration => Recursive(() => Node(NamespaceDeclaration | TypeDeclarationWithPreamble));
        public Rule File => UsingDirective.ZeroOrMore() + FileScopedNamespace.Optional() + TopDeclaration.ZeroOrMore();

        public Rule ArrayRankSpecifier => Node(Bracketed(Node(Symbol(",")).ZeroOrMore()));
        public Rule ArrayRankSpecifiers => Node(ArrayRankSpecifier.ZeroOrMore());
        public Rule TypeArgList => AngledBracketList(TypeExpr);
        public Rule Nullable => Node(Symbol("?").Optional());
        public Rule TypeExpr => Recursive(() => Node(QualifiedIdentifier + TypeArgList.Optional() + ArrayRankSpecifiers));

        // Tokenization pass 
        public Rule OperatorChar => "!%^&|*?+-=/><".ToCharSetRule();
        public Rule OperatorToken => OperatorChar.OneOrMore();
        public Rule Separator => ";,.".ToCharSetRule();
        public Rule Delimiter => "[]{}()".ToCharSetRule();
        public Rule Token => Node(OperatorToken | Identifier | Literal | Comment | Spaces | AnyChar);

        // Structural pass 
        public Rule TokenGroup => Node(Token.ButNot(Delimiter | Separator).OneOrMore()); 
        public Rule Element => Node(Structure | TokenGroup);
        public Rule BracedStructure => Node("{" + Element.ZeroOrMore() + "}");
        public Rule BracketedStructure => Node("[" + Element.ZeroOrMore() + "]");
        public Rule ParenthesizedStructure => Node("(" + Element.ZeroOrMore() + ")");
        public Rule Structure => Node(Recursive(() => BracketedStructure | ParenthesizedStructure | BracedStructure));

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