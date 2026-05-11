package steam

import (
	"context"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"server/internal"
	"strconv"
)

type SteamWebClient struct {
	APIKey string
	HTTP   *http.Client
}

func NewClient(apiKey string) *SteamWebClient {
	return &SteamWebClient{
		APIKey: apiKey,
		HTTP:   http.DefaultClient,
	}
}

func (c *SteamWebClient) GetPlayerSummaries(ctx context.Context, steamIDs string) (*PlayerSummariesResponse, error) {
	endpoint := "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/"

	query := url.Values{}
	query.Set("key", c.APIKey)
	query.Set("steamids", steamIDs)

	request, err := http.NewRequestWithContext(ctx, http.MethodGet, endpoint+"?"+query.Encode(), nil)
	if err != nil {
		return nil, err
	}

	response, err := c.HTTP.Do(request)
	if err != nil {
		return nil, err
	}
	defer response.Body.Close()

	if response.StatusCode >= 300 {
		return nil, fmt.Errorf("steam api returned status %d", response.StatusCode)
	}

	var out PlayerSummariesResponse
	if err := json.NewDecoder(response.Body).Decode(&out); err != nil {
		return nil, err
	}

	return &out, nil
}

func (c *SteamWebClient) AuthUser(ctx context.Context, ticket string, identity string) (*AuthenticateUserTicketParams, error) {
	endpoint := "https://partner.steam-api.com/ISteamUserAuth/AuthenticateUserTicket/v1/"
	ticketHex := hex.EncodeToString([]byte(ticket))
	query := url.Values{}
	query.Set("key", c.APIKey)
	query.Set("appid", strconv.Itoa(internal.SteamAppId))
	query.Set("ticket", ticketHex)
	request, err := http.NewRequestWithContext(ctx, http.MethodGet, endpoint+"?"+query.Encode(), nil)
	if err != nil {
		return nil, err
	}

	response, err := c.HTTP.Do(request)
	if err != nil {
		return nil, err
	}
	defer response.Body.Close()

	if response.StatusCode >= 300 {
		body, _ := io.ReadAll(response.Body)
		return nil, fmt.Errorf("steam auth returned status %d: %s", response.StatusCode, string(body))
	}

	var out AuthenticateUserTicketResponse
	if err := json.NewDecoder(response.Body).Decode(&out); err != nil {
		return nil, err
	}

	if out.Response.Params.Result != "OK" {
		fmt.Println(out)
		return nil, fmt.Errorf("steam auth failed: %s", out.Response.Params.Result)
	}

	return &out.Response.Params, nil
}
