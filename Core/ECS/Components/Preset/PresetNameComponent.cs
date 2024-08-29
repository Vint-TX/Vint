using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Preset;

[ProtocolId(1493974995307), ClientChangeable]
public class PresetNameComponent(
    string name
) : IComponent {
    public string Name { get; private set; } = name;

    public async Task Changed(IPlayerConnection connection, IEntity entity) {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 18) return;

        Player player = connection.Player;
        Database.Models.Preset preset = player.UserPresets.Single(p => p.Entity!.Id == entity.Id);
        preset.Name = Name;

        await using DbConnection db = new();
        await db.Presets
            .Where(p => p.PlayerId == player.Id && p.Index == preset.Index)
            .Set(p => p.Name, preset.Name)
            .UpdateAsync();
    }
}
