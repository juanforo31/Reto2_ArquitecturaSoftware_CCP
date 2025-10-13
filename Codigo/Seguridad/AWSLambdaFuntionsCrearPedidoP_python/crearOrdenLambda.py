import json
import hmac
import hashlib
import os
import time
import uuid

def lambda_handler(event, context):
    timings = {}
    """
    Lambda para crear pedidos con validación HMAC.
    """
    try:
        t0 = time.perf_counter()
        # ✅ 1. Obtener cabecera X-Signature
        headers = event.get("headers") or {}
        signature_client = headers.get("X-Signature") or headers.get("x-signature")
        if not signature_client:
            t1 = time.perf_counter()
            timings['falta_firma'] = (t1 - t0) * 1000  # en ms
            return {
                "statusCode": 400,
                "body": json.dumps({"error": "Falta cabecera X-Signature", "estado": 1, "tiempo": timings['falta_firma']})
            }

        # ✅ 2. Leer el body
        body = event.get("body")
        if not body:
            t2 = time.perf_counter()
            timings['body_vacio'] = (t2 - t0) * 1000  # en ms
            return {
                "statusCode": 400,
                "body": json.dumps({"error": "Body vacío", "estado": 1, "tiempo": timings['body_vacio']})
            }

        # Si API Gateway envía el body como string, lo convertimos
        if isinstance(body, str):
            try:
                payload = json.loads(body)
            except json.JSONDecodeError:
                return {
                    "statusCode": 400,
                    "body": json.dumps({"mensaje": "JSON inválido"})
                }
        else:
            payload = body

        t3 = time.perf_counter()
        # ✅ 3. Generar la firma HMAC en el servidor
        secret_key = os.environ.get("HMAC_KEY", "Arquitectura de software")
        json_payload = json.dumps(payload, separators=(',', ':'))  # compactar JSON
        print('Prueba ' + json_payload)
        # json_payload =  json
        signature_server = hmac.new(
            secret_key.encode('utf-8'),
            json_payload.encode('utf-8'),
            hashlib.sha256
        ).hexdigest()

        # ✅ 4. Comparar firmas (tiempo constante)
        if not hmac.compare_digest(signature_server, signature_client):
            t4 = time.perf_counter()
            timings['error_firmas'] = (t4 - t3) * 1000  # en ms
            print("⚠️ Firma inválida detectada")
            return {
                "statusCode": 401,
                "body": json.dumps({"error": "Firma inválida. Mensaje modificado.", "estado": 2, "tiempo": timings['error_firmas'] })
            }

        t5 = time.perf_counter()
        timings['exito_firmas'] = (t5 - t3) * 1000  # en ms
        # ✅ 5. TODO: aquí podrías insertar la orden en tu base de datos
        print(f"✅ Orden recibida: {payload}")
        

        return {
            "statusCode": 200,
            "headers": {
                "Content-Type": "application/json"
            },
            "body": json.dumps({"mensaje": "Orden creada correctamente", "pedidoId": str(uuid.uuid4()), "estado": 3, "tiempo": timings['exito_firmas']})
        }

    except Exception as e:
        print(f"❌ Error interno: {e}")
        return {
            "statusCode": 500,
            "body": json.dumps({"error": "Error interno del servidor"})
        }

