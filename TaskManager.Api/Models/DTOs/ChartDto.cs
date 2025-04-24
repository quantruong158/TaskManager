namespace TaskManager.Api.Models.DTOs
{
    public class ChartDataDto
    {
        public required string ChartType { get; set; } // "pie", "bar", "line", etc.
        public required string Title { get; set; }
        public List<string> Labels { get; set; } = [];
        public List<DataSeriesDto> Series { get; set; } = [];
    }

    public class DataSeriesDto
    {
        public required string Name { get; set; }
        public List<double> Data { get; set; } = [];
        public string? Color { get; set; }
    }
}
