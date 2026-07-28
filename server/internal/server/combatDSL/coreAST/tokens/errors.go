package combatDSL

import "fmt"

type TextSpan struct {
	Start  int
	Length int
}

func (s TextSpan) End() int {
	return s.Start + s.Length
}

type Expr interface {
	Span() TextSpan
}

type baseExpr struct {
	span TextSpan
}

func (b baseExpr) Span() TextSpan {
	return b.span
}

type IntLit struct {
	baseExpr
	Value int
}

func NewIntLit(value int, span TextSpan) *IntLit {
	return &IntLit{
		baseExpr: baseExpr{span: span},
		Value:    value,
	}
}

type Call struct {
	baseExpr
	Name string
	Args []Expr
}

func NewCall(name string, args []Expr, span TextSpan) *Call {
	return &Call{
		baseExpr: baseExpr{span: span},
		Name:     name,
		Args:     args,
	}
}

type ParseError struct {
	Message  string
	Position int
}

func (e *ParseError) Error() string {
	return fmt.Sprintf("parse error at character %d: %s", e.Position+1, e.Message)
}

type CompileError struct {
	Message  string
	Position int
}

func (e *CompileError) Error() string {
	return fmt.Sprintf("compile error at character %d: %s", e.Position+1, e.Message)
}

type TokenKind int

const (
	TokenIdentifier TokenKind = iota
	TokenInteger
	TokenLeftParen
	TokenRightParen
	TokenComma
	TokenEnd
)

type Token struct {
	Kind     TokenKind
	Text     string
	IntValue int
	Position int
}
