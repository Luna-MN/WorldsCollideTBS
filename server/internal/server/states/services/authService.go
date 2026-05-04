package services

import (
	"context"
	"fmt"
	"server/internal/server"
	"server/internal/server/db"
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
	err = bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(password))
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
		Username:     strings.ToLower(username),
		PasswordHash: string(hash),
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
