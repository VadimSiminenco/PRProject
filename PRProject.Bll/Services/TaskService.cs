using Microsoft.EntityFrameworkCore;
using PRProject.Bll.Interfaces;
using PRProject.Common.DTO;
using PRProject.Dal.Data;
using PRProject.Domain.Entities;

namespace PRProject.Bll.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly ITaskRealtimeNotifier _taskRealtimeNotifier;
        public TaskService(AppDbContext context, ITaskRealtimeNotifier taskRealtimeNotifier)
        {
            _context = context;
            _taskRealtimeNotifier = taskRealtimeNotifier;
        }
        public async Task<List<TaskDto>> GetAllAsync()
        {
            var tasks = await _context.Tasks
                .OrderByDescending(t => t.Id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return tasks;
        }

        public async Task<TaskDto?> GetByIdAsync(int id)
        {
            var task = await _context.Tasks
                .Where(t => t.Id == id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt
                })
                .FirstOrDefaultAsync();

            return task;
        }

        public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            await _taskRealtimeNotifier.NotifyTasksChangedAsync();

            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateTaskDto dto)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return false;
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsCompleted = dto.IsCompleted;

            await _context.SaveChangesAsync();
            await _taskRealtimeNotifier.NotifyTasksChangedAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            await _taskRealtimeNotifier.NotifyTasksChangedAsync();
            return true;
        }
    }
}