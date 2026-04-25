using Application.Configuration;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPulseraApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GlucoseAlertOptions>(configuration.GetSection(GlucoseAlertOptions.SectionName));
        services.Configure<GlucoseDashboardOptions>(configuration.GetSection(GlucoseDashboardOptions.SectionName));
        services.Configure<GlucoseEmailImportOptions>(configuration.GetSection(GlucoseEmailImportOptions.SectionName));

        services.AddScoped<IClasificacionEstadoCardiaco, ClasificacionEstadoCardiaco>();
        services.AddScoped<IPacienteService, PacienteService>();
        services.AddScoped<IMedicionService, MedicionService>();
        services.AddScoped<IEventoEmergenciaService, EventoEmergenciaService>();
        services.AddScoped<IAlertaService, AlertaService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddScoped<IGlucoseAlertEvaluator, GlucoseAlertEvaluator>();
        services.AddScoped<IGlucoseImportService, GlucoseImportService>();
        services.AddScoped<IGlucoseReadingsQueryService, GlucoseReadingsQueryService>();

        return services;
    }
}
