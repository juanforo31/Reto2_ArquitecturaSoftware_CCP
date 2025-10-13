class Program1
{
    static async Task Main(string[] args)
    {
        var config = new AppConfig
        {
            Url = "https://jsonplaceholder.typicode.com/posts/1",
            DurationSeconds = 30,
            IntervalSeconds = 5
        };

        var fetcher = new DataFetcher();
        var exporter = new ExcelExporter();

        var data = await fetcher.FetchDataAsync(config);
        exporter.SaveToExcel(data, "output.xlsx");
    }
}