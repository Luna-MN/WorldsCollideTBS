package steam

type PlayerSummariesResponse struct {
	Response struct {
		Players []PlayerSummary `json:"players"`
	} `json:"response"`
}

type PlayerSummary struct {
	SteamID                  string `json:"steamid"`
	PersonaName              string `json:"personaname"`
	ProfileURL               string `json:"profileurl"`
	Avatar                   string `json:"avatar"`
	AvatarMedium             string `json:"avatarmedium"`
	AvatarFull               string `json:"avatarfull"`
	PersonaState             int    `json:"personastate"`
	CommunityVisibilityState int    `json:"communityvisibilitystate"`
}

type AuthenticateUserTicketResponse struct {
	Response AuthenticateUserTicketInnerResponse `json:"response"`
}

type AuthenticateUserTicketInnerResponse struct {
	Params AuthenticateUserTicketParams `json:"params"`
}

type AuthenticateUserTicketParams struct {
	Result          string `json:"result"`
	SteamID         string `json:"steamid"`
	OwnerSteamID    string `json:"ownersteamid"`
	VACBanned       bool   `json:"vacbanned"`
	PublisherBanned bool   `json:"publisherbanned"`
}
