using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Database;

public sealed class DbConnection() : DataConnection(Schema) {
    static DbConnection() {
        DefaultSettings = new DatabaseSettings();

        Schema = new MappingSchema();
        Schema.SetConverter<IEntity, long>(entity => entity.Id);
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        Schema.SetConverter<IEntity, DataParameter>(entity => new DataParameter(null, entity?.Id, DataType.Int64));
        Schema.SetConverter<long, IEntity>(id => GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == id));

        Schema.SetConverter<TimeSpan, long>(timeSpan => timeSpan.Ticks);
        Schema.SetConverter<TimeSpan, DataParameter>(timeSpan => new DataParameter(null, timeSpan.Ticks, DataType.Int64));
        Schema.SetConverter<long, TimeSpan>(TimeSpan.FromTicks);

        Schema.SetConverter<TimeSpan?, long?>(timeSpan => timeSpan?.Ticks);
        Schema.SetConverter<TimeSpan?, DataParameter>(timeSpan => new DataParameter(null, timeSpan?.Ticks, DataType.Int64));
        Schema.SetConverter<long?, TimeSpan?>(_ => null);
    }

    static MappingSchema Schema { get; set; }

    public ITable<Player> Players => this.GetTable<Player>();
    public ITable<Preset> Presets => this.GetTable<Preset>();
    public ITable<Avatar> Avatars => this.GetTable<Avatar>();
    public ITable<Cover> Covers => this.GetTable<Cover>();
    public ITable<Graffiti> Graffities => this.GetTable<Graffiti>();
    public ITable<Hull> Hulls => this.GetTable<Hull>();
    public ITable<HullSkin> HullSkins => this.GetTable<HullSkin>();
    public ITable<Paint> Paints => this.GetTable<Paint>();
    public ITable<Shell> Shells => this.GetTable<Shell>();
    public ITable<Weapon> Weapons => this.GetTable<Weapon>();
    public ITable<WeaponSkin> WeaponSkins => this.GetTable<WeaponSkin>();
    public ITable<Relation> Relations => this.GetTable<Relation>();
    public ITable<Module> Modules => this.GetTable<Module>();
    public ITable<Container> Containers => this.GetTable<Container>();
    public ITable<Statistics> Statistics => this.GetTable<Statistics>();
    public ITable<SeasonStatistics> SeasonStatistics => this.GetTable<SeasonStatistics>();
    public ITable<ReputationStatistics> ReputationStatistics => this.GetTable<ReputationStatistics>();
    public ITable<PresetModule> PresetModules => this.GetTable<PresetModule>();
    public ITable<Invite> Invites => this.GetTable<Invite>();
    public ITable<Punishment> Punishments => this.GetTable<Punishment>();
    public ITable<DiscordLink> DiscordLinks => this.GetTable<DiscordLink>();
    public ITable<Quest> Quests => this.GetTable<Quest>();
}
