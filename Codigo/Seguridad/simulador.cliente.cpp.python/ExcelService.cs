using ClosedXML.Excel;

public class ExcelService
{
    private readonly string _ruta;
    private readonly XLWorkbook _workbook;
    private readonly IXLWorksheet _datos;
    private readonly IXLWorksheet _resumen;
    private int _filaActual = 2;
    private readonly object _lock = new object();

    public ExcelService(string ruta)
    {
        _ruta = ruta;
        _workbook = File.Exists(ruta) ? new XLWorkbook(ruta) : new XLWorkbook();
        _datos = _workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Datos") ?? _workbook.AddWorksheet("Datos");
        _resumen = _workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Resumen") ?? _workbook.AddWorksheet("Resumen");

        if (_filaActual == 2 && _datos.Cell(1, 1).IsEmpty())
        {
            _datos.Cell(1, 1).Value = "Numero";
            _datos.Cell(1, 2).Value = "Status";
            _datos.Cell(1, 3).Value = "Fecha de inicio";
            _datos.Cell(1, 4).Value = "Tiempo de respuesta";
            _datos.Cell(1, 5).Value = "PedidoId";
            _datos.Cell(1, 6).Value = "Estado";
            _datos.Cell(1, 7).Value = "Tiempo";
            _datos.Cell(1, 8).Value = "Error";
            _datos.Cell(1, 9).Value = "Manipulado";
        }
    }

    public void AgregarFila(PeticionResultado r)
    {
        lock (_lock)
        {
            _datos.Cell(_filaActual, 1).Value = r.Numero;
            _datos.Cell(_filaActual, 2).Value = r.Status;
            _datos.Cell(_filaActual, 3).Value = r.FechaInicio.ToString("yyyy-MM-dd HH:mm:ss");
            _datos.Cell(_filaActual, 4).Value = r.TiempoRespuestaMs;
            _datos.Cell(_filaActual, 5).Value = r.PedidoId?.ToString() ?? "";
            _datos.Cell(_filaActual, 6).Value = r.Estado ?? 0;
            _datos.Cell(_filaActual, 7).Value = r.Tiempo ?? 0;
            _datos.Cell(_filaActual, 8).Value = r.Error ?? "";
            _datos.Cell(_filaActual, 9).Value = r.Manipulado ?? "";
            _filaActual++;
            _workbook.SaveAs(_ruta);
        }
    }

    public void GuardarResumen(int total, TimeSpan duracion)
    {
        lock (_lock)
        {
            _resumen.Cell(1, 1).Value = "Total de peticiones";
            _resumen.Cell(1, 2).Value = total;
            _resumen.Cell(2, 1).Value = "Duración total (s)";
            _resumen.Cell(2, 2).Value = duracion.TotalSeconds;
            _workbook.SaveAs(_ruta);
        }
    }
}