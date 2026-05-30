using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AlertaRepository : IAlertaRepository
{
    private readonly AppDbContext _db;

    public AlertaRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Alerta alerta, CancellationToken cancellationToken = default) =>
        await _db.Alertas.AddAsync(alerta, cancellationToken);

    public async Task<IReadOnlyList<Alerta>> BuscarAsync(int? pacienteId, bool? leida, CancellationToken cancellationToken = default)
    {
        var query = _db.Alertas.AsNoTracking().Include(a => a.Paciente).AsQueryable();

        if (pacienteId.HasValue)
            query = query.Where(a => a.PacienteId == pacienteId.Value);

        if (leida.HasValue)
            query = query.Where(a => a.Leida == leida.Value);

        return await query.OrderByDescending(a => a.FechaHora).ToListAsync(cancellationToken);
    }

    public async Task<int> ContarActivasAsync(CancellationToken cancellationToken = default) =>
        await _db.Alertas.AsNoTracking().CountAsync(a => !a.Leida, cancellationToken);

    public async Task<Alerta?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Alertas.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Alerta>> GetUltimasAsync(int cantidad, CancellationToken cancellationToken = default) =>
        await _db.Alertas.AsNoTracking()
            .Include(a => a.Paciente)
            .OrderByDescending(a => a.FechaHora)
            .Take(cantidad)
            .ToListAsync(cancellationToken);

    public void Update(Alerta alerta) =>
        _db.Alertas.Update(alerta);

    public void Remove(Alerta alerta) =>
        _db.Alertas.Remove(alerta);

    public async Task<IReadOnlyList<Alerta>> GetSosPorPacienteYFechaAsync(
        int pacienteId,
        DateTime fechaHora,
        TimeSpan ventana,
        CancellationToken cancellationToken = default)
    {
        var desde = fechaHora.Subtract(ventana);
        var hasta = fechaHora.Add(ventana);

        return await _db.Alertas
            .Where(a => a.PacienteId == pacienteId
                        && a.FechaHora >= desde
                        && a.FechaHora <= hasta
                        && (a.TipoAlerta == TipoAlerta.SosManual || a.TipoAlerta == TipoAlerta.SosAutomatico))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteAlertaMedicionEnVentanaAsync(
        int pacienteId,
        DateTime fechaHora,
        TimeSpan ventana,
        CancellationToken cancellationToken = default)
    {
        var desde = fechaHora.Subtract(ventana);
        var hasta = fechaHora.Add(ventana);

        return await _db.Alertas.AnyAsync(
            a => a.PacienteId == pacienteId
                 && a.FechaHora >= desde
                 && a.FechaHora <= hasta
                 && (a.TipoAlerta == TipoAlerta.FrecuenciaCardiaca
                     || a.TipoAlerta == TipoAlerta.SosAutomatico),
            cancellationToken);
    }
}
