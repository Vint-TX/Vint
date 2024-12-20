namespace Vint.Core.Server.Game.Protocol.Commands;

public interface ICommand;

public interface IServerCommand : ICommand {
    Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider);
}
