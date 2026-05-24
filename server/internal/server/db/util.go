package db

import (
	"database/sql"
	"time"
)

func NewNullString(s string) sql.NullString {
	str := sql.NullString{String: s, Valid: true}
	return str
}

func NewNullTime(t time.Time) sql.NullTime {
	time := sql.NullTime{Time: t, Valid: true}
	return time
}

func NewNullInt64(i int64) sql.NullInt64 {
	return sql.NullInt64{Int64: i, Valid: true}
}
