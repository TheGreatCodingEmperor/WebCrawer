using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.Configuration;
using webCrawler.Dto;
using Newtonsoft.Json;

namespace webCrawler.Helpers {
    public class WebCrawerHelper {
        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }
        public WebCrawerHelper (Microsoft.Extensions.Configuration.IConfiguration configuration) {
            Configuration = configuration;
        }
        public async Task<string> getDatas (string stockNo) {
            HttpClient httpClient = new HttpClient ();

            var url = Configuration.GetSection ("StockHome").Get<string> ();
            url = $"{url}{stockNo}";
            string[] cols = Configuration.GetSection ("Col").Get<string> ().Split(",");
            string[] chCols = Configuration.GetSection ("ChCol").Get<string> ().Split(",");
            SelectorDto[] selectors = JsonConvert.DeserializeObject<SelectorDto[]>(Configuration.GetSection ("Selector").Get<string> ());
            StringBuilder stringBuilder = new StringBuilder ();
            var responseMessage = await httpClient.GetAsync (url); //發送請求

            //檢查回應的伺服器狀態StatusCode是否是200 OK
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK) {
                string responseResult = responseMessage.Content.ReadAsStringAsync ().Result; //取得內容

                var config = AngleSharp.Configuration.Default;
                var context = BrowsingContext.New (config);

                var document = await context.OpenAsync (res => res.Content (responseResult));
                Dictionary<string, object> row = new Dictionary<string, object> ();

                for (var j = 0; j < cols.Count (); j++) {
                    row[chCols[j]] = document.QuerySelectorAll (selectors[j].Exp) [selectors[j].Index].InnerHtml;
                }

                // var closingPrice = float.Parse (document.QuerySelector (".info-lp span").InnerHtml);
                // var title = document.QuerySelector (".header_second").InnerHtml;
                return JsonConvert.SerializeObject(row,Formatting.Indented);
            }

            return "";
        }

    }
}