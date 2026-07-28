package combatDSL

import (
	"server/internal/server/combatDSL/coreAST/tokens"
	"strconv"
	"unicode"
)

type Lexer struct {
	text     []rune
	position int
}

func NewLexer(text string) *Lexer {
	return &Lexer{text: []rune(text)}
}

func (l *Lexer) Lex() ([]combatDSL.Token, error) {
	tokens := make([]combatDSL.Token, 0)

	for {
		token, err := l.nextToken()
		if err != nil {
			return nil, err
		}

		tokens = append(tokens, token)

		if token.Kind == combatDSL.TokenEnd {
			return tokens, nil
		}
	}
}

func (l *Lexer) nextToken() (combatDSL.Token, error) {
	l.skipWhitespace()

	if l.position >= len(l.text) {
		return combatDSL.Token{Kind: combatDSL.TokenEnd, Text: "", Position: l.position}, nil
	}

	c := l.text[l.position]

	if unicode.IsLetter(c) {
		return l.readIdentifier(), nil
	}

	if unicode.IsDigit(c) {
		return l.readInteger()
	}

	start := l.position
	l.position++

	switch c {
	case '(':
		return combatDSL.Token{Kind: combatDSL.TokenLeftParen, Text: "(", Position: start}, nil
	case ')':
		return combatDSL.Token{Kind: combatDSL.TokenRightParen, Text: ")", Position: start}, nil
	case ',':
		return combatDSL.Token{Kind: combatDSL.TokenComma, Text: ",", Position: start}, nil
	default:
		return combatDSL.Token{}, &combatDSL.ParseError{
			Message:  "unexpected character '" + string(c) + "'",
			Position: start,
		}
	}
}

func (l *Lexer) readIdentifier() combatDSL.Token {
	start := l.position

	for l.position < len(l.text) && unicode.IsLetter(l.text[l.position]) || l.position < len(l.text) && unicode.IsDigit(l.text[l.position]) {
		l.position++
	}

	text := string(l.text[start:l.position])

	return combatDSL.Token{
		Kind:     combatDSL.TokenIdentifier,
		Text:     text,
		Position: start,
	}
}

func (l *Lexer) readInteger() (combatDSL.Token, error) {
	start := l.position

	for l.position < len(l.text) && unicode.IsDigit(l.text[l.position]) {
		l.position++
	}

	text := string(l.text[start:l.position])
	value, err := strconv.Atoi(text)
	if err != nil {
		return combatDSL.Token{}, &combatDSL.ParseError{
			Message:  "integer literal '" + text + "' is too large",
			Position: start,
		}
	}

	return combatDSL.Token{
		Kind:     combatDSL.TokenInteger,
		Text:     text,
		IntValue: value,
		Position: start,
	}, nil
}

func (l *Lexer) skipWhitespace() {
	for l.position < len(l.text) && unicode.IsSpace(l.text[l.position]) {
		l.position++
	}
}
