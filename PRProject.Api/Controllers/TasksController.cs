using Microsoft.AspNetCore.Mvc;
using PRProject.Bll.Interfaces;
using PRProject.Common.DTO;

namespace PRProject.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskDto>>> GetAll()
        {
            var tasks = await _taskService.GetAllAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetById(int id)
        {
            var task = await _taskService.GetByIdAsync(id);

            if (task == null)
            {
                return NotFound(new { message = "Задача не найдена" });
            }

            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
        {
            var createdTask = await _taskService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTaskDto dto)
        {
            var updated = await _taskService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new { message = "Задача не найдена" });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _taskService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = "Задача не найдена" });
            }

            return NoContent();
        }
    }
}