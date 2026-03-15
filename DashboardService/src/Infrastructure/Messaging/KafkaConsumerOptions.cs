namespace DashboardService.Infrastructure.Messaging;

public sealed class KafkaConsumerOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "dashboard-projections-v1";
    public List<string> Topics { get; set; } =
    [
        "orders.orderplaced.v1",
        "payments.paymentauthorized.v1",
        "payments.paymentfailed.v1",
        "promotions.pointsearned.v1",
        "promotions.pointsspent.v1",
        "promotions.loyaltyprofileupdated.v1"
    ];
}
