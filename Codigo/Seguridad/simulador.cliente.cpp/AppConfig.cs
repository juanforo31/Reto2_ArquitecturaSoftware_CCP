public class AppConfig
{
    public string Url { get; set; } = "https://l8etwwgmo1.execute-api.us-east-1.amazonaws.com/default/crearOrden";
    public int DurationSeconds { get; set; } = 60; // tiempo total
    public int IntervalSeconds { get; set; } = 5;  // cada cuánto hacer la petición
}