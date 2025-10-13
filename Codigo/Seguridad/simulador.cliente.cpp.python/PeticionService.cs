using System.Text;
using System.Text.Json;

public class PeticionService
{
    private readonly HttpClient _httpClient = new();

    public async Task<PeticionResultado> EjecutarAsync(string url, int numero)
    {
        var inicio = DateTime.Now;
        var resultado = new PeticionResultado
        {
            Numero = numero,
            FechaInicio = inicio
        };

        try
        {
            // Cuerpo JSON a enviar
            //var payload = new
            //{
            //    solicitudId = Guid.NewGuid(),
            //    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            //};

            var clienteId = 456;

            PedidoDto payload = new PedidoDto
            {
                VendedorId = 123,
                ClienteId= clienteId,
                Detalle = new List<PedidoDetalleDto>(){
                    new PedidoDetalleDto(){ ProductoId = 1, Cantidad = 2},
                    new PedidoDetalleDto(){ ProductoId = 2, Cantidad = 3},
                }
            };
            string firmaCliente = new HmacHelper().GenerarHmac(payload);

            Random random = new Random();
            int numeroAleatorio = random.Next(1, 3);
            payload.ClienteId = numeroAleatorio == 1 ? payload.ClienteId : payload.ClienteId + numeroAleatorio;

            #region python
            var opciones = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(payload, opciones);
            #endregion

            // var json = JsonSerializer.Serialize(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Header personalizado
            _httpClient.DefaultRequestHeaders.Remove("X-Signature");
            _httpClient.DefaultRequestHeaders.Add("X-Signature", firmaCliente);

            var response = await _httpClient.PostAsync(url, content);
            resultado.Status = (int)response.StatusCode;
            var contenido = await response.Content.ReadAsStringAsync();
            resultado.TiempoRespuestaMs = (long)(DateTime.Now - inicio).TotalMilliseconds;

            using var doc = JsonDocument.Parse(contenido);
            var root = doc.RootElement;

            resultado.Manipulado = numeroAleatorio == 1 ? "NO" : "SI";

            if (response.IsSuccessStatusCode)
            {
                resultado.PedidoId = root.GetProperty("pedidoId").GetGuid();
                resultado.Estado = root.GetProperty("estado").GetInt32();
                resultado.Tiempo = root.GetProperty("tiempo").GetDouble();
            }
            else
            {
                resultado.Error = root.GetProperty("error").GetString();
                resultado.Estado = root.GetProperty("estado").GetInt32();
                resultado.Tiempo = root.GetProperty("tiempo").GetDouble();
            }
        }
        catch (Exception ex)
        {
            resultado.Status = 500;
            resultado.TiempoRespuestaMs = (long)(DateTime.Now - inicio).TotalMilliseconds;
            resultado.Error = ex.Message;
        }

        return resultado;
    }
}