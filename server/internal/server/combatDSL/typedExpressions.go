package combatDSL

type ValueKind int

const (
	ValueKindEType ValueKind = iota
	ValueKindCType
	ValueKindFunc
)

type DSLExpr interface {
	dslExpr()
}

type EType interface {
	DSLExpr
	eType()
}

type CType interface {
	DSLExpr
	cType()
}

type FuncExpr interface {
	DSLExpr
	funcExpr()
}

type DamageExpr struct {
	Amount int
}

func (*DamageExpr) dslExpr() {}
func (*DamageExpr) eType()   {}

type HealExpr struct {
	Amount int
}

func (*HealExpr) dslExpr() {}
func (*HealExpr) eType()   {}

type PoisonExpr struct {
	Amount int
}

func (*PoisonExpr) dslExpr() {}
func (*PoisonExpr) eType()   {}

type BleedExpr struct {
	Amount int
}

func (*BleedExpr) dslExpr() {}
func (*BleedExpr) eType()   {}

type CompositeExpr struct {
	Effects []EType
}

func (*CompositeExpr) dslExpr() {}
func (*CompositeExpr) eType()   {}

type TurnsExpr struct {
	Amount int
}

func (*TurnsExpr) dslExpr() {}
func (*TurnsExpr) cType()   {}

type InflictExpr struct {
	Effect EType
	Turns  int
}

func (*InflictExpr) dslExpr()  {}
func (*InflictExpr) funcExpr() {}

type DelayExpr struct {
	Turns int
	Then  FuncExpr
}

func (*DelayExpr) dslExpr()  {}
func (*DelayExpr) funcExpr() {}
