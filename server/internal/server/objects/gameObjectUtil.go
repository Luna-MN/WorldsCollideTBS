package objects

type TileType int

const (
	Grass TileType = iota
	River
	Path
	Fall
)

type TopTileType int

const (
	None TopTileType = iota
	Tree
	Stone
)

type TileTopState int

const (
	Small TileTopState = iota
	Medium
	Large
)
