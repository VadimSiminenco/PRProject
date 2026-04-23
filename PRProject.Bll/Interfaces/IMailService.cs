namespace PRProject.Bll.Interfaces
{
    public interface IMailService
    {
        Task<bool> SendTaskByEmailAsync(int taskId);
        Task<List<string>> GetMessagesByImapAsync();
        Task<List<string>> GetMessagesByPop3Async();
    }
}
