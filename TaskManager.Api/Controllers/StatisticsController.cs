using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;
        public StatisticsController(IUserService userService, ITaskService taskService)
        {
            _userService = userService;
            _taskService = taskService;
        }

        [HttpGet("users/count")]
        public async Task<ActionResult<int>> GetUserCount()
        {
            var count = await _userService.GetTotalNumberOfUsers();
            return Ok(count);
        }

        [HttpGet("tasks/count")]
        public async Task<ActionResult<ChartDataDto>> GetTaskCount(string groupBy = "status")
        {
            if (groupBy != "status" && groupBy != "priority")
            {
                return BadRequest("Invalid groupBy parameter. Use 'status' or 'priority'.");
            }

            ChartDataDto chartData;

            if (groupBy == "status")
            {
                var countByStatus = await _taskService.GetNumberOfTasksByStatus();

                chartData = new ChartDataDto
                {
                    ChartType = "pie",
                    Title = $"Tasks by {char.ToUpper(groupBy[0]) + groupBy[1..]}",
                    Labels = countByStatus.TaskCounts.Select(tc => tc.StatusName).ToList(),
                    Series = new List<DataSeriesDto>
                    {
                        new DataSeriesDto
                        {
                            Name = "Tasks",
                            Data = countByStatus.TaskCounts.Select(tc => (double)tc.TaskCount).ToList(),
                            Color = null
                        }
                    }
                };

                return Ok(chartData);
            }

            var countByPriority = await _taskService.GetNumberOfTasksByPriority();

            chartData = new ChartDataDto
            {
                ChartType = "bar",
                Title = $"Tasks by {char.ToUpper(groupBy[0]) + groupBy[1..]}",
                Labels = countByPriority.TaskCounts.Select(tc => tc.Priority).ToList(),
                Series = new List<DataSeriesDto>
                {
                    new DataSeriesDto
                    {
                        Name = "Tasks",
                        Data = countByPriority.TaskCounts.Select(tc => (double)tc.TaskCount).ToList(),
                        Color = null
                    }
                }
            };

            return Ok(chartData);
        }
    }
}
