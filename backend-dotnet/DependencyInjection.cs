using Application.Abstractions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.Notifications;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPulseraInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en configuración.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<IMedicionRepository, MedicionRepository>();
        services.AddScoped<IAlertaRepository, AlertaRepository>();
        services.AddScoped<IEventoEmergenciaRepository, EventoEmergenciaRepository>();

        services.AddScoped<INotificacionContactoEmergencia, LoggingNotificacionContactoEmergencia>();

        return services;
    }
}
