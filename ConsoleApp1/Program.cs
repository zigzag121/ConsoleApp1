using ConsoleApp1.Models;
using ConsoleApp1.Services;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ConsoleApp1.Data;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Repositories;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;


            try
            {
                var depthChartService = services.GetRequiredService<DepthChartService>();
                // Clear existing players and depth chart entries
                var repository = services.GetRequiredService<IDepthChartRepository>();
                repository.RemoveAllPlayersAndEntries();
                // Specified Inputs
                var TomBrady = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
                var BlaineGabbert = new Player { PlayerId = 2, Number = 11, Name = "Blaine Gabbert" };
                var KyleTrask = new Player { PlayerId = 3, Number = 2, Name = "Kyle Trask" };
                var MikeEvans = new Player { PlayerId = 4, Number = 13, Name = "Mike Evans" };
                var JaelonDarden = new Player { PlayerId = 5, Number = 1, Name = "Jaelon Darden" };
                var ScottMiller = new Player { PlayerId = 6, Number = 10, Name = "Scott Miller" };

                depthChartService.AddPlayerToDepthChart("QB", TomBrady, 0);
                depthChartService.AddPlayerToDepthChart("QB", BlaineGabbert, 1);
                depthChartService.AddPlayerToDepthChart("QB", KyleTrask, 2);
                depthChartService.AddPlayerToDepthChart("LWR", MikeEvans, 0);
                depthChartService.AddPlayerToDepthChart("LWR", JaelonDarden, 1);
                depthChartService.AddPlayerToDepthChart("LWR", ScottMiller, 2);

                // Outputs
                Console.WriteLine("Backups for Tom Brady:");
                var backupsTomBrady = depthChartService.GetBackups("QB", TomBrady);
                foreach (var backup in backupsTomBrady)
                {
                    Console.WriteLine($"#{backup.Number} – {backup.Name}");
                }

                Console.WriteLine("Backups for Blaine Gabbert:");
                var backupsBlaineGabbert = depthChartService.GetBackups("QB", BlaineGabbert);
                foreach (var backup in backupsBlaineGabbert)
                {
                    Console.WriteLine($"#{backup.Number} – {backup.Name}");
                }

                Console.WriteLine("Backups for Kyle Trask:");
                var backupsKyleTrask = depthChartService.GetBackups("QB", KyleTrask);
                foreach (var backup in backupsKyleTrask)
                {
                    Console.WriteLine($"#{backup.Number} – {backup.Name}");
                }

                Console.WriteLine("Backups for Mike Evans:");
                var backupsMikeEvans = depthChartService.GetBackups("LWR", MikeEvans);
                foreach (var backup in backupsMikeEvans)
                {
                    Console.WriteLine($"#{backup.Number} – {backup.Name}");
                }

                Console.WriteLine("Full Depth Chart:");
                Console.WriteLine(depthChartService.GetFullDepthChart());

                Console.WriteLine("Removing Mike Evans from LWR:");
                var removedPlayer = depthChartService.RemovePlayerFromDepthChart("LWR", MikeEvans);
                if (removedPlayer != null)
                {
                    Console.WriteLine($"#{removedPlayer.Number} – {removedPlayer.Name} removed");
                }

                Console.WriteLine("Full Depth Chart After Removal:");
                Console.WriteLine(depthChartService.GetFullDepthChart());
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the depth chart.");
            }
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<DepthChartContext>(options =>
                    options.UseSqlite(connectionString));
                services.AddScoped<IDepthChartRepository, DepthChartRepository>();
                services.AddScoped<DepthChartService>();
                services.AddLogging(configure => configure.AddConsole());
            });
}