using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnm.Application.Services;
using CassaComuneAnM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CassaComuneAnM.MauiAppUi;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // DB path
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cassacomune.db");
        builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));

        // Services
        builder.Services.AddScoped<ITripService, TripService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
