using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MedicionRepository : IMedicionRepository
{
    private readonly AppDbContext _db;

    public MedicionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Medicion medicion, CancellationToken cancellationToken = default) =>
        await _db.Mediciones.AddAsync(medicion, cancellationToken);

    public async Task<IReadOnlyList<Medicion>> BuscarAsync(
        int? pacienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        EstadoClinico? estado,
        bool? soloFueraDeRango,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Mediciones.AsNoTracking().Include(m => m.Paciente).AsQueryable();

        if (pacienteId.HasValue)
            query = query.Where(m => m.PacienteId == pacienteId.Value);

        if (fechaDesde.HasValue)
            query = query.Where(m => m.FechaHora >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(m => m.FechaHora <= fechaHasta.Value);

        if (estado.HasValue)
            query = query.Where(m => m.Estado == estado.Value);

        if (soloFueraDeRango == true)
            query = query.Where(m => m.EsFueraDeRango);

        return await query.OrderByDescending(m => m.FechaHora).ToListAsync(cancellationToken);
    }

    public async Task<int> ContarFueraDeRangoAsync(CancellationToken cancellationToken = default) =>
        await _db.Mediciones.AsNoTracking().CountAsync(m => m.EsFueraDeRango, cancellationToken);

    public async Task<Medicion?> GetUltimaPorPacienteAsync(int pacienteId, CancellationToken cancellationToken = default) =>
        await _db.Mediciones.AsNoTracking()
            .Where(m => m.PacienteId == pacienteId)
            .OrderByDescending(m => m.FechaHora)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Medicion?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Mediciones.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public void Remove(Medicion medicion) =>
        _db.Mediciones.Remove(medicion);
}
