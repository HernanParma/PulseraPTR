using System;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RangoValoresMedicionRepository : IRangoValoresMedicionRepository
{
    private readonly AppDbContext _db ;

    public RangoValoresMedicionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<RangoValoresMedicion>> GetAll()
    {
        return await _db.RangoValores.ToListAsync();
    }

    public async Task<IReadOnlyCollection<RangoValoresMedicion>> GetRangoSegunTipoMedicion(TipoMedicion tipoMed)
    {
        return await _db.RangoValores   
                         .Where(rv => rv.TipoMedicion == tipoMed)
                         .ToListAsync();
    }

    public async Task<RangoValoresMedicion> Insert(RangoValoresMedicion rango)
    {
        await _db.RangoValores.AddAsync(rango);
        return rango;
    }
}
