using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Invites")]
public class Invite {
    [PrimaryKey, Identity] public long Id { get; init; }
    [Column] public string Code { get; init; } = null!;
    [Column] public ushort RemainingUses { get; set; }
}