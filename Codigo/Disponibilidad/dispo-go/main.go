package main

import (
	"context"
	"encoding/json"
	"log"
	"math/rand"
	"strconv"
	"time"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
)

type Producto struct {
	ID       string `json:"id"`
	Nombre   string `json:"nombre"`
	Cantidad int    `json:"cantidad"`
}

func getProducto(ctx context.Context, request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	id := request.PathParameters["id"]
	if id == "" {
		log.Println("ID no proporcionado en pathParameters")
		return events.APIGatewayProxyResponse{StatusCode: 400, Body: "ID requerido"}, nil
	}

	cfg, err := config.LoadDefaultConfig(ctx)
	if err != nil {
		log.Printf("Error cargando configuración AWS: %v", err)
		return events.APIGatewayProxyResponse{StatusCode: 500, Body: "Error interno"}, nil
	}

	svc := dynamodb.NewFromConfig(cfg)

	result, err := svc.GetItem(ctx, &dynamodb.GetItemInput{
		TableName: aws.String("Productos"),
		Key: map[string]types.AttributeValue{
			"id": &types.AttributeValueMemberS{Value: id},
		},
	})
	if err != nil {
		log.Printf("Error en GetItem: %v", err)
		return events.APIGatewayProxyResponse{StatusCode: 500, Body: "Error al consultar producto"}, nil
	}

	if result.Item == nil {
		log.Printf("Producto con ID %s no encontrado", id)
		return events.APIGatewayProxyResponse{StatusCode: 404, Body: "Producto no encontrado"}, nil
	}

	nombreAttr, ok := result.Item["nombre"].(*types.AttributeValueMemberS)
	if !ok {
		log.Println("Campo 'nombre' no encontrado o tipo incorrecto")
		return events.APIGatewayProxyResponse{StatusCode: 500, Body: "Campo 'nombre' inválido"}, nil
	}

	cantidadAttr, ok := result.Item["cantidad"].(*types.AttributeValueMemberN)
	if !ok {
		log.Println("Campo 'cantidad' no encontrado o tipo incorrecto")
		return events.APIGatewayProxyResponse{StatusCode: 500, Body: "Campo 'cantidad' inválido"}, nil
	}

	cantidad, err := strconv.Atoi(cantidadAttr.Value)
	if err != nil {
		log.Printf("Error convirtiendo cantidad: %v", err)
		return events.APIGatewayProxyResponse{StatusCode: 500, Body: "Cantidad inválida"}, nil
	}

	// Simulación: 50% de probabilidad de duplicar cantidad
	rand.Seed(time.Now().UnixNano())
	chance := rand.Float64()
	if chance < 0.5 {
		cantidad *= 2
		log.Printf("⚠️ Simulación activada: Cantidad duplicada a %d", cantidad)
	}

	producto := Producto{
		ID:       id,
		Nombre:   nombreAttr.Value,
		Cantidad: cantidad,
	}

	body, err := json.Marshal(producto)
	if err != nil {
		log.Printf("Error serializando respuesta: %v", err)
		return events.APIGatewayProxyResponse{StatusCode: 500, Body: "Error al generar respuesta"}, nil
	}

	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Body:       string(body),
	}, nil
}

func main() {
	lambda.Start(getProducto)
}
