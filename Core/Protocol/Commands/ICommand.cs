using Vint.Core.Server.Game;

namespace Vint.Core.Protocol.Commands;

public interface ICommand {
    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider);
}
