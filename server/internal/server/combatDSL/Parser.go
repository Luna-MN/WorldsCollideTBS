package combatDSL

import tokens2 "server/internal/server/combatDSL/coreAST/tokens"

type Parser struct {
	tokens   []tokens2.Token
	position int
}

func Parse(text string) (tokens2.Expr, error) {
	lexer := NewLexer(text)

	tokens, err := lexer.Lex()
	if err != nil {
		return nil, err
	}

	parser := &Parser{tokens: tokens}

	expr, err := parser.parseExpr()
	if err != nil {
		return nil, err
	}

	if _, err := parser.expect(tokens2.TokenEnd, "expected end of input"); err != nil {
		return nil, err
	}

	return expr, nil
}

func (p *Parser) parseExpr() (tokens2.Expr, error) {
	token := p.current()

	switch token.Kind {
	case tokens2.TokenInteger:
		return p.parseInt()
	case tokens2.TokenIdentifier:
		return p.parseCall()
	default:
		return nil, &tokens2.ParseError{
			Message:  "expected integer literal or function call",
			Position: token.Position,
		}
	}
}

func (p *Parser) parseInt() (tokens2.Expr, error) {
	token, err := p.expect(tokens2.TokenInteger, "expected integer literal")
	if err != nil {
		return nil, err
	}

	return tokens2.NewIntLit(
		token.IntValue,
		tokens2.TextSpan{Start: token.Position, Length: len(token.Text)},
	), nil
}

func (p *Parser) parseCall() (tokens2.Expr, error) {
	name, err := p.expect(tokens2.TokenIdentifier, "expected function name")
	if err != nil {
		return nil, err
	}

	if _, err := p.expect(tokens2.TokenLeftParen, "expected '(' after '"+name.Text+"'"); err != nil {
		return nil, err
	}

	args := make([]tokens2.Expr, 0)

	if !p.match(tokens2.TokenRightParen) {
		for {
			arg, err := p.parseExpr()
			if err != nil {
				return nil, err
			}

			args = append(args, arg)

			if !p.match(tokens2.TokenComma) {
				break
			}
		}

		if _, err := p.expect(tokens2.TokenRightParen, "expected ')' after arguments for '"+name.Text+"'"); err != nil {
			return nil, err
		}
	}

	previous := p.previous()
	end := previous.Position + len(previous.Text)

	return tokens2.NewCall(
		name.Text,
		args,
		tokens2.TextSpan{Start: name.Position, Length: end - name.Position},
	), nil
}

func (p *Parser) current() tokens2.Token {
	return p.tokens[p.position]
}

func (p *Parser) previous() tokens2.Token {
	return p.tokens[p.position-1]
}

func (p *Parser) match(kind tokens2.TokenKind) bool {
	if p.current().Kind != kind {
		return false
	}

	p.position++
	return true
}

func (p *Parser) expect(kind tokens2.TokenKind, message string) (tokens2.Token, error) {
	if p.current().Kind != kind {
		return tokens2.Token{}, &tokens2.ParseError{
			Message:  message,
			Position: p.current().Position,
		}
	}

	token := p.current()
	p.position++

	return token, nil
}
