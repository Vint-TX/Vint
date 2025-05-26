using System.Data;
using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250526215345)]
public class AddEmailConfirmationsTable : Migration {
    public override void Up() {
        Create.Table(DbConstants.EmailConfirmations)
            .WithColumn(DbConstants.Id).AsInt64().PrimaryKey().Identity()
            .WithColumn(DbConstants.PlayerId).AsInt64().NotNullable().Indexed()
            .WithColumn("Token").AsBinString(64).NotNullable().Indexed()
            .WithColumn("OldEmail").AsBinString().Nullable()
            .WithColumn("NewEmail").AsBinString().NotNullable()
            .WithColumn("Used").AsBoolean().NotNullable()
            .WithColumn("Invalidated").AsBoolean().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("ExpiresAt").AsDateTime().NotNullable()
            .WithColumn("UsedAt").AsDateTime().Nullable();

        Create.ForeignKey()
            .FromTable(DbConstants.EmailConfirmations).ForeignColumn(DbConstants.PlayerId)
            .ToTable(DbConstants.Players).PrimaryColumn(DbConstants.Id)
            .OnDelete(Rule.Cascade);
    }

    public override void Down() =>
        Delete.Table(DbConstants.EmailConfirmations);
}
