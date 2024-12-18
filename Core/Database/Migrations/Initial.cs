using System.Data;
using FluentMigrator;
using Vint.Core.Utils;

namespace Vint.Core.Database.Migrations;

[Migration(20241213)]
public class Initial : Migration {
    public override void Up() {
        if (!Schema.Table(DbConstants.Invites).Exists()) {
            Create.Table(DbConstants.Invites)
                .WithColumn(DbConstants.Id).AsInt64().PrimaryKey().Identity().NotNullable()
                .WithColumn("Code").AsString(64).Unique().NotNullable()
                .WithColumn("RemainingUses").AsUInt16().NotNullable();
        }

        if (!Schema.Table(DbConstants.PromoCodes).Exists()) {
            Create.Table(DbConstants.PromoCodes)
                .WithColumn(DbConstants.Id).AsInt64().PrimaryKey().Identity().NotNullable()
                .WithColumn("Code").AsString(32).Unique().NotNullable()
                .WithColumn("Uses").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("MaxUses").AsInt32().NotNullable().WithDefaultValue(-1)
                .WithColumn("OwnedPlayerId").AsInt64().NotNullable().WithDefaultValue(-1)
                .WithColumn("ExpiresAt").AsDateTime().Nullable().WithDefaultValue(null);
        }

        if (!Schema.Table(DbConstants.Players).Exists()) {
            Create.Table(DbConstants.Players)
                .WithColumn(DbConstants.Id).AsInt64().PrimaryKey().NotNullable()
                .WithColumn("Username").AsBinString().Unique().NotNullable()
                .WithColumn("Email").AsBinString().Unique().NotNullable()
                .WithColumn("DiscordUserId").AsUInt64().NotNullable()
                .WithColumn("RememberMe").AsBoolean().NotNullable()
                .WithColumn("AutoLoginToken").AsBinary(255).Nullable().WithDefaultValue(null)
                .WithColumn("PasswordHash").AsBinary(255).NotNullable()
                .WithColumn("HardwareFingerprint").AsString().NotNullable()
                .WithColumn("Groups").AsInt32().NotNullable()
                .WithColumn("RewardedLeagues").AsInt32().NotNullable()
                .WithColumn("DiscordLinkRewarded").AsBoolean().NotNullable()
                .WithColumn("DiscordLinked").AsBoolean().NotNullable()
                .WithColumn("Subscribed").AsBoolean().NotNullable()
                .WithColumn("CountryCode").AsString().NotNullable()
                .WithColumn("CurrentAvatarId").AsInt64().NotNullable()
                .WithColumn("CurrentPresetIndex").AsInt32().NotNullable()
                .WithColumn("Crystals").AsInt64().NotNullable()
                .WithColumn("XCrystals").AsInt64().NotNullable()
                .WithColumn("GoldBoxItems").AsInt32().NotNullable()
                .WithColumn("Experience").AsInt64().NotNullable()
                .WithColumn("FractionName").AsString().NotNullable()
                .WithColumn("FractionScore").AsInt64().NotNullable()
                .WithColumn("RegistrationTime").AsDateTime().NotNullable()
                .WithColumn("LastLoginTime").AsDateTime().NotNullable()
                .WithColumn("LastQuestUpdateTime").AsDateTime().NotNullable()
                .WithColumn("QuestChangesResetTime").AsDateTime().Nullable().WithDefaultValue(null)
                .WithColumn("QuestChanges").AsInt32().NotNullable()
                .WithColumn("LastLoginRewardTime").AsDateTime().NotNullable().WithDefaultValue(new DateTime(2024, 1, 1, 0, 0, 0))
                .WithColumn("LastLoginRewardDay").AsInt32().NotNullable()
                .WithColumn("Reputation").AsUInt32().NotNullable()
                .WithColumn("GameplayChestScore").AsInt64().NotNullable()
                .WithColumn("DesertedBattlesCount").AsInt64().NotNullable()
                .WithColumn("NeedGoodBattlesCount").AsInt32().NotNullable();
        }

        if (!Schema.Table(DbConstants.Avatars).Exists()) {
            Create.Table(DbConstants.Avatars)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Avatars)
                .Columns(DbConstants.PlayerId, DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Avatars).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Containers).Exists()) {
            Create.Table(DbConstants.Containers)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("Count").AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Containers)
                .Columns(DbConstants.PlayerId, DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Containers).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Covers).Exists()) {
            Create.Table(DbConstants.Covers)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Covers)
                .Columns(DbConstants.PlayerId, DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Covers).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.DiscordLinks).Exists()) {
            Create.Table(DbConstants.DiscordLinks)
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("UserId").AsUInt64().NotNullable()
                .WithColumn("TokenExpirationDate").AsDateTime().NotNullable()
                .WithColumn("AccessToken").AsBinString().NotNullable()
                .WithColumn("RefreshToken").AsBinString().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.DiscordLinks)
                .Columns(DbConstants.PlayerId, "UserId");

            Create.ForeignKey()
                .FromTable(DbConstants.DiscordLinks).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Graffities).Exists()) {
            Create.Table(DbConstants.Graffities)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Graffities)
                .Columns(DbConstants.PlayerId, DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Graffities).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Hulls).Exists()) {
            Create.Table(DbConstants.Hulls)
                .WithColumn("Xp").AsInt64().NotNullable()
                .WithColumn("SkinId").AsInt64().NotNullable()
                .WithColumn("Kills").AsInt64().NotNullable()
                .WithColumn("BattlesPlayed").AsInt64().NotNullable()
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Hulls)
                .Columns(DbConstants.PlayerId, DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Hulls).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.HullSkins).Exists()) {
            Create.Table(DbConstants.HullSkins)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn("HullId").AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.HullSkins)
                .Columns(DbConstants.PlayerId, "HullId", DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.HullSkins).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.HullSkins).ForeignColumns(DbConstants.PlayerId, "HullId")
                .ToTable(DbConstants.Hulls).PrimaryColumns(DbConstants.PlayerId, DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Modules).Exists()) {
            Create.Table(DbConstants.Modules)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("Level").AsInt32().NotNullable()
                .WithColumn("Cards").AsInt32().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Modules)
                .Columns(DbConstants.PlayerId, DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Modules).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Paints).Exists()) {
            Create.Table(DbConstants.Paints)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Paints)
                .Columns(DbConstants.PlayerId, DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Paints).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.PromoCodeItems).Exists()) {
            Create.Table(DbConstants.PromoCodeItems)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn("PromoCodeId").AsInt64().NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.PromoCodeItems)
                .Columns("PromoCodeId", DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.PromoCodeItems).ForeignColumn("PromoCodeId")
                .ToTable(DbConstants.PromoCodes).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.PromoCodeRedemptions).Exists()) {
            Create.Table(DbConstants.PromoCodeRedemptions)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn("PromoCodeId").AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("RedeemedAt").AsDateTime().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.PromoCodeRedemptions)
                .Columns(DbConstants.Id, "PromoCodeId", DbConstants.PlayerId);

            Create.UniqueConstraint().OnTable(DbConstants.PromoCodeRedemptions)
                .Columns("PromoCodeId", DbConstants.PlayerId);

            Create.ForeignKey()
                .FromTable(DbConstants.PromoCodeRedemptions).ForeignColumn("PromoCodeId")
                .ToTable(DbConstants.PromoCodes).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.PromoCodeRedemptions).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Alter.Column(DbConstants.Id)
                .OnTable(DbConstants.PromoCodeRedemptions)
                .AsInt64().Identity().NotNullable();
        }

        if (!Schema.Table(DbConstants.Punishments).Exists()) {
            Create.Table(DbConstants.Punishments)
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn("IPAddress").AsString().Nullable().WithDefaultValue(null)
                .WithColumn("HardwareFingerprint").AsString().NotNullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("PunishTime").AsDateTime().NotNullable()
                .WithColumn("Duration").AsInt64().Nullable().WithDefaultValue(null)
                .WithColumn("Reason").AsString().Nullable().WithDefaultValue(null)
                .WithColumn("Active").AsBoolean().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Punishments)
                .Columns(DbConstants.Id, DbConstants.PlayerId);

            Create.ForeignKey()
                .FromTable(DbConstants.Punishments).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Alter.Column(DbConstants.Id)
                .OnTable(DbConstants.Punishments)
                .AsInt64().Identity().NotNullable();
        }

        if (!Schema.Table(DbConstants.Quests).Exists()) {
            Create.Table(DbConstants.Quests)
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("ProgressCurrent").AsInt32().NotNullable()
                .WithColumn("ProgressTarget").AsInt32().NotNullable()
                .WithColumn("RewardEntity").AsInt64().NotNullable()
                .WithColumn("RewardAmount").AsInt32().NotNullable()
                .WithColumn("Rarity").AsByte().NotNullable()
                .WithColumn("CompletionDate").AsDateTime().Nullable().WithDefaultValue(null)
                .WithColumn("Condition").AsByte().Nullable().WithDefaultValue(null)
                .WithColumn("ConditionValue").AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Quests)
                .Columns(DbConstants.PlayerId, "Index");

            Create.ForeignKey()
                .FromTable(DbConstants.Quests).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Relations).Exists()) {
            Create.Table(DbConstants.Relations)
                .WithColumn("SourcePlayerId").AsInt64().NotNullable()
                .WithColumn("TargetPlayerId").AsInt64().NotNullable()
                .WithColumn("Types").AsInt32().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Relations)
                .Columns("SourcePlayerId", "TargetPlayerId");

            Create.ForeignKey()
                .FromTable(DbConstants.Relations).ForeignColumn("SourcePlayerId")
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.Relations).ForeignColumn("TargetPlayerId")
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.ReputationStatistics).Exists()) {
            Create.Table(DbConstants.ReputationStatistics)
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("Date").AsDate().NotNullable()
                .WithColumn("SeasonNumber").AsUInt32().NotNullable()
                .WithColumn("Reputation").AsUInt32().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.ReputationStatistics)
                .Columns(DbConstants.PlayerId, "Date");

            Create.ForeignKey()
                .FromTable(DbConstants.ReputationStatistics).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.SeasonStatistics).Exists()) {
            Create.Table(DbConstants.SeasonStatistics)
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("SeasonNumber").AsUInt32().NotNullable()
                .WithColumn("Reputation").AsUInt32().NotNullable()
                .WithColumn("BattlesPlayed").AsUInt32().NotNullable()
                .WithColumn("Kills").AsUInt32().NotNullable()
                .WithColumn("ExperienceEarned").AsUInt32().NotNullable()
                .WithColumn("CrystalsEarned").AsUInt32().NotNullable()
                .WithColumn("XCrystalsEarned").AsUInt32().NotNullable()
                .WithColumn("ContainersOpened").AsUInt32().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.SeasonStatistics)
                .Columns(DbConstants.PlayerId, "SeasonNumber");

            Create.ForeignKey()
                .FromTable(DbConstants.SeasonStatistics).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Statistics).Exists()) {
            Create.Table(DbConstants.Statistics)
                .WithColumn(DbConstants.PlayerId).AsInt64().PrimaryKey().NotNullable()
                .WithColumn("BattlesParticipated").AsInt64().NotNullable()
                .WithColumn("AllBattlesParticipated").AsInt64().NotNullable()
                .WithColumn("AllCustomBattlesParticipated").AsInt64().NotNullable()
                .WithColumn("Victories").AsInt64().NotNullable()
                .WithColumn("Defeats").AsInt64().NotNullable()
                .WithColumn("CrystalsEarned").AsUInt64().NotNullable()
                .WithColumn("XCrystalsEarned").AsUInt64().NotNullable()
                .WithColumn("Kills").AsUInt32().NotNullable()
                .WithColumn("Deaths").AsUInt32().NotNullable()
                .WithColumn("FlagsDelivered").AsUInt32().NotNullable()
                .WithColumn("FlagsReturned").AsUInt32().NotNullable()
                .WithColumn("GoldBoxesCaught").AsUInt32().NotNullable()
                .WithColumn("Shots").AsUInt32().NotNullable()
                .WithColumn("Hits").AsUInt32().NotNullable();

            Create.ForeignKey()
                .FromTable(DbConstants.Statistics).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Weapons).Exists()) {
            Create.Table(DbConstants.Weapons)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn("Xp").AsInt64().NotNullable()
                .WithColumn("SkinId").AsInt64().NotNullable()
                .WithColumn("ShellId").AsInt64().NotNullable()
                .WithColumn("Kills").AsInt64().NotNullable()
                .WithColumn("BattlesPlayed").AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Weapons)
                .Columns(DbConstants.PlayerId, DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Weapons).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Shells).Exists()) {
            Create.Table(DbConstants.Shells)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn("WeaponId").AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Shells)
                .Columns(DbConstants.PlayerId, "WeaponId", DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.Shells).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.Shells).ForeignColumns(DbConstants.PlayerId, "WeaponId")
                .ToTable(DbConstants.Weapons).PrimaryColumns(DbConstants.PlayerId, DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.WeaponSkins).Exists()) {
            Create.Table(DbConstants.WeaponSkins)
                .WithColumn(DbConstants.Id).AsInt64().NotNullable()
                .WithColumn("WeaponId").AsInt64().NotNullable()
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.WeaponSkins)
                .Columns(DbConstants.PlayerId, "WeaponId", DbConstants.Id);

            Create.ForeignKey()
                .FromTable(DbConstants.WeaponSkins).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.WeaponSkins).ForeignColumns(DbConstants.PlayerId, "WeaponId")
                .ToTable(DbConstants.Weapons).PrimaryColumns(DbConstants.PlayerId, DbConstants.Id)
                .OnDelete(Rule.Cascade);
        }

        if (!Schema.Table(DbConstants.Presets).Exists()) {
            Create.Table(DbConstants.Presets)
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                .WithColumn("Name").AsString(4000).NotNullable()
                .WithColumn("Weapon").AsInt64().NotNullable()
                .WithColumn("Hull").AsInt64().NotNullable()
                .WithColumn("WeaponSkin").AsInt64().NotNullable()
                .WithColumn("HullSkin").AsInt64().NotNullable()
                .WithColumn("Cover").AsInt64().NotNullable()
                .WithColumn("Paint").AsInt64().NotNullable()
                .WithColumn("Shell").AsInt64().NotNullable()
                .WithColumn("Graffiti").AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.Presets)
                .Columns(DbConstants.PlayerId, "Index");

            Create.ForeignKey()
                .FromTable(DbConstants.Presets).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.Presets).ForeignColumns(DbConstants.PlayerId, "Weapon")
                .ToTable(DbConstants.Weapons).PrimaryColumns(DbConstants.PlayerId, "Id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.Presets).ForeignColumns(DbConstants.PlayerId, "Hull")
                .ToTable(DbConstants.Hulls).PrimaryColumns(DbConstants.PlayerId, "Id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey("FK_Presets_WeaponSkins")
                .FromTable(DbConstants.Presets).ForeignColumns(DbConstants.PlayerId, "Weapon", "WeaponSkin")
                .ToTable(DbConstants.WeaponSkins).PrimaryColumns(DbConstants.PlayerId, "WeaponId", "Id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey("FK_Presets_HullSkins")
                .FromTable(DbConstants.Presets).ForeignColumns(DbConstants.PlayerId, "Hull", "HullSkin")
                .ToTable(DbConstants.HullSkins).PrimaryColumns(DbConstants.PlayerId, "HullId", "Id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.Presets).ForeignColumns(DbConstants.PlayerId, "Cover")
                .ToTable(DbConstants.Covers).PrimaryColumns(DbConstants.PlayerId, "Id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.Presets).ForeignColumns(DbConstants.PlayerId, "Paint")
                .ToTable(DbConstants.Paints).PrimaryColumns(DbConstants.PlayerId, "Id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey("FK_Presets_Shells")
                .FromTable(DbConstants.Presets).ForeignColumns(DbConstants.PlayerId, "Weapon", "Shell")
                .ToTable(DbConstants.Shells).PrimaryColumns(DbConstants.PlayerId, "WeaponId", "Id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.Presets).ForeignColumns(DbConstants.PlayerId, "Graffiti")
                .ToTable(DbConstants.Graffities).PrimaryColumns(DbConstants.PlayerId, "Id")
                .OnDelete(Rule.Cascade);
        }

        // ReSharper disable once InvertIf
        if (!Schema.Table(DbConstants.PresetModules).Exists()) {
            Create.Table(DbConstants.PresetModules)
                .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable()
                .WithColumn("PresetIndex").AsInt32().NotNullable()
                .WithColumn("Slot").AsByte().NotNullable()
                .WithColumn("Entity").AsInt64().NotNullable();

            Create.PrimaryKey().OnTable(DbConstants.PresetModules)
                .Columns(DbConstants.PlayerId, "PresetIndex", "Slot");

            Create.ForeignKey()
                .FromTable(DbConstants.PresetModules).ForeignColumn(DbConstants.PlayerId)
                .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
                .OnDelete(Rule.Cascade);

            Create.ForeignKey()
                .FromTable(DbConstants.PresetModules).ForeignColumns(DbConstants.PlayerId, "PresetIndex")
                .ToTable(DbConstants.Presets).PrimaryColumns(DbConstants.PlayerId, "Index")
                .OnDelete(Rule.Cascade);
        }
    }

    public override void Down() => throw new NotSupportedException();
}
