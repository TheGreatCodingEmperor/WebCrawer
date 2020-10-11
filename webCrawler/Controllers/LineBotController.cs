using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using webCrawler.Helpers;

namespace webCrawler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LineBotController : ControllerBase
    {

        private readonly IConfiguration _config;
        private int PackageId { get; set; }
        private List<int> StickerList { get; set; }
        private readonly ILogger<LineBotController> _logger;

        public LineBotController(
            ILogger<LineBotController> logger,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _config = configuration;
        }
        [HttpGet()]
        public async Task<IActionResult> config([FromHeader] string configText)
        {
            await Task.CompletedTask;
            return Ok(_config.GetSection(configText).Get<string>());
        }

        [HttpPost]
        public async Task<IActionResult> POST()
        {
            StickerList = Array.ConvertAll(_config.GetSection("Sticker:StickerList").Get<string>().Split(","), int.Parse).ToList();
            PackageId = _config.GetSection("Sticker:PackageId").Get<int>();
            //get configuration from appsettings.json
            var token = _config.GetSection("channelAccessToken");
            var AdminUserId = _config.GetSection("adminUserID");
            var body = ""; //for JSON Body
            //create vot instance
            var bot = new isRock.LineBot.Bot(token.Value);
            isRock.LineBot.MessageBase responseMsg = null;
            //message collection for response multi-message 
            List<isRock.LineBot.MessageBase> responseMsgs =
                new List<isRock.LineBot.MessageBase>();

            try
            {
                //get JSON Body
                using (StreamReader reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8))
                {
                    body = reader.ReadToEndAsync().Result;
                }
                //parsing JSON
                var ReceivedMessage = isRock.LineBot.Utility.Parsing(body);
                //Get LINE Event
                var LineEvent = ReceivedMessage.events.FirstOrDefault();
                //prepare reply message
                if (LineEvent.type.ToLower() == "message")
                {
                    switch (LineEvent.message.type.ToLower())
                    {
                        case "text":
                            Random random = new Random();
                            //add text response
                            responseMsg =
                                new isRock.LineBot.StickerMessage(PackageId, StickerList[random.Next(StickerList.Count - 1)]);
                            responseMsgs.Add(responseMsg);
                            //add ButtonsTemplate if user say "/Show ButtonsTemplate"
                            if (LineEvent.message.text.ToLower().Contains("/show buttonstemplate"))
                            {
                                //define actions
                                var act1 = new isRock.LineBot.MessageAction() { text = "test action1", label = "test action1" };
                                var act2 = new isRock.LineBot.MessageAction() { text = "test action2", label = "test action2" };

                                var tmp = new isRock.LineBot.ButtonsTemplate()
                                {
                                    text = "Button Template text",
                                    title = "Button Template title",
                                    thumbnailImageUrl = new Uri("https://i.imgur.com/wVpGCoP.png"),
                                };

                                tmp.actions.Add(act1);
                                tmp.actions.Add(act2);
                                //add TemplateMessage into responseMsgs
                                responseMsgs.Add(new isRock.LineBot.TemplateMessage(tmp));
                            }
                            else if (LineEvent.message.text.Contains("股票"))
                            {
                                string stockNo = Regex.Split(LineEvent.message.text, "股票.")[1];
                                var webCrawlerHelper = new WebCrawerHelper(_config);
                                string data = webCrawlerHelper.getDatas(stockNo).Result;
                                responseMsg =
                                new isRock.LineBot.TextMessage(data);
                                responseMsgs.Add(responseMsg);
                                var url = _config.GetSection("StockHome").Get<string>();
                                url = $"{url}{stockNo}";
                                responseMsgs.Add( new isRock.LineBot.TextMessage(data));
                            }
                            break;
                        case "sticker":
                            responseMsg =
                                new isRock.LineBot.StickerMessage(1, 2);
                            responseMsgs.Add(responseMsg);
                            break;
                        default:
                            var random2 = new Random();
                            // responseMsg = new isRock.LineBot.TextMessage($"None handled message type : { LineEvent.message.type}");
                            responseMsg = new isRock.LineBot.StickerMessage(PackageId, StickerList[random2.Next(StickerList.Count - 1)]);
                            responseMsgs.Add(responseMsg);
                            break;
                    }
                }
                else
                {
                    responseMsg = new isRock.LineBot.TextMessage($"None handled event type : { LineEvent.type}");
                    responseMsgs.Add(responseMsg);
                }
                await Task.CompletedTask;

                //回覆訊息
                bot.ReplyMessage(LineEvent.replyToken, responseMsgs);
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //如果有錯誤，push給admin
                bot.PushMessage(AdminUserId.Value, "Exception : \n" + ex.Message);
                //response OK
                return Ok();
            }
        }
    }
}