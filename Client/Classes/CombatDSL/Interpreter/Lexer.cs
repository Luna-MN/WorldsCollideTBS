using System;
using System.Collections.Generic;

internal sealed class Lexer
{
    private readonly string _text;
    private int _position;

    public Lexer(string text)
    {
        _text = text;
    }

    public IReadOnlyList<Token> Lex()
    {
        List<Token> tokens = new();

        while (true)
        {
            Token token = NextToken();
            tokens.Add(token);

            if (token.Kind == TokenKind.End)
                return tokens;
        }
    }

    private Token NextToken()
    {
        SkipWhitespace();

        if (_position >= _text.Length)
            return new Token(TokenKind.End, string.Empty, 0, _position);

        char c = _text[_position];

        if (char.IsLetter(c))
            return ReadIdentifier();

        if (char.IsDigit(c))
            return ReadInteger();

        int start = _position;
        _position++;

        return c switch
        {
            '(' => new Token(TokenKind.LeftParen, "(", 0, start),
            ')' => new Token(TokenKind.RightParen, ")", 0, start),
            ',' => new Token(TokenKind.Comma, ",", 0, start),
            _ => throw new DslParseException($"Unexpected character '{c}'.", start)
        };
    }

    private Token ReadIdentifier()
    {
        int start = _position;

        while (_position < _text.Length && char.IsLetterOrDigit(_text[_position]))
            _position++;

        string text = _text[start.._position];
        return new Token(TokenKind.Identifier, text, 0, start);
    }

    private Token ReadInteger()
    {
        int start = _position;

        while (_position < _text.Length && char.IsDigit(_text[_position]))
            _position++;

        string text = _text[start.._position];

        if (!int.TryParse(text, out int value))
            throw new DslParseException($"Integer literal '{text}' is too large.", start);

        return new Token(TokenKind.Integer, text, value, start);
    }

    private void SkipWhitespace()
    {
        while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            _position++;
    }
}

