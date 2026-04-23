using Microsoft.AspNetCore.SignalR;
using PRProject.Api.Hubs;
using PRProject.Bll.Interfaces;

namespace PRProject.Api.Services
{
    public class SignalRTaskRealtimeNotifier : ITaskRealtimeNotifier
    {
        private readonly IHubContext<TaskHub> _hubContext;

        public SignalRTaskRealtimeNotifier(IHubContext<TaskHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyTasksChangedAsync()
        {
            await _hubContext.Clients.All.SendAsync("TasksChanged");
        }
    }
}