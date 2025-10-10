using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Battle.Autopilot.Connection;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Autopilot;

public class BotBuilder( // todo
    GameServer server,
    IServiceScopeFactory serviceScopeFactory
) {
    static IList<string> AvailableAvatars { get; } = [ // todo from config
        "Jokerge",
        "Crab",
        "Tankist"
    ];

    static IList<string> AvailableWeapons { get; } = [
        "Smoky",
        "Twins",
        "Thunder",
        "Ricochet"
    ];

    static IList<string> AvailableHulls { get; } = [
        "Hunter",
        "Hornet",
        "Viking"
    ];

    static IList<string> AvailableCovers { get; } = [
        "None"
    ];

    static IList<string> AvailablePaints { get; } = [
        "Green",
        "Lobster",
        "Swamp",
        "Bananas",
        "League_bronze"
    ];

    static IList<string> AvailableGraffities { get; } = [
        "Logo",
        "Xmas1"
    ];

    static FrozenDictionary<long, IList<string>> AvailableWeaponSkins { get; } = new Dictionary<long, IList<string>> {
        [GlobalEntities.GetEntity("weapons", "Smoky").Id] = [
            "SmokyM0",
            "SmokyM1gold",
            "SmokyM1steel"
        ],
        [GlobalEntities.GetEntity("weapons", "Twins").Id] = [
            "TwinsM0",
            "TwinsM1gold",
            "TwinsM1steel"
        ],
        [GlobalEntities.GetEntity("weapons", "Thunder").Id] = [
            "ThunderM0",
            "ThunderM1gold",
            "ThunderM1steel"
        ],
        [GlobalEntities.GetEntity("weapons", "Hammer").Id] = [
            "HammerM0",
            "HammerM1gold",
            "HammerM1steel"
        ],
        [GlobalEntities.GetEntity("weapons", "Ricochet").Id] = [
            "RicochetM0",
            "RicochetM1gold",
            "RicochetM1steel"
        ]
    }.ToFrozenDictionary();

    static FrozenDictionary<long, IList<string>> AvailableHullSkins { get; } = new Dictionary<long, IList<string>> {
        [GlobalEntities.GetEntity("hulls", "Hunter").Id] = [
            "HunterM0",
            "HunterM1gold",
            "HunterM1steel"
        ],
        [GlobalEntities.GetEntity("hulls", "Hornet").Id] = [
            "HornetM0",
            "HornetM1gold",
            "HornetM1steel"
        ],
        [GlobalEntities.GetEntity("hulls", "Viking").Id] = [
            "VikingM0",
            "VikingM1gold",
            "VikingM1steel"
        ],
    }.ToFrozenDictionary();

    static FrozenDictionary<long, IList<string>> AvailableShells { get; } = new Dictionary<long, IList<string>> {
        [GlobalEntities.GetEntity("weapons", "Smoky").Id] = [
            "SmokyStandard"
        ],
        [GlobalEntities.GetEntity("weapons", "Twins").Id] = [
            "TwinsBlue"
        ],
        [GlobalEntities.GetEntity("weapons", "Thunder").Id] = [
            "ThunderStandard"
        ],
        [GlobalEntities.GetEntity("weapons", "Hammer").Id] = [
            "HammerStandard"
        ],
        [GlobalEntities.GetEntity("weapons", "Ricochet").Id] = [
            "RicochetAurulent"
        ]
    }.ToFrozenDictionary();

    public async Task<BotConnection> ConnectNewBot(string nickname) {
        AsyncServiceScope serviceScope = serviceScopeFactory.CreateAsyncScope();

        IEntity weapon = GetRandomWeapon();
        IEntity hull = GetRandomHull();

        BotConnection bot = new BotConnectionBuilder()
            .SetId(server.GenerateConnectionId())
            .SetServiceScope(serviceScope)
            .SetPlayer(player => player
                .SetId(EntityRegistry.GenerateId())
                .SetUsername(nickname)
                .SetAvatar(GetRandomAvatar())
                .SetExperience(0)
                .SetReputation(0)
                .SetPreset(preset => preset
                    .SetIndex(0)
                    .SetName("жопа")
                    .SetWeapon(weapon)
                    .SetHull(hull)
                    .SetWeaponSkin(GetRandomWeaponSkin(weapon.Id))
                    .SetHullSkin(GetRandomHullSkin(hull.Id))
                    .SetCover(GetRandomCover())
                    .SetPaint(GetRandomPaint())
                    .SetShell(GetRandomShell(weapon.Id))
                    .SetGraffiti(GetRandomGraffiti())));

        await server.PlayerConnected(bot);
        return bot;
    }

    static IEntity GetRandomAvatar() => GetRandomEntity("avatars", AvailableAvatars);

    static IEntity GetRandomWeapon() => GetRandomEntity("weapons", AvailableWeapons);

    static IEntity GetRandomHull() => GetRandomEntity("hulls", AvailableHulls);

    static IEntity GetRandomWeaponSkin(long weaponId) => GetRandomEntity("weaponSkins", AvailableWeaponSkins[weaponId]);

    static IEntity GetRandomHullSkin(long hullId) => GetRandomEntity("hullSkins", AvailableHullSkins[hullId]);

    static IEntity GetRandomCover() => GetRandomEntity("covers", AvailableCovers);

    static IEntity GetRandomPaint() => GetRandomEntity("paints", AvailablePaints);

    static IEntity GetRandomShell(long weaponId) => GetRandomEntity("shells", AvailableShells[weaponId]);

    static IEntity GetRandomGraffiti() => GetRandomEntity("graffities", AvailableGraffities);

    static IEntity GetRandomEntity(string type, IList<string> list) =>
        GlobalEntities.GetEntity(type, list.RandomElement());
}
