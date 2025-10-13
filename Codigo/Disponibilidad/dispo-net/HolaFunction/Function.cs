using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HolaFunction;

public class Producto
{
    public string Id { get; set; }
    public string Nombre { get; set; }
    public int Cantidad { get; set; }
}

public class Function
{
    private readonly AmazonDynamoDBClient _dynamoDbClient = new();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (!request.PathParameters.TryGetValue("id", out var id))
        {
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Falta el parámetro 'id'" };
        }

        context.Logger.LogLine($"🔍 Buscando producto con ID: {id}");

        var getItemRequest = new GetItemRequest
        {
            TableName = "Productos",
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } }
            }
        };

        var response = await _dynamoDbClient.GetItemAsync(getItemRequest);

        if (!response.IsItemSet)
        {
            context.Logger.LogLine($"⚠️ Producto con ID '{id}' no encontrado.");
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "Producto no encontrado" };
        }

        var item = response.Item;
        if (!item.TryGetValue("nombre", out var nombreAttr) || !item.TryGetValue("cantidad", out var cantidadAttr))
        {
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "Campos faltantes en producto" };
        }

        var nombre = nombreAttr.S;
        if (!int.TryParse(cantidadAttr.N, out var cantidad))
        {
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "Cantidad inválida" };
        }

        // Simulación: 50% de probabilidad de duplicar cantidad
        if (Random.Shared.NextDouble() < 0.5)
        {
            cantidad *= 2;
            context.Logger.LogLine($"⚠️ Simulación activada: Cantidad duplicada a {cantidad}");
        }

        var producto = new Producto
        {
            Id = id,
            Nombre = nombre,
            Cantidad = cantidad
        };

        var json = JsonSerializer.Serialize(producto);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = json,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
