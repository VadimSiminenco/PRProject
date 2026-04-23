using PRProject.Common.DTO;

namespace PRProject.Bll.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskDto>> GetAllAsync();
        Task<TaskDto?> GetByIdAsync(int id);
        Task<TaskDto> CreateAsync(CreateTaskDto dto);
        Task<bool> UpdateAsync(int id, UpdateTaskDto dto);
        Task<bool> DeleteAsync(int id);
    }
}