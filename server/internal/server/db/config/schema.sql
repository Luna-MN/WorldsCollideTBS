-- noinspection AnnotatorForFile

CREATE TABLE IF NOT EXISTS users (
                                     id INTEGER PRIMARY KEY AUTOINCREMENT,
                                     username TEXT NOT NULL UNIQUE,
                                     password_hash TEXT,
                                     steamId TEXT,
                                     LastLoggedIn DATETIME,
                                     Avatar TEXT
);

CREATE TABLE IF NOT EXISTS games (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT ,
                                    player1Id INTEGER,
                                    player2Id INTEGER,
                                    player1Score INTEGER,
                                    player2Score INTEGER,
                                    winnerId INTEGER,
                                    MatchTime DATETIME
)

-- sqlc generate -f server/internal/server/db/config/sqlc.yml