using Microsoft.AspNetCore.Mvc;
using PRProject.Bll.Interfaces;

namespace PRProject.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;

        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }

        [HttpPost("send-task/{id}")]
        public async Task<IActionResult> SendTaskByEmail(int id)
        {
            var sent = await _mailService.SendTaskByEmailAsync(id);

            if (!sent)
            {
                return NotFound(new { message = "Задача не найдена" });
            }

            return Ok(new { message = "Письмо успешно отправлено" });
        }

        [HttpGet("imap/messages")]
        public async Task<IActionResult> GetImapMessages()
        {
            var messages = await _mailService.GetMessagesByImapAsync();
            return Ok(messages);
        }

        [HttpGet("pop3/messages")]
        public async Task<IActionResult> GetPop3Messages()
        {
            var messages = await _mailService.GetMessagesByPop3Async();
            return Ok(messages);
        }
    }
}