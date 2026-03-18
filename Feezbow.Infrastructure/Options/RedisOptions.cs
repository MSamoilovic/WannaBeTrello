namespace WannabeTrello.Infrastructure.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "wannabetrello";
    public int DefaultExpirationMinutes { get; set; } = 5;
    public bool Enabled { get; set; } = true;
}
