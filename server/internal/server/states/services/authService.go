package services

import (
	"context"
	"database/sql"
	"errors"
	"fmt"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/steam"
	"server/pkg/packets"
	"strings"

	"golang.org/x/crypto/bcrypt"
)

type AuthService struct {
	dbCtx   context.Context
	dbTx    *server.DbTx
	queries db.Queries
}

func NewAuthService(tx *server.DbTx) *AuthService {
	return &AuthService{
		dbCtx:   tx.Ctx,
		dbTx:    tx,
		queries: *tx.Queries,
	}
}

func (a *AuthService) GetUserExists(username string) bool {
	return false
}

func (a *AuthService) IsPasswordValid(password string) error {
	return nil
}
func (a *AuthService) Login(username, password string) (packets.Msg, error) {
	genericFailMessage := packets.NewDeny("Incorrect username or password")

	user, err := a.queries.GetUserByUsername(a.dbCtx, strings.ToLower(username))
	if err != nil {
		return genericFailMessage, err
	}
	err = bcrypt.CompareHashAndPassword([]byte(user.PasswordHash.String), []byte(password))
	if err != nil {
		return genericFailMessage, err
	}
	return packets.NewOK(), nil
}
func (a *AuthService) Register(username, password string) (packets.Msg, error) {
	err := validateUsername(username)
	if err != nil {
		return packets.NewDeny(err.Error()), err
	}
	err = a.IsPasswordValid(password)
	if err != nil {
		return packets.NewDeny(err.Error()), err
	}
	if _, err = a.queries.GetUserByUsername(a.dbCtx, strings.ToLower(username)); err == nil {
		return packets.NewDeny("Username already taken"), err
	}

	genericFailMessage := packets.NewDeny("Error registering user (internal server error)")

	hash, err := bcrypt.GenerateFromPassword([]byte(password), bcrypt.DefaultCost)
	if err != nil {
		return genericFailMessage, err
	}

	_, err = a.queries.CreateUser(a.dbCtx, db.CreateUserParams{
		Username: strings.ToLower(username),
		PasswordHash: sql.NullString{
			String: string(hash),
			Valid:  true,
		},
	})
	if err != nil {
		return genericFailMessage, err
	}

	return packets.NewOK(), nil
}

func validateUsername(username string) error {
	if len(username) < 3 || len(username) > 20 {
		return fmt.Errorf("username must be between 3 and 20 characters long")
	}

	return nil
}

func (a *AuthService) SteamLogin(ticket string, steam *steam.SteamWebClient) (packets.Msg, db.User, error) {
	user := db.User{}

	id, err := steam.AuthUser(a.dbCtx, ticket)
	if err != nil {
		return packets.NewDeny("Error authenticating Steam user"), user, err
	}

	user, err = a.queries.GetSteamUser(a.dbCtx, db.NewNullString(id.OwnerSteamID))
	if err == nil {
		return packets.NewOK(), user, nil
	}

	if !errors.Is(err, sql.ErrNoRows) {
		return packets.NewDeny("Error loading Steam user"), user, err
	}

	summ, err := steam.GetPlayerSummaries(a.dbCtx, id.SteamID)
	if err != nil {
		return packets.NewDeny("Error getting Steam user info"), user, err
	}

	if len(summ.Response.Players) == 0 {
		return packets.NewDeny("Steam user info not found"), user, fmt.Errorf("no Steam player summary returned for steam id %s", id.SteamID)
	}

	player := summ.Response.Players[0]

	user, err = a.queries.CreateSteamUser(a.dbCtx, db.CreateSteamUserParams{
		Username: player.PersonaName,
		Steamid:  db.NewNullString(player.SteamID),
	})
	if err != nil {
		return packets.NewDeny("Error creating Steam user"), user, err
	}

	return packets.NewOK(), user, nil
}
