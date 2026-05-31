package packets

import (
	"server/internal/server/objects"
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

func NewSeed(seeds []int64) *Packet_Seed {
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
func NewVector3(x, y, z float32) *Vector3Msg {
	return &Vector3Msg{X: x, Y: y, Z: z}
}

func UnpackVector3(v *Vector3Msg) objects.Vector3 {
	return objects.Vector3{X: v.X, Y: v.Y, Z: v.Z}
}
