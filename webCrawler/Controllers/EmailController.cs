using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;
using Newtonsoft.Json;
using webCrawler.Dto;

namespace webCrawler.Controllers {
    [ApiController]
    [Route ("[controller]")]
    public class EmailController : ControllerBase {

        private readonly ILogger<EmailController> _logger;
        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }

        public EmailController (ILogger<EmailController> logger, Microsoft.Extensions.Configuration.IConfiguration configuration) {
            _logger = logger;
            Configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail ([FromQuery] string address, [FromQuery] string userName, [FromHeader] string password) {

            var _notificationMetadata = new NotificationMetadata ();
            var message = new MimeMessage ();
            message.From.Add(new MailboxAddress ("Self",address));
            message.To.Add(new MailboxAddress ("Self",address));
            message.Subject = "Welcome";
            message.Body = new TextPart(){
                Text = "hello world"
            };
            using (SmtpClient smtpClient = new SmtpClient ()) {
                smtpClient.Connect ("smtp.gmail.com",
                    465, true);
                smtpClient.Authenticate (userName,
                    password);
                try {
                    await smtpClient.SendAsync (message);
                    smtpClient.Disconnect (true);
                    return Ok();
                } catch(Exception e) {
                    return BadRequest(JsonConvert.SerializeObject(e,Formatting.Indented));
                }
            }
        }

        private MimeMessage CreateMimeMessageFromEmailMessage (EmailMessage message) {
            var mimeMessage = new MimeMessage ();
            mimeMessage.From.Add (message.Sender);
            mimeMessage.To.Add (message.Reciever);
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new TextPart (MimeKit.Text.TextFormat.Text) { Text = message.Content };
            return mimeMessage;
        }
    }
}