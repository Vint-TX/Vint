namespace Vint.Core.Battles.Modules.Interfaces;

public interface IModuleWithoutEffect {
    public bool IsActive { get; }
    public bool CanBeDeactivated { get; set; }

    public Task Deactivate();
}
