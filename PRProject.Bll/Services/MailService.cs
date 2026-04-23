using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using PRProject.Bll.Interfaces;
using PRProject.Common.Configuration;
using PRProject.Dal.Data;

namespace PRProject.Bll.Services
{
    public class MailService : IMailService
    {
        private readonly AppDbContext _context;
        private readonly MailSettings _mailSettings;

        public MailService(AppDbContext context, IOptions<MailSettings> mailOptions)
        {
            _context = context;
            _mailSettings = mailOptions.Value;
        }

        public async Task<bool> SendTaskByEmailAsync(int taskId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ToDo App", _mailSettings.FromEmail));
            message.To.Add(new MailboxAddress("User", _mailSettings.ToEmail));
            message.Subject = $"Задача #{task.Id}: {task.Title}";

            message.Body = new TextPart("plain")
            {
                Text =
$@"ID: {task.Id}
Название: {task.Title}
Описание: {task.Description}
Статус: {(task.IsCompleted ? "Выполнено" : "Не выполнено")}
Создано: {task.CreatedAt}"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.None);
            await client.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }

        public async Task<List<string>> GetMessagesByImapAsync()
        {
            var result = new List<string>();

            using var client = new ImapClient();
            await client.ConnectAsync(_mailSettings.ImapHost, _mailSettings.ImapPort, SecureSocketOptions.None);
            await client.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

            for (int i = 0; i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i);
                result.Add($"Subject: {message.Subject}");
            }

            await client.DisconnectAsync(true);
            return result;
        }

        public async Task<List<string>> GetMessagesByPop3Async()
        {
            var result = new List<string>();

            using var client = new Pop3Client();
            await client.ConnectAsync(_mailSettings.Pop3Host, _mailSettings.Pop3Port, SecureSocketOptions.None);
            await client.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);

            for (int i = 0; i < client.Count; i++)
            {
                var message = await client.GetMessageAsync(i);
                result.Add($"Subject: {message.Subject}");
            }

            await client.DisconnectAsync(true);
            return result;
        }
    }
}