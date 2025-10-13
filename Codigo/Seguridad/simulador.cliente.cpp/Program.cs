class Program
{
    static async Task Main(string[] args)
    {
        var inicio = DateTime.Now;
        var inicioTest = inicio.ToString("yyyyMMdd_HHmmss");
        Console.WriteLine($"Inicio {inicioTest}");

        var url = "https://l8etwwgmo1.execute-api.us-east-1.amazonaws.com/default/crearOrden";
        var duracion = TimeSpan.FromSeconds(30);
        var intervaloMin = 500;
        var intervaloMax = 1500;
        var rutaExcel = $"resultado_{inicioTest}.xlsx";

        var peticionService = new PeticionService();
        var excelService = new ExcelService(rutaExcel);

        var tareas = new List<Task>();

        int contador = 0;
        var random = new Random();

        while (DateTime.Now - inicio < duracion)
        {
            contador++;
            var tarea = Task.Run(async () =>
            {
                var resultado = await peticionService.EjecutarAsync(url, contador);
                excelService.AgregarFila(resultado);
            });

            tareas.Add(tarea);
            await Task.Delay(random.Next(intervaloMin, intervaloMax));
        }

        // un registro
        //contador++;
        //var tarea = Task.Run(async () =>
        //{
        //    var resultado = await peticionService.EjecutarAsync(url, contador);
        //    excelService.AgregarFila(resultado);
        //});

        //tareas.Add(tarea);
        //await Task.Delay(random.Next(intervaloMin, intervaloMax));

        await Task.WhenAll(tareas);
        excelService.GuardarResumen(contador, DateTime.Now - inicio);
    }
}