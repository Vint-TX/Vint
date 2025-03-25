using LinqToDB.Mapping;
using Vint.Core.User;

namespace Vint.Core.Database.Models;

[Table(DbConstants.Reports)]
public class Report {
    [PrimaryKey(0)] public required long ReporterId { get; init; }
    [PrimaryKey(1)] public required long ReportedId { get; init; }

    [Column] public required InteractionSource InteractionSource { get; init; }
    [Column] public required DateTimeOffset CreatedAt { get; init; }
}
