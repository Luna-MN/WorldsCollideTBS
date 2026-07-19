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
                                     skills TEXT, -- CSV style split by commas
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
                                  count INTEGER DEFAULT 1 NOT NULL,
                                  PRIMARY KEY (armyId, unitId)
);

CREATE TABLE IF NOT EXISTS users_army (
                                    userId INTEGER NOT NULL REFERENCES users(id),
                                    armyId INTEGER NOT NULL REFERENCES faction(id),
                                    PRIMARY KEY (userId, armyId)
);


CREATE TABLE IF NOT EXISTS skills (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL UNIQUE,
                                    description TEXT NOT NULL,
                                    type TEXT NOT NULL,
                                    cooldown INTEGER NOT NULL,
                                    AP INTEGER NOT NULL,
                                    range INTEGER NOT NULL,

                                    damage INTEGER,

                                    healing INTEGER
);

CREATE TABLE IF NOT EXISTS units_skills (
                                    unitId INTEGER NOT NULL REFERENCES units(id),
                                    skillId INTEGER NOT NULL REFERENCES skills(id),
                                    PRIMARY KEY (unitId, skillId)
);

-- sqlc generate -f server/internal/server/db/config/sqlc.yml