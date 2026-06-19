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
);

CREATE TABLE IF NOT EXISTS faction (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL UNIQUE,
                                    description TEXT
);

CREATE TABLE IF NOT EXISTS army (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL UNIQUE,
                                    description TEXT
);

CREATE TABLE IF NOT EXISTS units (
                                     id INTEGER PRIMARY KEY AUTOINCREMENT,
                                     name TEXT NOT NULL,
                                     attacks TEXT, -- CSV style split by commas
                                     movement TEXT,
                                     maxHP INTEGER,
                                     AP INTEGER,
                                     Speed INTEGER, -- 0 Normal, 1 Fast, 2 Slow
                                     Armies TEXT
);

CREATE TABLE IF NOT EXISTS army_faction (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    factionId INTEGER NOT NULL REFERENCES faction(id),
                                    armyId INTEGER NOT NULL REFERENCES army(id)
);

CREATE TABLE IF NOT EXISTS army_units (
                                  armyId INTEGER NOT NULL REFERENCES army(id),
                                  unitId INTEGER NOT NULL REFERENCES units(id),
                                  PRIMARY KEY (armyId, unitId)
);

CREATE TABLE IF NOT EXISTS users_army (
                                    userId INTEGER NOT NULL REFERENCES users(id),
                                    armyId INTEGER NOT NULL REFERENCES faction(id),
                                    PRIMARY KEY (userId, armyId)
);

-- sqlc generate -f server/internal/server/db/config/sqlc.yml