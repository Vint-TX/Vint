namespace Vint.Core.ECS.Events.Payment;

public enum PaymentStatisticsAction {
    OpenPayment,
    CountrySelect,
    ItemSelect,
    ModeSelect,
    Proceed,
    ClosePayment,
    OpenExchange,
    PaymentError,
    ConfirmedOneTimeOffer
}