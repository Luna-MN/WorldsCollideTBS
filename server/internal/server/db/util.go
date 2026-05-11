package db

import "database/sql"

func NewNullString(s string) sql.NullString {
	str := sql.NullString{String: s, Valid: true}
	return str
}
