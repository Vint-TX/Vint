namespace Vint.Core.ECS.Enums;

public enum PaymentStatisticsAction : byte {
    OpenPayment = 0,
    CountrySelect = 1,
    ItemSelect = 2,
    ModeSelect = 3,
    Proceed = 4,
    ClosePayment = 5,
    OpenExchange = 6,
    PaymentError = 7,
    ConfirmedOneTimeOffer = 8
}