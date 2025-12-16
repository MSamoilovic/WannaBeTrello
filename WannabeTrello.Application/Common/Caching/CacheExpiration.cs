namespace WannabeTrello.Application.Common.Caching;

public static class CacheExpiration
{
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(1);      
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(5);  
    public static readonly TimeSpan Long = TimeSpan.FromMinutes(15);   
    public static readonly TimeSpan VeryLong = TimeSpan.FromMinutes(30);
}
