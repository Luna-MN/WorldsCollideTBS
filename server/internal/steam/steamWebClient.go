package steam

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"
	"strings"
)

type Client struct {
	APIKey string
	HTTP   *http.Client
}

func NewClient(apiKey string) *Client {
	return &Client{
		APIKey: apiKey,
		HTTP:   http.DefaultClient,
	}
}

func (c *Client) GetPlayerSummaries(ctx context.Context, steamIDs []string) (*PlayerSummariesResponse, error) {
	endpoint := "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/"

	query := url.Values{}
	query.Set("key", c.APIKey)
	query.Set("steamids", strings.Join(steamIDs, ","))

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
