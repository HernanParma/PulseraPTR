using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Mapping;

namespace Application.Services;

public class AlertaService : IAlertaService
{
    private readonly IAlertaRepository _alertas;
    private readonly IEventoEmergenciaRepository _eventos;
    private readonly IUnitOfWork _unitOfWork;

    public AlertaService(
        IAlertaRepository alertas,
        IEventoEmergenciaRepository eventos,
        IUnitOfWork unitOfWork)
    {
        _alertas = alertas;
        _eventos = eventos;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AlertaDto>> ListarAsync(int? pacienteId, bool? leida, CancellationToken cancellationToken = default)
    {
        var items = await _alertas.BuscarAsync(pacienteId, leida, cancellationToken);
        return items.OrderByDescending(a => a.FechaHora).Select(a => a.ToDto()).ToList();
    }

    public Task<int> ContarSinLeerAsync(CancellationToken cancellationToken = default) =>
        _alertas.ContarActivasAsync(cancellationToken);

    public async Task<IReadOnlyList<AlertaDto>> ListarRecientesAsync(
        int cantidad = 12,
        CancellationToken cancellationToken = default)
    {
        var items = await _alertas.GetUltimasAsync(cantidad, cancellationToken);
        return items.Select(a => a.ToDto()).ToList();
    }

    public async Task MarcarLeidaAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _alertas.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la alerta {id}.");

        entity.Leida = true;
        _alertas.Update(entity);

        await SosAlertaSynchronizer.MarcarEventoSosAtendidoSiExisteAsync(_eventos, entity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _alertas.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la alerta {id}.");

        _alertas.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
