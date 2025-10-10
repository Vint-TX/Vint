using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Presets.Components;

namespace Vint.Core.Battle.Autopilot.Connection;

[PublicAPI]
public class BotConnectionBuilder {
    static ExceptionThrower BotConnectionExceptionThrower { get; } = new(nameof(BotConnectionBuilder));

    int? Id { get; set; }
    IServiceScope ServiceScope { get; set; } = null!;
    PlayerBuilder PlayerBuilder { get; set; } = null!;

    public BotConnectionBuilder SetId(int id) {
        Id = id;
        return this;
    }

    public BotConnectionBuilder SetServiceScope(IServiceScope serviceScope) {
        ServiceScope = serviceScope;
        return this;
    }

    public BotConnectionBuilder SetPlayer(Action<PlayerBuilder> configure) {
        PlayerBuilder builder = new();
        configure(builder);
        PlayerBuilder = builder;
        return this;
    }

    public BotConnection Build() {
        Validate();

        Database.Models.Player player = PlayerBuilder.Build();
        BotConnection botConnection = new(Id!.Value, player, ServiceScope);
        return botConnection;
    }

    void Validate() {
        BotConnectionExceptionThrower.ThrowIfNull(Id);
        BotConnectionExceptionThrower.ThrowIfNull(ServiceScope);
        BotConnectionExceptionThrower.ThrowIfNull(PlayerBuilder);
    }

    public static implicit operator BotConnection(BotConnectionBuilder builder) => builder.Build();
}

[PublicAPI]
public class PlayerBuilder {
    static ExceptionThrower PlayerExceptionThrower { get; } = new(nameof(PlayerBuilder));

    long? Id { get; set; }
    string Username { get; set; } = null!;
    long? AvatarId { get; set; }
    long? Experience { get; set; }
    uint? Reputation { get; set; }
    PresetBuilder PresetBuilder { get; set; } = null!;

    public PlayerBuilder SetId(long id) {
        Id = id;
        return this;
    }

    public PlayerBuilder SetUsername(string username) {
        Username = username;
        return this;
    }

    public PlayerBuilder SetAvatar(IEntity marketItem) {
        AvatarId = marketItem.Id;
        return this;
    }

    public PlayerBuilder SetAvatar(long avatarId) {
        AvatarId = avatarId;
        return this;
    }

    public PlayerBuilder SetExperience(long experience) {
        Experience = experience;
        return this;
    }

    public PlayerBuilder SetReputation(uint reputation) {
        Reputation = reputation;
        return this;
    }

    public PlayerBuilder SetPreset(Action<PresetBuilder> configure) {
        PresetBuilder presetBuilder = new();
        configure(presetBuilder);
        PresetBuilder = presetBuilder;
        return this;
    }

    public Database.Models.Player Build() {
        Validate();

        Database.Models.Player player = new() {
            Id = Id!.Value,
            Username = Username,
            CurrentAvatarId = AvatarId!.Value,
            Experience = Experience!.Value,
            Reputation = Reputation!.Value,
            // Default values for other properties
            Email = default!,
            PasswordHash = default!,
            HardwareFingerprint = default!,
            NewsletterUnsubscribeToken = default!,
            NewsletterSubscribed = default!,
            CountryCode = default!,
            RegistrationTime = default,
            LastLoginTime = default,
            LastQuestUpdateTime = default
        };

        Preset preset = PresetBuilder.Build(player);

        player.UserPresets.Add(preset);
        player.CurrentPresetIndex = preset.Index;
        return player;
    }

    void Validate() {
        PlayerExceptionThrower.ThrowIfNull(Id);
        PlayerExceptionThrower.ThrowIfNullOrWhiteSpace(Username);
        PlayerExceptionThrower.ThrowIfNull(AvatarId);
        PlayerExceptionThrower.ThrowIfNull(Experience);
        PlayerExceptionThrower.ThrowIfNull(Reputation);
        PlayerExceptionThrower.ThrowIfNull(PresetBuilder);
    }

    public static implicit operator Database.Models.Player(PlayerBuilder builder) => builder.Build();
}

[PublicAPI]
public class PresetBuilder { // todo modules
    static ExceptionThrower PresetExceptionThrower { get; } = new(nameof(PresetBuilder));

    int? Index { get; set; }
    string Name { get; set; } = null!;
    IEntity Weapon { get; set; } = null!;
    IEntity Hull { get; set; } = null!;
    IEntity WeaponSkin { get; set; } = null!;
    IEntity HullSkin { get; set; } = null!;
    IEntity Cover { get; set; } = null!;
    IEntity Paint { get; set; } = null!;
    IEntity Shell { get; set; } = null!;
    IEntity Graffiti { get; set; } = null!;

    public PresetBuilder SetIndex(int index) {
        Index = index;
        return this;
    }

    public PresetBuilder SetName(string name) {
        Name = name;
        return this;
    }

    public PresetBuilder SetWeapon(IEntity weapon) {
        Weapon = weapon;
        return this;
    }

    public PresetBuilder SetHull(IEntity hull) {
        Hull = hull;
        return this;
    }

    public PresetBuilder SetWeaponSkin(IEntity weaponSkin) {
        WeaponSkin = weaponSkin;
        return this;
    }

    public PresetBuilder SetHullSkin(IEntity hullSkin) {
        HullSkin = hullSkin;
        return this;
    }

    public PresetBuilder SetCover(IEntity cover) {
        Cover = cover;
        return this;
    }

    public PresetBuilder SetPaint(IEntity paint) {
        Paint = paint;
        return this;
    }

    public PresetBuilder SetShell(IEntity shell) {
        Shell = shell;
        return this;
    }

    public PresetBuilder SetGraffiti(IEntity graffiti) {
        Graffiti = graffiti;
        return this;
    }

    public Preset Build(Database.Models.Player player) {
        Validate();

        Preset preset = new() {
            Player = player,
            Index = Index!.Value,
            Name = Name,
            Weapon = Weapon,
            Hull = Hull,
            WeaponSkin = WeaponSkin,
            HullSkin = HullSkin,
            Cover = Cover,
            Paint = Paint,
            Shell = Shell,
            Graffiti = Graffiti,
        };

        // uurghdhahfhhsshhhhh...
        IEntity userItem = GlobalEntities.GetEntity("misc", "Preset");

        userItem.TemplateAccessor!.Template = ((MarketEntityTemplate)userItem.TemplateAccessor.Template).UserTemplate;
        userItem.Id = EntityRegistry.GenerateId();

        userItem.AddComponent(new PresetEquipmentComponent(preset));

        preset.Entity = userItem;
        return preset;
    }

    void Validate() {
        PresetExceptionThrower.ThrowIfNull(Index);
        PresetExceptionThrower.ThrowIfNullOrWhiteSpace(Name);
        PresetExceptionThrower.ThrowIfNull(Weapon);
        PresetExceptionThrower.ThrowIfNull(Hull);
        PresetExceptionThrower.ThrowIfNull(WeaponSkin);
        PresetExceptionThrower.ThrowIfNull(HullSkin);
        PresetExceptionThrower.ThrowIfNull(Cover);
        PresetExceptionThrower.ThrowIfNull(Paint);
        PresetExceptionThrower.ThrowIfNull(Shell);
        PresetExceptionThrower.ThrowIfNull(Graffiti);
    }
}

class ExceptionThrower(
    string builderName
) {
    string Template { get; } = $"{{0}} must be set before building from {builderName}";

    [ContractAnnotation("value:null => halt")]
    public void ThrowIfNull(object? value, [CallerArgumentExpression(nameof(value))] string? propertyName = null) {
        if (value == null)
            throw new InvalidOperationException(string.Format(Template, propertyName));
    }

    [ContractAnnotation("value:null => halt")]
    public void ThrowIfNullOrWhiteSpace(string? value, [CallerArgumentExpression(nameof(value))] string? propertyName = null) {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(string.Format(Template, propertyName));
    }
}
