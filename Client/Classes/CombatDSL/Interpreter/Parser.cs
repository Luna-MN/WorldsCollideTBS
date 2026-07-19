using System.Collections.Generic;


public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _position;

    private Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    public static Expr Parse(string text)
    {
        Lexer lexer = new(text);
        IReadOnlyList<Token> tokens = lexer.Lex();

        Parser parser = new(tokens);
        Expr expr = parser.ParseExpr();

        parser.Expect(TokenKind.End, "Expected end of input.");

        return expr;
    }

    private Expr ParseExpr()
    {
        Token token = Current;

        return token.Kind switch
        {
            TokenKind.Integer => ParseInt(),
            TokenKind.Identifier => ParseCall(),
            _ => throw new DslParseException("Expected integer literal or function call.", token.Position)
        };
    }

    private Expr ParseInt()
    {
        Token token = Expect(TokenKind.Integer, "Expected integer literal.");
        return new IntLit(token.IntValue, new TextSpan(token.Position, token.Text.Length));
    }

    private Expr ParseCall()
    {
        Token name = Expect(TokenKind.Identifier, "Expected function name.");
        Expect(TokenKind.LeftParen, $"Expected '(' after '{name.Text}'.");

        List<Expr> args = new();

        if (!Match(TokenKind.RightParen))
        {
            while (true)
            {
                args.Add(ParseExpr());

                if (!Match(TokenKind.Comma))
                    break;
            }

            Expect(TokenKind.RightParen, $"Expected ')' after arguments for '{name.Text}'.");
        }

        int end = Previous.Position + Previous.Text.Length;
        return new Call(
            name.Text,
            args,
            new TextSpan(name.Position, end - name.Position)
        );
    }

    private Token Current => _tokens[_position];

    private Token Previous => _tokens[_position - 1];

    private bool Match(TokenKind kind)
    {
        if (Current.Kind != kind)
            return false;

        _position++;
        return true;
    }

    private Token Expect(TokenKind kind, string message)
    {
        if (Current.Kind != kind)
            throw new DslParseException(message, Current.Position);

        Token token = Current;
        _position++;
        return token;
    }
}
