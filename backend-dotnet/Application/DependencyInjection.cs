using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPulseraApplication(this IServiceCollection services)
    {
        services.AddScoped<IClasificacionEstadoCardiaco, ClasificacionEstadoCardiaco>();
        services.AddScoped<IPacienteService, PacienteService>();
        services.AddScoped<IMedicionService, MedicionService>();
        services.AddScoped<IEventoEmergenciaService, EventoEmergenciaService>();
        services.AddScoped<IAlertaService, AlertaService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
