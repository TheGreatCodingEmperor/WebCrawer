using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using line

namespace webCrawler.Controllers {
    [ApiController]
    [Route ("[controller]")]
    public class LineBotController : ControllerBase {

        private readonly ILogger<LineBotController> _logger;
        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }

        public LineBotController (ILogger<LineBotController> logger, Microsoft.Extensions.Configuration.IConfiguration configuration) {
            _logger = logger;
            Configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage () {
            isRock.LineBot LineBotHelper = new LineBot.LineBotHelper (
                "你的Channel ID", "你的Channel Secret", "你的MID");

            //Get  Post RawData
            string postData = Request.Content.ReadAsStringAsync ().Result;

            //取得LineBot接收到的訊息
            var ReceivedMessage = LineBotHelper.GetReceivedMessage (postData);

            //發送訊息
            var ret = LineBotHelper.SendMessage (
                new List<string> () { ReceivedMessage.result[0].content.from },
                "你剛才說了 " + ReceivedMessage.result[0].content.text);
        }

    }
}