using ClosedXML.Excel;

public class ExcelExporter
{
    public void SaveToExcel(List<string> data, string filePath)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Responses");

        worksheet.Cell(1, 1).Value = "Timestamp";
        worksheet.Cell(1, 2).Value = "Response";

        for (int i = 0; i < data.Count; i++)
        {
            worksheet.Cell(i + 2, 1).Value = DateTime.Now.AddSeconds(i * 5).ToString("HH:mm:ss");
            worksheet.Cell(i + 2, 2).Value = data[i];
        }

        workbook.SaveAs(filePath);
        Console.WriteLine($"Excel guardado en: {filePath}");
    }
}