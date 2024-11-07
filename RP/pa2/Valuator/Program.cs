namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.        

        builder.Services.AddRazorPages();

        builder.Services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.Configuration = $"{RedisService.SERVER}:{RedisService.PORT}";
        });

        builder.Services.AddSingleton<IRedisService, RedisService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
