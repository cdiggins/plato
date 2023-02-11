using System.Security.Cryptography.X509Certificates;

namespace PlatoParser
{
    public class CommonRules : Grammar
    {
        public Rule List(Rule r, Rule separator) => (r + (separator + r).ZeroOrMore()).Optional();
        public Rule List(Rule r) => List(r, Symbol(","));
        public Rule List(Rule first, Rule last, Rule r, Rule separator) => first + WS + List(r, separator) + WS + last;
        public Rule Delimited(Rule first, Rule middle, Rule last) => first + WS + middle + WS + last;
        public Rule Parenthesized(Rule r) => Delimited(Symbol("("), r, Symbol(")"));
        public Rule ParenthesizedList(Rule r) => Parenthesized(List(r));
        public Rule Bracketed(Rule r) => Delimited(Symbol("["), r, Symbol("]"));
        public Rule Keyword(string s) => s + IdentifierChar.NotAt() + WS;
        public Rule UntilPast(Rule r) => r.NotAt().Then(AnyChar).ZeroOrMore().Then(r);
        public Rule Symbol(Rule r) => r + WS;
        public Rule Keywords(params string[] strings) => Choice(strings.Select(Keyword).ToArray());
        public Rule Braced(Rule r) => Delimited(Symbol("{"), r, Symbol("}"));
        public Rule BracedList(Rule r) => Delimited(Symbol("{"), List(r), Symbol("}"));
        public Rule AngledBracketList(Rule r) => Delimited(Symbol("<"), List(r), Symbol(">"));

        public Rule AnyChar => AnyCharRule.Default;
        public Rule OperatorChar => "!%^&|*?+-=/><".ToCharSetRule();
        public Rule LowerCaseLetter => 'a'.To('z');
        public Rule UpperCaseLetter => 'A'.To('Z');
        public Rule Letter => LowerCaseLetter | UpperCaseLetter;
        public Rule Digit => '0'.To('9');
        public Rule DigitOrLetter => Letter | Digit;
        public Rule IdentifierFirstChar => '_' | Letter;
        public Rule IdentifierChar => IdentifierFirstChar | Digit;
        public Rule Digits => Digit.OneOrMore();
        public Rule SpaceChars => " \t\n\r\0\v\f".ToCharSetRule();
        public Rule Spaces => SpaceChars.OneOrMore();
        public Rule NewLine => Choice("\r\n", "\n");
        public Rule UntilNextLine => AnyChar.Except(NewLine).ZeroOrMore().Then(NewLine.Optional());
        public Rule SingleLineComment => Sequence("//", UntilNextLine);
        public Rule BlockComment => Sequence("/*", UntilPast("*/"));
        public Rule Comments => SingleLineComment | BlockComment;
        public Rule WS => (Spaces | Comments).ZeroOrMore();
        public Rule Identifier => Node(IdentifierFirstChar.Then(IdentifierChar.ZeroOrMore())).Then(WS);
        public Rule Number => Node(Digits); // TODO: more complex numerical expression
        public Rule EscapedLiteralChar => '\\' + AnyChar; // TODO: handle special codes like \u codes and \x
        public Rule StringLiteralChar => EscapedLiteralChar | AnyChar.Except('"');
        public Rule StringLiteral => Node('"' + StringLiteralChar.ZeroOrMore() + '"');
        public Rule CharLiteralChar => EscapedLiteralChar | AnyChar.Except('\'');
        public Rule CharLiteral => Node('\'' + CharLiteralChar + '\'');
        public Rule BooleanLiteral => Node(Keyword("true") | Keyword("false"));
        public Rule NullLiteral => Node(Keyword("null"));

        public Rule Literal => Node(Number | StringLiteral | CharLiteral | BooleanLiteral | NullLiteral).Then(WS);

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
        public Rule IndexingExpression => Expression + Bracketed(Expression);
        public Rule BinaryArithmeticOperators => CommonArithmeticOperators | ShiftOperators | BitwiseOperators | BooleanOperators;
        public Rule EqualityOperators => Choice("==", "!=");
        public Rule ComparisonOperators => Choice("<", ">", "<=", ">=");
        public Rule NullCoalescingOperator => "??";

        public Rule BinaryOperator => Node(BinaryArithmeticOperators | EqualityOperators | ComparisonOperators | NullCoalescingOperator);

        public Rule BinaryOperation => Node(Expression + BinaryOperator + Expression);
        public Rule UnaryOperator => Node(IncrementOperator | DecrementOperator | ComplementOperator | NotOperator | NegationOperator | PositiveOperator);
        public Rule PostfixOperator => Node(IncrementOperator | DecrementOperator | NegationOperator | MemberAccess | ConditionalMemberAccess);
        public Rule UnaryOperation => Node(UnaryOperator + Expression);
        public Rule PostfixOperation => Node(Expression + PostfixOperator);
        public Rule ParenthesizedExpression => Node(ParenthesizedList(Expression));
        public Rule ConditionalOperation => Node(Expression + Symbol("?") + Expression + Symbol(":") + Expression);
        public Rule SimpleAssignment => Node(Expression + Symbol("=") + Expression);
        public Rule CompoundAssignmentOperator => BinaryArithmeticOperators.Then("=");
        public Rule CompoundAssignment => Expression + CompoundAssignmentOperator + Expression;
        public Rule Assignment => Node(CompoundAssignment | SimpleAssignment);
        public Rule ThrowExpression => Node(Keyword("throw") + WS + Expression);
        public Rule LambdaParameter => Node((TypeExpr + Identifier) | Identifier);
        public Rule LambdaParameters => Node(LambdaParameter | ParenthesizedList(LambdaParameter));
        public Rule LambdaExpr => Node(LambdaParameter + Symbol("=>") + (CompoundStatement | Expression));  
        public Rule MemberAccess => Node(Symbol('.') + Identifier);
        public Rule ConditionalMemberAccess => Node(Symbol("?.") + Identifier);
        public Rule TypeOf => Node(Keyword("typeof") + Parenthesized(TypeExpr));
        public Rule NameOf => Node(Keyword("nameof") + Parenthesized(Expression));
        public Rule Default => Node(Keyword("default") + Parenthesized(TypeExpr).Optional());
        public Rule InitializerClause => Node((Identifier + Symbol("=") + Expression) | Expression);
        public Rule Initializer => BracedList(InitializerClause);
        public Rule ArraySizeSpecifier => Node(Bracketed(Expression));
        public Rule New => Node(Keyword("new") + TypeExpr + (FunctionArgs | ArraySizeSpecifier.Optional()) + Initializer.Optional());

        public Rule Expression => Node(ParenthesizedExpression
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
            | ConditionalOperation)
            .Then(WS);


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
        public Rule CatchClause => Node(Keyword("catch") +  ParenthesizedExpression + CompoundStatement);
        public Rule FinallyClause => Node(Keyword("finally") + CompoundStatement);
        public Rule CaseClause => Node((Keyword("default") | (Keyword("case") + Expression)).Then(Statement));

        public Rule SwitchStatement => Node(Keyword("switch") + Braced(CaseClause.ZeroOrMore()));
        public Rule TryStatement => Node(Keyword("try") + CompoundStatement + CatchClause.Optional() + FinallyClause.Optional());
        public Rule ForEachStatement => Node(Keyword("foreach") + Symbol("(") + TypeExpr + Keyword("in") + Expression + Symbol(")") + Statement);
        public Rule ForStatement => Node(Keyword("for") + Symbol("(") + VarDeclStatement + Expression + EOS + Expression + Symbol(")") + Statement);
        public Rule Initialization => Node(Symbol("=") + Expression);
        public Rule VarDeclStatement => Node(TypeExpr) + Identifier + Initialization.Optional() + EOS;
        public Rule Statement => Node(EOS | IfStatement | WhileStatement | DoWhileStatement | ReturnStatement | BreakStatement | ContinueStatement | ForStatement | ForEachStatement | VarDeclStatement | ExpressionStatement);
        public Rule TypeArgList => AngledBracketList(TypeExpr);
        public Rule RefKeyword => Keyword("ref");
        public Rule OutKeyword => Keyword("out");
        public Rule ParamsKeyword => Keyword("params");
        public Rule IsExpression => Node(Keyword("is") + TypeExpr + Identifier.Optional());
        public Rule AsExpression => Node(Keyword("as") + TypeExpr + Identifier.Optional());
        public Rule StringInterpolationContent => Braced(Expression) | StringLiteralChar;
        public Rule StringInterpolation => Node("$\"" + StringInterpolationContent.ZeroOrMore() + "\"");
        public Rule FunctionCall => Node(Expression + FunctionArgs);
        public Rule FunctionArgKeyword => Node(Keywords("ref", "out").ZeroOrMore());
        public Rule FunctionArg => Node(FunctionArgKeyword.ZeroOrMore() + Expression);
        public Rule FunctionArgs => Node(ParenthesizedList(FunctionArg));


        public Rule ConstructorDeclaration;
        public Rule FieldDeclaration;
        public Rule PropertyDeclaration;
        public Rule MethodDeclaration;
        public Rule IndexerDeclaration;
        public Rule OperatorDeclaration;    

        public Rule QualifiedIdentifier => Node(List(Identifier, Symbol(".")));
        public Rule UsingDirective => Node(Keyword("using") + QualifiedIdentifier);
        

        public Rule Modifier => Node(Keywords("static", "sealed", "partial", "readonly", "const"));
        public Rule AccessSpecifier => Node(Keywords("public", "private", "protected", "internal"));
        public Rule ModifiersAndSpecifiers => (Modifier | AccessSpecifier).ZeroOrMore();
        public Rule Attribute => Bracketed(List(Identifier + FunctionArgs.Optional()));
        public Rule AttributeList => Node(Attribute.ZeroOrMore());
        public Rule DeclarationPreamble => AttributeList + ModifiersAndSpecifiers;
        public Rule TypeParameter => Node(Identifier);
        public Rule TypeParameterList => (AngledBracketList(TypeParameter).Optional());
        public Rule BaseClassList => Node((Symbol(":") + List(TypeExpr)).Optional());
        public Rule Constraint => Node(Keyword("class") | Keyword("struct") | TypeExpr); // TODO: many types of constraints are possible 
        public Rule ConstraintClause => Node(Keyword("where") + Identifier + Symbol(":") + TypeExpr);
        public Rule ConstraintList => Node(ConstraintClause.ZeroOrMore());

        public Rule Kind => Node(Keywords("class", "struct", "interface"));
        public Rule TypeDeclaration => Node(Kind + Identifier + TypeParameterList + BaseClassList + ConstraintList + Braced(MemberDeclaration.ZeroOrMore()));

        public Rule TypeDeclarationWithPreamble => Node(DeclarationPreamble + TypeDeclaration);

        public Rule MemberDeclaration => Node(DeclarationPreamble 
            + ConstructorDeclaration
            | MethodDeclaration
            | FieldDeclaration            
            | OperatorDeclaration
            | TypeDeclaration
            | IndexerDeclaration
            | PropertyDeclaration);

        public Rule NamespaceDeclaration => Node(Keyword("namespace") + QualifiedIdentifier + Braced(TopDeclaration.ZeroOrMore()));
        public Rule TopDeclaration => Node(NamespaceDeclaration | TypeDeclarationWithPreamble);

        public Rule ArrayRankSpecifier => Symbol("[") + Symbol("]");
        public Rule TypeExpr => Node(Keyword("dynamic") 
            | Keyword("var") 
            | (QualifiedIdentifer + TypeArgList.Optional() + ArrayRankSpecifier.Optional()));

        // Lambda
        // Switch
        // "new" expression 
        // "??"
        // .

        // Not supported:
        // goto
        // label
        // async
        // await
        // fixed
        // lock
        // Events
        // ! definitely not null
        // nullable type 
        // "[]" types
        // params 
        // ref / out
        // finalizer

        // Array initalizers 

        // Parameter 

        // TODO: test all rules to make sure that having WS afterwards gets consumed. 
        // NOT supported: complex arrays.

        // TODO: parse the main structure first, then from there feed each section back into more specific rules.
        // NOTE: all of the tests we want are present in this source code. 
    }
}