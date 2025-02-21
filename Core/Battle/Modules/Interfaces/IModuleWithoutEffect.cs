namespace Vint.Core.Battle.Modules.Interfaces;

public interface IModuleWithoutEffect {
    bool IsActive { get; }
    bool CanBeDeactivated { get; set; }

    Task Deactivate();
}
