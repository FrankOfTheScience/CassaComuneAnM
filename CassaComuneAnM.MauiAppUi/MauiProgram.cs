using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnm.Application.Services;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.Infrastructure.Data;
using CassaComuneAnM.Infrastructure.Repositories;
using CassaComuneAnM.MauiAppUi.Services;
using CassaComuneAnM.MauiAppUi.ViewModels;
using CassaComuneAnM.MauiAppUi.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cassacomune.db");
        builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddScoped<ITripRepository, EfTripRepository>();
        builder.Services.AddScoped<IRepository<Expense>, EfRepository<Expense>>();
        builder.Services.AddScoped<IRepository<Deposit>, EfRepository<Deposit>>();
        builder.Services.AddScoped<ITripService, TripService>();
        builder.Services.AddSingleton<IAppDialogService, AppDialogService>();

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<CreateTripViewModel>();
        builder.Services.AddTransient<TripListViewModel>();
        builder.Services.AddTransient<TripDetailViewModel>();
        builder.Services.AddTransient<ParticipantViewModel>();
        builder.Services.AddTransient<ExpenseViewModel>();
        builder.Services.AddTransient<DepositViewModel>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CreateTripPage>();
        builder.Services.AddTransient<TripListPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();

        return app;
    }
}
