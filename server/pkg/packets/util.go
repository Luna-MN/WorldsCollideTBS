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
func NewVector3(x, y, z float32) *Vector3Msg {
	return &Vector3Msg{X: x, Y: y, Z: z}
}

func UnpackVector3(v *Vector3Msg) objects.Vector3 {
	return objects.Vector3{X: v.X, Y: v.Y, Z: v.Z}
}
