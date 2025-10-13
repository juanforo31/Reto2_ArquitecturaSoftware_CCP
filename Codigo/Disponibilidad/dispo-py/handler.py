import json
import random
import boto3
import logging

dynamodb = boto3.client('dynamodb')
logger = logging.getLogger()
logger.setLevel(logging.INFO)

def lambda_handler(event, context):
    path_params = event.get('pathParameters') or {}
    producto_id = path_params.get('id')

    if not producto_id:
        logger.warning("ID no proporcionado en pathParameters")
        return {
            "statusCode": 400,
            "body": "ID requerido"
        }

    try:
        response = dynamodb.get_item(
            TableName='Productos',
            Key={'id': {'S': producto_id}}
        )
    except Exception as e:
        logger.error(f"Error en GetItem: {str(e)}")
        return {
            "statusCode": 500,
            "body": "Error al consultar producto"
        }

    item = response.get('Item')
    if not item:
        logger.info(f"Producto con ID {producto_id} no encontrado")
        return {
            "statusCode": 404,
            "body": "Producto no encontrado"
        }

    try:
        nombre = item['nombre']['S']
        cantidad = int(item['cantidad']['N'])
    except (KeyError, ValueError) as e:
        logger.error(f"Error extrayendo campos: {str(e)}")
        return {
            "statusCode": 500,
            "body": "Campos inválidos en producto"
        }

    # Simulación: 50% de probabilidad de duplicar cantidad
    if random.random() < 0.5:
        cantidad *= 2
        logger.info(f"⚠️ Simulación activada: Cantidad duplicada a {cantidad}")

    producto = {
        "id": producto_id,
        "nombre": nombre,
        "cantidad": cantidad
    }

    return {
        "statusCode": 200,
        "body": json.dumps(producto),
        "headers": {
            "Content-Type": "application/json"
        }
    }
