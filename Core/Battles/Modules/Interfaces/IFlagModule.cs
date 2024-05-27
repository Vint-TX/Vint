namespace Vint.Core.Battles.Modules.Interfaces;

public interface IFlagModule {
    public Task OnFlagAction(FlagAction action);
}

public enum FlagAction {
    Capture,
    Drop,
    Return,
    Deliver
}
