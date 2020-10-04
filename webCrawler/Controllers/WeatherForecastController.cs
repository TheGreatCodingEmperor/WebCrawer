using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace webCrawler.Controllers {
    [ApiController]
    [Route ("[controller]")]
    public class WeatherForecastController : ControllerBase {
        private static readonly string[] Summaries = new [] {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get;set; }

        public WeatherForecastController (ILogger<WeatherForecastController> logger, Microsoft.Extensions.Configuration.IConfiguration configuration) {
            _logger = logger;
            Configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get () {
            var rng = new Random ();
            return Enumerable.Range (1, 5).Select (index => new WeatherForecast {
                    Date = DateTime.Now.AddDays (index),
                        TemperatureC = rng.Next (-20, 55),
                        Summary = Summaries[rng.Next (Summaries.Length)]
                })
                .ToArray ();
        }

        [HttpGet ("crawler")]
        public async Task<IActionResult> Crawler () {
            HttpClient httpClient = new HttpClient ();

            string[] cols = Configuration.GetSection ("Col").Get<string[]> ();
            string[] chCols = Configuration.GetSection ("ChCol").Get<string[]> ();
            SelectorDto[] selectors = Configuration.GetSection ("Selector").Get<SelectorDto[]> ();
            StringBuilder stringBuilder = new StringBuilder ();
            var colsBox = new List<string>();
            foreach(var chCol in chCols){
                colsBox.Add(chCol);
            }
            stringBuilder.AppendLine(String.Join(",",colsBox));

            var urls = Configuration.GetSection ("Url").Get<string[]> ();
            for (var i=0; i<urls.Count(); i++) {
                string url = urls[i];
                var responseMessage = await httpClient.GetAsync (url); //發送請求

                //檢查回應的伺服器狀態StatusCode是否是200 OK
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK) {
                    string responseResult = responseMessage.Content.ReadAsStringAsync ().Result; //取得內容

                    var config = AngleSharp.Configuration.Default;
                    var context = BrowsingContext.New (config);

                    var document = await context.OpenAsync (res => res.Content (responseResult));

                    List<string> box= new List<string>();
                    foreach(SelectorDto selector in selectors){
                        box.Add( $"{document.QuerySelectorAll (selector.Exp)[selector.Index].InnerHtml}");
                    }
                    stringBuilder.AppendLine(String.Join(",",box));
                    // var closingPrice = float.Parse (document.QuerySelector (".info-lp span").InnerHtml);
                    // var title = document.QuerySelector (".header_second").InnerHtml;
                    // stringBuilder.AppendLine($"{title},{closingPrice}");
                }
            }

            if(stringBuilder != new StringBuilder()){
                await Task.CompletedTask;
                return Ok(File (Encoding.UTF8.GetBytes (stringBuilder.ToString ()), "text/csv", "Reports.csv"));
            }
            else{
                await Task.CompletedTask;
                return BadRequest();
            }
        }

        [HttpGet ("stock")]
        public async Task<IActionResult> Stock () {
            HttpClient httpClient = new HttpClient ();
            var result = new List<Dictionary<string,object>> ();
            
            var urls = Configuration.GetSection ("Url").Get<string[]> ();
            string[] cols = Configuration.GetSection ("Col").Get<string[]> ();
            string[] chCols = Configuration.GetSection ("ChCol").Get<string[]> ();
            SelectorDto[] selectors = Configuration.GetSection ("Selector").Get<SelectorDto[]> ();
            StringBuilder stringBuilder = new StringBuilder ();
            foreach (string url in urls) {
                var responseMessage = await httpClient.GetAsync (url); //發送請求

                //檢查回應的伺服器狀態StatusCode是否是200 OK
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK) {
                    string responseResult = responseMessage.Content.ReadAsStringAsync ().Result; //取得內容

                    var config = AngleSharp.Configuration.Default;
                    var context = BrowsingContext.New (config);

                    var document = await context.OpenAsync (res => res.Content (responseResult));
                    Dictionary<string,object> row = new Dictionary<string, object>();

                    for(var j=0; j<cols.Count();j++){
                        row[cols[j]] = document.QuerySelectorAll (selectors[j].Exp)[selectors[j].Index].InnerHtml;
                    }

                    // var closingPrice = float.Parse (document.QuerySelector (".info-lp span").InnerHtml);
                    // var title = document.QuerySelector (".header_second").InnerHtml;
                    result.Add (row);
                }
            }
            await Task.CompletedTask;

            return Ok (new{ Title = chCols,Result = result});
        }

        [HttpGet("html")]
        public async Task<IActionResult> getHtml([FromQuery] string url){
            HttpClient httpClient = new HttpClient ();
            var responseMessage = await httpClient.GetAsync (url); //發送請求

                //檢查回應的伺服器狀態StatusCode是否是200 OK
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK) {
                    string responseResult = responseMessage.Content.ReadAsStringAsync ().Result; //取得內容

                    var config = AngleSharp.Configuration.Default;
                    var context = BrowsingContext.New (config);

                    var document = await context.OpenAsync (res => res.Content (responseResult));
                    await Task.CompletedTask;
                    return Ok(document.ToHtml());
                }
            await Task.CompletedTask;
            return BadRequest();
        }

        [HttpPut("url")]
        public async Task<IActionResult> ModifyUrls([FromQuery] string[] urls){
            if(urls == null) return BadRequest();
            Configuration["Url"] = JsonConvert.SerializeObject(urls);
            await Task.CompletedTask;
            return Ok(JsonConvert.SerializeObject(urls));
        }
    }
}