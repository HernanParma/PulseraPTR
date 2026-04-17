using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EventoEmergenciaRepository : IEventoEmergenciaRepository
{
    private readonly AppDbContext _db;

    public EventoEmergenciaRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(EventoEmergencia evento, CancellationToken cancellationToken = default) =>
        await _db.EventosEmergencia.AddAsync(evento, cancellationToken);

    public async Task<IReadOnlyList<EventoEmergencia>> BuscarAsync(
        int? pacienteId,
        bool? atendido,
        TipoEventoEmergencia? tipoEvento,
        CancellationToken cancellationToken = default)
    {
        var query = _db.EventosEmergencia.AsNoTracking().Include(e => e.Paciente).AsQueryable();

        if (pacienteId.HasValue)
            query = query.Where(e => e.PacienteId == pacienteId.Value);

        if (atendido.HasValue)
            query = query.Where(e => e.Atendido == atendido.Value);

        if (tipoEvento.HasValue)
            query = query.Where(e => e.TipoEvento == tipoEvento.Value);

        return await query.OrderByDescending(e => e.FechaHora).ToListAsync(cancellationToken);
    }

    public async Task<int> ContarSosPendientesAsync(CancellationToken cancellationToken = default) =>
        await _db.EventosEmergencia.AsNoTracking()
            .CountAsync(e => e.TipoEvento == TipoEventoEmergencia.SOS && !e.Atendido, cancellationToken);

    public async Task<EventoEmergencia?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.EventosEmergencia.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<EventoEmergencia>> GetUltimosAsync(int cantidad, TipoEventoEmergencia tipoEvento, CancellationToken cancellationToken = default) =>
        await _db.EventosEmergencia.AsNoTracking()
            .Include(e => e.Paciente)
            .Where(e => e.TipoEvento == tipoEvento)
            .OrderByDescending(e => e.FechaHora)
            .Take(cantidad)
            .ToListAsync(cancellationToken);

    public void Update(EventoEmergencia evento) =>
        _db.EventosEmergencia.Update(evento);
}
