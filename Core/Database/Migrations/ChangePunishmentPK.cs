using System.Data;
using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20241218)]
public class ChangePunishmentPK : Migration {
    const string FKName = "Punishments_ibfk_1";

    public override void Up() {
        if (!Schema.Table(DbConstants.Punishments).Constraint(FKName).Exists())
            return;

        Delete.ForeignKey(FKName).OnTable(DbConstants.Punishments);

        Execute.Sql("ALTER TABLE `$(table)` DROP PRIMARY KEY, ADD PRIMARY KEY (`$(id)`, `$(playerId)`);", new Dictionary<string, string> {
            { "table", DbConstants.Punishments },
            { "id", DbConstants.Id },
            { "playerId", DbConstants.PlayerId }
        });

        Create.ForeignKey()
            .FromTable(DbConstants.Punishments).ForeignColumn(DbConstants.PlayerId)
            .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
            .OnDelete(Rule.Cascade);
    }

    public override void Down() { }
}
