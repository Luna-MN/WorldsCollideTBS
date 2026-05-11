-- noinspection AnnotatorForFile

CREATE TABLE IF NOT EXISTS users (
                                     id INTEGER PRIMARY KEY AUTOINCREMENT,
                                     username TEXT NOT NULL UNIQUE,
                                     password_hash TEXT,
                                     steamId TEXT
);


-- sqlc generate -f server/internal/server/db/config/sqlc.yml