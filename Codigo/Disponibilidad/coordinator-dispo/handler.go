package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"time"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
)

type Producto struct {
	ID       string `json:"id"`
	Nombre   string `json:"nombre"`
	Cantidad int    `json:"cantidad"`
}

type resultado struct {
	Nombre   string
	Producto *Producto
	Error    error
}

var urls = map[string]string{
	"NODE": "https://mgzd1xs0z1.execute-api.us-east-1.amazonaws.com/dev/producto/prod-1",
	"GO":   "https://i32qnrxbs5.execute-api.us-east-1.amazonaws.com/dev/producto/prod-1",
	"PY":   "https://gtkh92zyxb.execute-api.us-east-1.amazonaws.com/dev/producto/prod-1",
	"NET":  "https://2kdeu34h61.execute-api.us-east-1.amazonaws.com/producto/prod-1",
	"JAVA": "https://vs0ifqok48.execute-api.us-east-1.amazonaws.com/producto/prod-1",
}

func fetchProducto(nombre, url string, ch chan<- resultado) {
	client := &http.Client{Timeout: 5 * time.Second}
	resp, err := client.Get(url)
	if err != nil {
		ch <- resultado{Nombre: nombre, Error: fmt.Errorf("error llamando a %s: %w", nombre, err)}
		return
	}
	defer resp.Body.Close()

	if resp.StatusCode != 200 {
		ch <- resultado{Nombre: nombre, Error: fmt.Errorf("respuesta no exitosa de %s: %d", nombre, resp.StatusCode)}
		return
	}

	var producto Producto
	if err := json.NewDecoder(resp.Body).Decode(&producto); err != nil {
		ch <- resultado{Nombre: nombre, Error: fmt.Errorf("error decodificando respuesta de %s: %w", nombre, err)}
		return
	}

	ch <- resultado{Nombre: nombre, Producto: &producto}
}

func majorityCantidad(productos []*Producto) (int, map[int]int) {
	counts := make(map[int]int)
	for _, p := range productos {
		counts[p.Cantidad]++
	}

	var maxCount, majority int
	for cantidad, count := range counts {
		if count > maxCount {
			maxCount = count
			majority = cantidad
		}
	}
	return majority, counts
}

func handler(ctx context.Context, event events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	start := time.Now()

	ch := make(chan resultado)
	for nombre, url := range urls {
		go fetchProducto(nombre, url, ch)
	}

	respuestas := make(map[string]interface{})
	var productos []*Producto
	var coincidenConMayoría []string
	var noCoincidenConMayoría []string

	for i := 0; i < len(urls); i++ {
		r := <-ch
		if r.Error != nil {
			log.Printf("❌ %s falló: %v", r.Nombre, r.Error)
			respuestas[r.Nombre] = fmt.Sprintf("Error: %v", r.Error)
			noCoincidenConMayoría = append(noCoincidenConMayoría, r.Nombre) // Error técnico = falla
		} else {
			log.Printf("✅ %s respondió: %+v", r.Nombre, *r.Producto)
			respuestas[r.Nombre] = r.Producto
			productos = append(productos, r.Producto)
		}
	}

	majority, counts := majorityCantidad(productos)

	for nombre, data := range respuestas {
		if p, ok := data.(*Producto); ok {
			if p.Cantidad == majority {
				coincidenConMayoría = append(coincidenConMayoría, nombre)
			} else {
				log.Printf("⚠️ Inconsistencia: %s devolvió cantidad %d (esperado %d)", nombre, p.Cantidad, majority)
				noCoincidenConMayoría = append(noCoincidenConMayoría, nombre)
			}
		}
	}

	elapsed := time.Since(start)
	log.Printf("⏱️ Tiempo total de ejecución: %s", elapsed)

	body, _ := json.Marshal(map[string]interface{}{
		"respuestas":        respuestas,
		"cantidadMayoría":   majority,
		"conteoPorCantidad": counts,
		"tiempoTotal":       elapsed.String(),
		"Exitosos":          coincidenConMayoría,
		"Revisar":           noCoincidenConMayoría,
	})

	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Body:       string(body),
		Headers:    map[string]string{"Content-Type": "application/json"},
	}, nil
}

func main() {
	lambda.Start(handler)
}
