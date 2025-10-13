
# 🔐 Manual Informativo: Laboratorio de Seguridad

## 🎯 Objetivo del Experimento

Validar la integridad de mensajes entre cliente y servidor mediante **HMAC con SHA-256**, simulando ataques de manipulación (tampering) y aplicando la táctica de **verificación de autenticidad**. Se mide el tiempo de verificación en milisegundos y se documenta el flujo de respuesta ante alteraciones.

---

## 🧪 Flujo del Experimento

### 1️⃣ Preparación del JSON en el Cliente

Se arma un JSON representando un pedido:

```json
{
  "pedidoId": "12345",
  "producto": "Laptop",
  "cantidad": 1,
  "precio": 1500
}
```

Este JSON debe tener **orden y formato consistente**, ya que cualquier cambio afecta la firma.

---

### 2️⃣ Generación de Firma HMAC (Cliente)

Se genera una firma HMAC usando:

- Algoritmo: `SHA-256`
- Llave secreta compartida
- Cuerpo del mensaje (JSON)

La firma se agrega en la cabecera HTTP:

```
X-Signature: <firma-generada>
```

---

### 3️⃣ Envío de la Petición POST

Se envía el JSON como cuerpo y la firma en la cabecera:

```http
POST /verificar-pedido
Content-Type: application/json
X-Signature: <firma-generada>

{ ...json del pedido... }
```

En Postman se puede usar un script para generar la firma automáticamente antes de enviar la petición.

---

### 4️⃣ Verificación en el Servidor

El servidor:

- Extrae la firma del cliente (`X-Signature`)
- Genera su propia firma usando el mismo algoritmo y llave
- Compara ambas firmas

---

### 5️⃣ Detección de Tampering

Si las firmas **no coinciden**:

- Se retorna `401 Unauthorized`
- Se registra el intento de manipulación

Esto cumple con la táctica **ASR de detección**.

---

## ⏱️ Medición de Tiempo de Verificación

En el servidor se mide el tiempo desde que se recibe el JSON hasta que se termina la comparación de firmas.

