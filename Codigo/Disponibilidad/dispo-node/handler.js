const { DynamoDBClient, GetItemCommand } = require("@aws-sdk/client-dynamodb");

exports.handler = async (event) => {
  const id = event.pathParameters?.id;
  if (!id) {
    console.log("ID no proporcionado en pathParameters");
    return {
      statusCode: 400,
      body: "ID requerido",
    };
  }

  const client = new DynamoDBClient({});
  try {
    const command = new GetItemCommand({
      TableName: "Productos",
      Key: {
        id: { S: id },
      },
    });

    const result = await client.send(command);

    if (!result.Item) {
      console.log(`Producto con ID ${id} no encontrado`);
      return {
        statusCode: 404,
        body: "Producto no encontrado",
      };
    }

    const nombreAttr = result.Item.nombre?.S;
    const cantidadAttr = result.Item.cantidad?.N;

    if (!nombreAttr || !cantidadAttr) {
      console.log("Campos 'nombre' o 'cantidad' inválidos");
      return {
        statusCode: 500,
        body: "Campos inválidos",
      };
    }

    let cantidad = parseInt(cantidadAttr, 10);
    if (isNaN(cantidad)) {
      console.log("Cantidad inválida");
      return {
        statusCode: 500,
        body: "Cantidad inválida",
      };
    }

    // Simulación: 50% de probabilidad de duplicar cantidad
    if (Math.random() < 0.5) {
      cantidad *= 2;
      console.log(`⚠️ Simulación activada: Cantidad duplicada a ${cantidad}`);
    }

    const producto = {
      id,
      nombre: nombreAttr,
      cantidad,
    };

    return {
      statusCode: 200,
      body: JSON.stringify(producto),
    };
  } catch (err) {
    console.error("Error al consultar producto:", err);
    return {
      statusCode: 500,
      body: "Error interno",
    };
  }
};
