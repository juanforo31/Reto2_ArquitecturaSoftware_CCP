using System.Text.Json;

public class PeticionService1
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
            var response = await _httpClient.GetAsync(url);
            resultado.Status = (int)response.StatusCode;
            var contenido = await response.Content.ReadAsStringAsync();
            resultado.TiempoRespuestaMs = (long)(DateTime.Now - inicio).TotalMilliseconds;

            using var doc = JsonDocument.Parse(contenido);
            var root = doc.RootElement;

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