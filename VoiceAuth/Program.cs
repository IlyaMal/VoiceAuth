using Microsoft.AspNetCore.Mvc;

namespace VoiceAuth;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        
        builder.Services.AddScoped<IAuthBLL, AuthBLL >();
        builder.Services.AddScoped<IVoiceBLL, VoiceBLL>();
        builder.Configuration.AddJsonFile("config.json");

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddControllers();

        var app = builder.Build();
        

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();

       

        app.Run();
    }
}