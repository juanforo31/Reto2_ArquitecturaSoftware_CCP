

# 🧪 Manual Informativo: Laboratorio de Disponibilidad con Lambdas y DynamoDB

## 🎯 Objetivo del Laboratorio

Evaluar la **disponibilidad y consistencia** de múltiples funciones Lambda escritas en distintos lenguajes, todas consultando una misma base de datos en **DynamoDB**, y verificar mediante una Lambda coordinadora cuáles respuestas son correctas y cuáles presentan inconsistencias.

---

## 🧱 Arquitectura del Experimento

### 🔹 Lambdas individuales (GET)

Cada Lambda está escrita en un lenguaje diferente y expone un endpoint que consulta DynamoDB para obtener los datos de un producto. Todas responden con un JSON que incluye:

```json
{
    "id": "prod-1",
    "nombre": "Controles",
    "cantidad": 300
}
```

#### Endpoints disponibles:

| Lenguaje | URL |
|---------|-----|
| NODE    | `https://mgzd1xs0z1.execute-api.us-east-1.amazonaws.com/dev/producto/prod-1` |
| GO      | `https://i32qnrxbs5.execute-api.us-east-1.amazonaws.com/dev/producto/prod-1` |
| PYTHON  | `https://gtkh92zyxb.execute-api.us-east-1.amazonaws.com/dev/producto/prod-1` |
| .NET    | `https://2kdeu34h61.execute-api.us-east-1.amazonaws.com/producto/prod-1` |
| JAVA    | `https://vs0ifqok48.execute-api.us-east-1.amazonaws.com/producto/prod-1` |

---

### 🔸 Lambda Coordinadora (POST)

Esta función Lambda recibe un `id` de producto y una lista de URLs. Llama a cada Lambda individual, recopila sus respuestas, detecta la **cantidad más común (mayoría)** y reporta cuáles Lambdas están alineadas y cuáles presentan discrepancias.

#### Endpoint coordinador:

```
POST https://7t1z515phf.execute-api.us-east-1.amazonaws.com/dev/producto
```



---

## 📊 Resultado Esperado

La Lambda coordinadora devuelve un JSON como este:

```json
{
    "Exitosos": [
        "NET",
        "GO",
        "PY"
    ],
    "Revisar": [
        "JAVA",
        "NODE"
    ],
    "cantidadMayoría": 300,
    "conteoPorCantidad": {
        "300": 3,
        "600": 2
    },
    "respuestas": {
        "GO": {
            "id": "prod-1",
            "nombre": "Controles",
            "cantidad": 300
        },
        "JAVA": {
            "id": "prod-1",
            "nombre": "Controles",
            "cantidad": 600
        },
        "NET": {
            "id": "prod-1",
            "nombre": "Controles",
            "cantidad": 300
        },
        "NODE": {
            "id": "prod-1",
            "nombre": "Controles",
            "cantidad": 600
        },
        "PY": {
            "id": "prod-1",
            "nombre": "Controles",
            "cantidad": 300
        }
    },
    "tiempoTotal": "4.710756921s"
}
```

### ✅ Interpretación

- **Cantidad mayoritaria**: 300
- Lambdas exitosas: NET, GO, PY
- Lambda para revisar: JAVA, NODE