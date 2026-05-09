namespace Application.Dtos
{
    public record CrearRangoValoresMedicionDto(string TipoMedicion,
                                               int EdadMinima,
                                               int EdadMaxima,
                                               int ValorNormalMinimo,
                                               int ValorNormalMaximo,
                                               int ValorCriticoMinimo,
                                               int ValorCriticoMaximo);

    public record RangoValoresMedicionDto(string TipoMedicion,
                                               int EdadMinima,
                                               int EdadMaxima,
                                               int ValorNormalMinimo,
                                               int ValorNormalMaximo,
                                               int ValorCriticoMinimo,
                                               int ValorCriticoMaximo);



}
