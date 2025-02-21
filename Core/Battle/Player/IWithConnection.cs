using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Player;

public interface IWithConnection {
    IPlayerConnection Connection { get; }
}
