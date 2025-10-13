package com.example.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.amazonaws.services.lambda.runtime.events.APIGatewayProxyRequestEvent;
import com.amazonaws.services.lambda.runtime.events.APIGatewayProxyResponseEvent;
import com.fasterxml.jackson.databind.ObjectMapper;
import software.amazon.awssdk.services.dynamodb.DynamoDbClient;
import software.amazon.awssdk.services.dynamodb.model.*;

import java.util.Map;
import java.util.Random;

public class GetProductoLambda implements RequestHandler<APIGatewayProxyRequestEvent, APIGatewayProxyResponseEvent> {

    private final DynamoDbClient dynamoDb = DynamoDbClient.create();
    private static final ObjectMapper MAPPER = new ObjectMapper();
    private final String TABLE_NAME = System.getenv().getOrDefault("TABLE_NAME", "Productos");

    @Override
    public APIGatewayProxyResponseEvent handleRequest(APIGatewayProxyRequestEvent request, Context context) {
        String id = null;
        if (request.getPathParameters() != null) {
            id = request.getPathParameters().get("id");
        }

        if (id == null || id.isEmpty()) {
            context.getLogger().log("ID no proporcionado en pathParameters");
            return respuesta(400, "{\"message\":\"ID requerido\"}");
        }

        try {
            GetItemRequest getRequest = GetItemRequest.builder()
                    .tableName(TABLE_NAME)
                    .key(Map.of("id", AttributeValue.builder().s(id).build()))
                    .build();

            GetItemResponse result = dynamoDb.getItem(getRequest);

            if (result.item() == null || result.item().isEmpty()) {
                context.getLogger().log("Producto con ID " + id + " no encontrado");
                return respuesta(404, "{\"message\":\"Producto no encontrado\"}");
            }

            Map<String, AttributeValue> item = result.item();

            if (!item.containsKey("nombre") || !item.containsKey("cantidad")) {
                context.getLogger().log("Campos faltantes en el ítem");
                return respuesta(500, "{\"message\":\"Campos inválidos\"}");
            }

            String nombre = item.get("nombre").s();
            int cantidad;
            try {
                cantidad = Integer.parseInt(item.get("cantidad").n());
            } catch (NumberFormatException ex) {
                context.getLogger().log("Cantidad inválida: " + ex.getMessage());
                return respuesta(500, "{\"message\":\"Cantidad inválida\"}");
            }

            // Simulación: 50% chance duplicar
            Random rand = new Random();
            if (rand.nextDouble() < 0.5) {
                cantidad *= 2;
                context.getLogger().log("⚠️ Simulación activada: Cantidad duplicada a " + cantidad);
            }

            Producto producto = new Producto(id, nombre, cantidad);
            String body = MAPPER.writeValueAsString(producto);
            return respuesta(200, body);

        } catch (Exception e) {
            context.getLogger().log("Error procesando solicitud: " + e.getMessage());
            return respuesta(500, "{\"message\":\"Error interno\"}");
        }
    }

    private APIGatewayProxyResponseEvent respuesta(int status, String body) {
        return new APIGatewayProxyResponseEvent()
                .withStatusCode(status)
                .withBody(body)
                .withHeaders(Map.of("Content-Type", "application/json"));
    }

    public static class Producto {
        private String id;
        private String nombre;
        private int cantidad;

        public Producto() {}

        public Producto(String id, String nombre, int cantidad) {
            this.id = id;
            this.nombre = nombre;
            this.cantidad = cantidad;
        }
        public String getId() { return id; }
        public String getNombre() { return nombre; }
        public int getCantidad() { return cantidad; }
        public void setId(String id) { this.id = id; }
        public void setNombre(String nombre) { this.nombre = nombre; }
        public void setCantidad(int cantidad) { this.cantidad = cantidad; }
    }
}
