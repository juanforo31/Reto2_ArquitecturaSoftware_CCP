using System;
using System.Text;
using System.Threading;

class Program
{
    static async Task Main(string[] args)
    {
        await ejecutarConsultaBase();
    }

    static async Task ejecutarConsultaBase()
    {
        var inicio = DateTime.Now;
        var inicioTest = inicio.ToString("yyyyMMdd_HHmmss");
        Console.WriteLine($"Inicio {inicioTest}");

        // python
        var url = "https://r8xfdjo1af.execute-api.us-east-1.amazonaws.com/default/crearOrdenP";
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

    static async Task ejecutarConsultaNumeroUsuarios()
    {
        var inicio = DateTime.Now;
        int numberOfUsers = 10; // n usuarios
        int durationMinutes = 2; // x minutos
        int intervalMilliseconds = 5000; // cada usuario envía una petición cada 5 segundos

        var inicioTest = inicio.ToString("yyyyMMdd_HHmmss");
        Console.WriteLine($"Inicio {inicioTest}");


        // python
        var url = "https://r8xfdjo1af.execute-api.us-east-1.amazonaws.com/default/crearOrdenP";
        var duracion = TimeSpan.FromSeconds(30);
        var intervaloMin = 500;
        var intervaloMax = 1500;
        var rutaExcel = $"resultado_{inicioTest}.xlsx";

        var peticionService = new PeticionService();
        var excelService = new ExcelService(rutaExcel);

        Task[] userTasks = new Task[numberOfUsers];
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(durationMinutes));

        int contador = 0;
        for (int i = 0; i < numberOfUsers; i++)
        {
            int userId = i + 1;
            userTasks[i] = Task.Run(async () =>
            {
                contador++;
                while (!cts.Token.IsCancellationRequested)
                {
                    var resultado = await peticionService.EjecutarAsync(url, contador);
                    excelService.AgregarFila(resultado);
                    await Task.Delay(intervalMilliseconds, cts.Token);
                }
            }, cts.Token);
        }

        await Task.WhenAll(userTasks);
        excelService.GuardarResumen(contador, DateTime.Now - inicio);
        Console.WriteLine("Simulación completada.");
    }
}