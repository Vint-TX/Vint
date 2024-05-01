using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Preset;

[ProtocolId(1493974995307), ClientChangeable]
public class PresetNameComponent( // wtf is this shit? todo refactor
    Database.Models.Preset? preset
) : IComponent {
    string? _name;

    public string Name {
        get => preset != null ? preset.Name : _name!;
        set {
            if (preset != null) preset.Name = value;
            else _name = value;
        }
    }

    public async Task Changed(IPlayerConnection connection, IEntity entity) {
        if (_name?.Length > 18) return;

        preset ??= connection.Player.UserPresets.Single(p => p.Entity!.Id == entity.Id);
        if (_name != null) preset.Name = _name;

        await using DbConnection db = new();
        await db.UpdateAsync(preset);
    }
}
