namespace Domain.Enums;

public enum TransferTrigger
{
    TimeBased, // transform after x days
    Manual, // admin/platform approval
    DeliveryConfirmed, // buyer confirms delivery,
    ShipmentConfirmed
}