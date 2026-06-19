package packets

import (
	"server/internal/server/db"
	"server/internal/server/objects"
	"server/internal/server/objects/units/units/util"
)

type Msg = isPacket_Msg

func NewChat(msg string, msgType ChatType) *Packet_Chat {
	return &Packet_Chat{
		Chat: &ChatMessage{
			Msg:  msg,
			Type: msgType,
		},
	}
}

func NewId(id uint64) *Packet_Id {
	return &Packet_Id{
		Id: &IdMessage{
			Id: id,
		},
	}
}

func NewOK() Msg {
	return &Packet_OK{
		OK: &OkResponseMessage{},
	}
}

func NewDeny(reason string) Msg {
	return &Packet_Deny{
		Deny: &DenyResponseMessage{
			Reason: reason,
		},
	}
}

func NewQueue(queue string) Msg {
	return &Packet_Queue{Queue: &QueueMessage{
		QueueType: queue,
	}}
}

func NewOpponent(id string, steam bool, avatarImage string) Msg {
	return &Packet_Opponent{
		Opponent: &OpponentMessage{
			Id:          id,
			Steam:       steam,
			AvatarImage: avatarImage,
		},
	}
}

func NewStartGame() Msg {
	return &Packet_StartGame{
		StartGame: &StartGameMessage{},
	}
}

func NewSeed(seeds []int32) *Packet_Seed {
	return &Packet_Seed{
		Seed: &SeedMessage{
			Seed: seeds,
		},
	}
}

func NewIds(Ids []uint64) *Packet_IDs {
	var ids []*IdMessage
	for _, id := range Ids {
		ids = append(ids, &IdMessage{Id: id})
	}
	return &Packet_IDs{
		IDs: &IDsMessage{
			IDs: ids,
		},
	}
}

func NewHexPosition(position objects.Vector3) *HexPositionMessage {
	return &HexPositionMessage{
		Position: NewVector2(position.X, position.Z),
	}
}

func NewHexPositions(id uint64, positions []objects.Vector3) *Packet_HexPositions {
	var hexPositions []*HexPositionMessage
	for _, position := range positions {
		hexPositions = append(hexPositions, NewHexPosition(position))
	}
	return &Packet_HexPositions{
		HexPositions: &HexPositionsMessage{
			Id:        id,
			Positions: hexPositions,
		},
	}
}

func NewUnitData(data util.UnitData) *UnitMessage {
	return &UnitMessage{
		OwnerId:  data.OwnerID,
		UnitId:   data.UnitID,
		Attacks:  data.Attacks,
		Movement: data.Movement,
		AP:       data.AP,
		HP:       data.HP,
		MaxHP:    data.MaxHP,
		Speed:    Speeds(data.Speed),
		UnitName: data.UnitName,
		Pos:      NewVector3(data.Pos.X, data.Pos.Y, data.Pos.Z),
	}
}

func UnwrapUnitData(UnitPacket *UnitMessage) *util.UnitData {
	return util.NewUnitData(UnitPacket.UnitName, UnitPacket.GetOwnerId(), UnitPacket.GetUnitId(), UnitPacket.Attacks, UnitPacket.Movement, UnitPacket.AP, UnitPacket.HP, UnitPacket.MaxHP, util.Speed(UnitPacket.Speed), UnwrapVector3(UnitPacket.Pos))
}

func NewUnitDataFromPacket(UnitPacket *UnitMessage) *util.UnitData {
	return util.NewUnitData(UnitPacket.UnitName, UnitPacket.GetOwnerId(), UnitPacket.GetUnitId(), UnitPacket.Attacks, UnitPacket.Movement, UnitPacket.AP, UnitPacket.HP, UnitPacket.MaxHP, util.Speed(UnitPacket.Speed), UnwrapVector3(UnitPacket.Pos))
}

func NewFactionDataFromDB(f db.Faction, armies []int64) *FactionData {
	return &FactionData{
		Id:          f.ID,
		Name:        f.Name,
		Description: f.Description.String,
		ArmyIds:     armies,
	}
}

func NewArmyDataFromDB(a db.Army, units []int64) *ArmyData {
	return &ArmyData{
		Id:          a.ID,
		Name:        a.Name,
		Description: a.Description.String,
		UnitIds:     units,
	}
}

func NewUnitDataFromDB(u db.Unit, factionID int64) *UnitData {
	return &UnitData{
		Id:        u.ID,
		Name:      u.Name,
		Attacks:   u.Attacks.String,
		Movement:  u.Movement.String,
		MaxHP:     u.Maxhp.Int64,
		AP:        u.Ap.Int64,
		Speed:     Speeds(u.Speed.Int64),
		FactionId: factionID,
	}
}

func NewVersionMessage(version string) *GameDataVersion {
	return &GameDataVersion{
		Version: version,
	}
}

func NewVersionPacket(version string) *Packet_GameVersion {
	return &Packet_GameVersion{
		GameVersion: NewVersionMessage(version),
	}
}

func NewData(version string, fs []*FactionData, as []*ArmyData, us []*UnitData) *Packet_GameData {
	return &Packet_GameData{
		GameData: &GameDataMessage{
			Version:  NewVersionMessage(version),
			Factions: fs,
			Armies:   as,
			Units:    us,
		},
	}
}

func NewArmyId(id int64) *Packet_ArmyId {
	return &Packet_ArmyId{ArmyId: &ArmyIDMessage{Id: id}}
}

func NewVector3(x, y, z float32) *Vector3Msg {
	return &Vector3Msg{X: x, Y: y, Z: z}
}

func UnwrapVector3(v *Vector3Msg) objects.Vector3 {
	return objects.Vector3{X: v.X, Y: v.Y, Z: v.Z}
}

func NewVector2(x, y float32) *Vector2Msg {
	return &Vector2Msg{X: x, Y: y}
}
