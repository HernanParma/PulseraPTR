using Application;
using Application.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PulseraPTR.Hubs;
using PulseraPTR.Middleware;
using PulseraPTR.Services;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// User-secrets (GlucoseEmailImport:Password, etc.): cargar en Development aunque la compilaci�n sea Release.
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddPulseraInfrastructure(builder.Configuration);
builder.Services.AddPulseraApplication(builder.Configuration);
builder.Services.AddScoped<IPulseraRealtimeNotifier, PulseraRealtimeNotifier>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PulseraPTR API",
        Version = "v1",
        Description = "Backend de monitoreo para pulsera inteligente PulseraPTR."
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var seedDemo = configuration.GetValue("Pulsera:SeedDemoData", false);
    await DataSeeder.SeedAsync(db, logger, seedDemo);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

// En Development NO redirigimos HTTP ? HTTPS: el perfil Kestrel expone HTTPS solo en localhost:7052
// y HTTP en 0.0.0.0:5093. Si activamos UseHttpsRedirection aqu�, el celular que entra por
// http://192.168.x.x:5093 recibe un 307 a https://192.168.x.x:7052, donde no hay listener
// (el certificado dev tampoco sirve bien en el tel�fono). Para LAN + APK + Swagger us� HTTP 5093.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<PulseraHub>("/hubs/pulsera");

app.Run();
