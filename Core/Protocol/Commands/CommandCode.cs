namespace Vint.Core.Protocol.Commands;

public enum CommandCode : byte {
    SendEvent = 1,
    EntityShare = 2,
    EntityUnshare = 3,
    ComponentAdd = 4,
    ComponentRemove = 5,
    ComponentChange = 6,
    InitTime = 7,
    Close = 9
}