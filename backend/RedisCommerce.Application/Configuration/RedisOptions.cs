namespace RedisCommerce.Application.Configuration;

public class RedisOptions
{
    public const string SectionName = "RedisSettings";

    public string ConnectionString { get; set; } = string.Empty;
    public int ProductTTLMinutes { get; set; } = 30;
    public int CartTTLHours { get; set; } = 24;
    public int SessionTTLMinutes { get; set; } = 30;
    public int AnalyticsTTLDays { get; set; } = 90;
    public int NotificationTTLDays { get; set; } = 30;
}
