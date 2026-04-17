using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly AppDbContext _db;

    public PacienteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Paciente paciente, CancellationToken cancellationToken = default) =>
        await _db.Pacientes.AddAsync(paciente, cancellationToken);

    public async Task<int> ContarActivosAsync(CancellationToken cancellationToken = default) =>
        await _db.Pacientes.AsNoTracking().CountAsync(p => p.Activo, cancellationToken);

    public async Task<IReadOnlyList<Paciente>> GetAllAsync(bool incluirInactivos, CancellationToken cancellationToken = default)
    {
        var query = _db.Pacientes.AsNoTracking().AsQueryable();
        if (!incluirInactivos)
            query = query.Where(p => p.Activo);

        return await query.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Update(Paciente paciente) =>
        _db.Pacientes.Update(paciente);
}
