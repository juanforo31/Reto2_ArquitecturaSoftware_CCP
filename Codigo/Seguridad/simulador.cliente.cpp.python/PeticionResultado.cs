public class PeticionResultado
{
    public String Manipulado { get; set; }
    public int Numero { get; set; }
    public int Status { get; set; }
    public DateTime FechaInicio { get; set; }
    public long TiempoRespuestaMs { get; set; }
    public Guid? PedidoId { get; set; }
    public int? Estado { get; set; }
    public double? Tiempo { get; set; }
    public string? Error { get; set; }
}