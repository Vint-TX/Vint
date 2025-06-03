using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250603230910)]
public class ImplementPremiumQuest : Migration {
    public override void Up() =>
        Alter.Table(DbConstants.Players)
            .AddColumn("PremiumQuestEndTime").AsDateTime().Nullable().WithDefaultValue(null);

    public override void Down() =>
        Delete.Column("PremiumQuestEndTime")
            .FromTable(DbConstants.Players);
}
