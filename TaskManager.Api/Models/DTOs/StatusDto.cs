namespace TaskManager.Api.Models.DTOs
{
    public class StatusDto
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
    public class CreateStatusRequestDto
    {
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateStatusRequestDto
    {
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }

}