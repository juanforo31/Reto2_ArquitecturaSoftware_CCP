using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaFunctionsCrearPedido
{

    public class Function
    {
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            double tiempoEjecucion = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                PedidoDto pedido = JsonConvert.DeserializeObject<PedidoDto>(request.Body);
                string firmaCliente;
                firmaCliente = request.Headers.Where(h => h.Key.Equals("X-Signature", StringComparison.OrdinalIgnoreCase)).Select(h => h.Value).FirstOrDefault();
                bool test = firmaCliente == null || String.IsNullOrEmpty(firmaCliente) ? false : true;

                if (!test)
                {
                    // parar tiempo transcurrido
                    stopwatch.Stop();
                    tiempoEjecucion = stopwatch.Elapsed.TotalMilliseconds;
                    Console.WriteLine($"Sin firma");
                    Console.WriteLine($"Tiempo de ejecución: {tiempoEjecucion} ms");
                    return
                    new APIGatewayProxyResponse
                    {
                        StatusCode = 401,
                        Body = JsonConvert.SerializeObject(
                            new
                            {
                                error = $"No Autorizado",
                                firmaCliente = firmaCliente,
                                pedido = pedido,
                                headers = request.Headers,
                                estado = 1,
                                tiempo = tiempoEjecucion
                            },
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }),
                        Headers = new Dictionary<string, string>
                        {
                        { "Contet-Type", "application/json" }
                        }
                    };
                }


                // Recalcular la firma en el servidor
                var firmaServidor = new HmacHelper().GenerarHmac(pedido);

                if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(firmaServidor),
                    Encoding.UTF8.GetBytes(firmaCliente)))
                {
                    // Se detecto Tampering
                    // Verificación de integridad de mensajes
                    // TODO: Guardar lo encontrado
                    // parar tiempo transcurrido
                    stopwatch.Stop();
                    tiempoEjecucion = stopwatch.Elapsed.TotalMilliseconds;
                    Console.WriteLine($"Falla detectada");
                    Console.WriteLine($"Tiempo de ejecución: {tiempoEjecucion} ms");
                    return
                   new APIGatewayProxyResponse
                   {
                       StatusCode = 401,
                       Body = JsonConvert.SerializeObject(
                           new
                           {
                               error = $"No Autorizado",
                               estado = 2,
                               tiempo = tiempoEjecucion
                           },
                           new JsonSerializerSettings
                           {
                               ContractResolver = new CamelCasePropertyNamesContractResolver()
                           }),
                       Headers = new Dictionary<string, string>
                       {
                        { "Contet-Type", "application/json" }
                       }
                   };
                }
                // parar tiempo transcurrido
                stopwatch.Stop();
                tiempoEjecucion = stopwatch.Elapsed.TotalMilliseconds;
                Console.WriteLine($"Operación normal");
                Console.WriteLine($"Tiempo de ejecución: {tiempoEjecucion} ms");
                return
                new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(
                        new
                        {
                            pedidoId = Guid.NewGuid(),
                            pedido = pedido,
                            estado = 3,
                            tiempo = tiempoEjecucion
                        },
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }),
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    }
                };

            }
            catch (JsonException e)
            {
                if (stopwatch != null && stopwatch.IsRunning)
                {
                    tiempoEjecucion = stopwatch.Elapsed.TotalMilliseconds;
                    Console.WriteLine($"Operación normal");
                    Console.WriteLine($"Tiempo de ejecución: {tiempoEjecucion} ms");
                    stopwatch.Stop();
                    Console.WriteLine("Stopwatch detenido.");
                }
                return
                new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(
                        new
                        {
                            error = $"An error ocurred while excuting the function {e.Message}",
                            estado = -1,
                            tiempo = tiempoEjecucion
                        }
                        ),
                    Headers = new Dictionary<string, string>
                    {
                        { "Contet-Type", "application/json" }
                    }
                };
            }

        }

    }

}

public class FirmaDto
{
    public string Key { get; set; } = string.Empty;
}

public class PedidoDetalleDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }

}

public class PedidoDto
{
    public int VendedorId { get; set; }
    public int ClienteId { get; set; }
    public List<PedidoDetalleDto> Detalle { get; set; }

}




