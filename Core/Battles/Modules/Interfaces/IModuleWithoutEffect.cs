namespace Vint.Core.Battles.Modules.Interfaces;

public interface IModuleWithoutEffect {
    public bool CanBeDeactivated { get; set; }

    public Task Deactivate();
}
