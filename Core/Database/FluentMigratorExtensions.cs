using FluentMigrator.Builders.Create.Table;

namespace Vint.Core.Utils;

public static class FluentMigratorExtensions {
    public static ICreateTableColumnOptionOrWithColumnSyntax AsSByte(this ICreateTableColumnAsTypeSyntax createTableColumnAsTypeSyntax) =>
        createTableColumnAsTypeSyntax.AsCustom("TINYINT SIGNED");

    public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt16(this ICreateTableColumnAsTypeSyntax createTableColumnAsTypeSyntax) =>
        createTableColumnAsTypeSyntax.AsCustom("SMALLINT UNSIGNED");

    public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt32(this ICreateTableColumnAsTypeSyntax createTableColumnAsTypeSyntax) =>
        createTableColumnAsTypeSyntax.AsCustom("INT UNSIGNED");

    public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt64(this ICreateTableColumnAsTypeSyntax createTableColumnAsTypeSyntax) =>
        createTableColumnAsTypeSyntax.AsCustom("BIGINT UNSIGNED");

    public static ICreateTableColumnOptionOrWithColumnSyntax AsBinString(this ICreateTableColumnAsTypeSyntax createTableColumnAsTypeSyntax, ushort size = 255) =>
        createTableColumnAsTypeSyntax.AsCustom($"varchar({size}) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_bin'");
}
