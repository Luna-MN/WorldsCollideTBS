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

