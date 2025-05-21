using System.Data;
using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250521034430, BreakingChange = true)]
public class RemoveDiscord : Migration {
    public override void Up() {
        Delete.Table(DbConstants.DiscordLinks);

        Delete
            .Column("DiscordUserId")
            .Column("DiscordLinkRewarded")
            .Column("DiscordLinked")
            .FromTable(DbConstants.Players);
    }

    public override void Down() {
        Alter.Table(DbConstants.Players)
            .AddColumn("DiscordUserId").AsCustom("BIGINT UNSIGNED").NotNullable()
            .AddColumn("DiscordLinkRewarded").AsBoolean().NotNullable()
            .AddColumn("DiscordLinked").AsBoolean().NotNullable();

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
}
