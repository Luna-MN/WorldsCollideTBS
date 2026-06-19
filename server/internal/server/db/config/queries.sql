-- name: GetUserByUsername :one
SELECT
    *
FROM
    users
WHERE
    username = ?
LIMIT 1;

-- name: CreateUser :one
INSERT INTO
    users (username, password_hash)
VALUES
    (?, ?)
RETURNING *;

-- name: GetUserId :one
SELECT
    id
FROM
    users
WHERE
    username = lower(?);

-- name: CreateSteamUser :one
INSERT INTO
    users (username, steamId, Avatar, LastLoggedIn)
VALUES
    (?, ?, ?, ?)
RETURNING *;

-- name: GetSteamUser :one
SELECT
    *
FROM
    users
WHERE
    steamId = ?
LIMIT 1;

-- name: UpdateUsername :one
UPDATE
    users
SET
    username = ?
WHERE
    id = ?
RETURNING *;

-- name: UpdateAvatar :one
UPDATE
    users
SET
    Avatar = ?
WHERE
    id = ?
RETURNING *;

-- name: UpdateLastLoggedIn :one
UPDATE
    users
SET
    LastLoggedIn = ?
WHERE
    id = ?
RETURNING *;

-- name: GetSteamAvatarImage :one
SELECT
    Avatar
FROM
    users
WHERE
    steamId = ?
LIMIT 1;

-- name: NewGame :one
INSERT INTO
    games(player1Id, player2Id, player1Score, player2Score, winnerId, MatchTime)
VALUES
    (?, ?, ?, ?, ?, ?)
RETURNING *;

-- name: GetFaction :one
SELECT
    *
FROM
    faction
WHERE
    name = ?;

-- name: NewFaction :one
INSERT INTO
    faction(name, description)
VALUES
    (?, ?)
RETURNING *;

-- name: UpdateFaction :exec
UPDATE
    faction
SET
    description = ?
WHERE
    name = ?;

-- name: GetArmy :one
SELECT
    *
FROM
    army
WHERE
    name = ?;

-- name: NewArmy :one
INSERT INTO
    army(name, description)
VALUES
    (?, ?)
RETURNING *;

-- name: UpdateArmy :exec
UPDATE
    army
SET
    description = ?
WHERE
    name = ?;

-- name: GetUnit :one
SELECT
    *
FROM
    units
WHERE
    name = ?;

-- name: NewUnit :one
INSERT INTO
    units(name, attacks, movement, maxHP, AP, Speed, Armies)
VALUES
    (?, ?, ?, ?, ?, ?, ?)
RETURNING *;

-- name: UpdateUnit :exec
UPDATE units
SET
    attacks = ?,
    movement = ?,
    maxHP = ?,
    AP = ?,
    Speed = ?,
    Armies = ?
WHERE
    name = ?;

-- name: GetArmyFaction :one
SELECT
    *
FROM
    army_faction
WHERE
    armyId = ? AND factionId = ?;

-- name: NewArmyFaction :one
INSERT INTO
    army_faction(armyId, factionId)
VALUES
    (?, ?)
RETURNING *;

-- name: GetUnitArmy :one
SELECT
    *
FROM
    army_units
WHERE
    armyId = ? AND unitId = ?;

-- name: NewUnitArmy :one
INSERT INTO
    army_units(armyId, unitId)
VALUES
    (?, ?)
RETURNING *;

-- name: GetAllFactions :many
SELECT
    *
FROM
    faction;

-- name: GetAllArmies :many
SELECT
    *
FROM
    army;

-- name: GetAllUnits :many
SELECT
    *
FROM
    units;

-- name: GetArmyIdsForFaction :many
SELECT
    armyId
FROM
    army_faction
WHERE
    factionId = ?;

-- name: GetUnitIdsForArmy :many
SELECT
    unitId
FROM
    army_units
WHERE
    armyId = ?;

-- name: GetUnitsFaction :one
SELECT
    af.factionId
FROM
    army_units au
        JOIN
    army_faction af ON au.armyId = af.armyId
WHERE
    au.unitId = ?;