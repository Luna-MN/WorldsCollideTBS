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