using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Battle.Player;

public interface IWithConnection {
    IPlayerConnection Connection { get; }
}
