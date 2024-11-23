namespace Vint.Core.Server.Game.Protocol.Commands;

public interface ICommand {
    Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider);
}
