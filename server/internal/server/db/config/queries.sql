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
    users (username, steamId)
VALUES
    (?, ?)
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