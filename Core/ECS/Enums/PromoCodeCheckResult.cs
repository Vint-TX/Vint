namespace Vint.Core.ECS.Enums;

public enum PromoCodeCheckResult : byte {
    Valid = 0,
    NotFound = 1,
    Used = 2,
    Expired = 3,
    Invalid = 4,
    Owned = 5
}