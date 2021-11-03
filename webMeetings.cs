using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using dcinc.api.entities;
using FluentValidation;

namespace dcinc.api
{
    public static class webMeetings
    {
        [FunctionName("webMeetings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "meeting-info-db",
                collectionName: "WebMeetings",
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string message = string.Empty;

            // メソッドにより取得処理と登録処理を切り替える。
            try
            {
                switch (req.Method)
                {
                    case "GET":
                        log.LogInformation("GET webMeetings");
                        break;
                    case "POST":
                        log.LogInformation("POST webMeetings");

                        // リクエストのBODYからパラメータ取得
                        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                        dynamic data = JsonConvert.DeserializeObject(requestBody);
                        
                        WebMeeting webMeeting = new WebMeeting();
                        webMeeting.Name = data?.name;
                        webMeeting.StartDateTime = data?.startDateTime;
                        webMeeting.Url = data?.url;
                        webMeeting.RegisteredAt = data?.registeredAt;
                        webMeeting.SlackChannelId = data?.slackChannelId;

                        WebMeetingValidator validator = new WebMeetingValidator();
                        validator.ValidateAndThrow(webMeeting);

                        // Web会議情報を登録
                        message = await AddWebMeetings(documentsOut, webMeeting);

                        break;
                    default:
                        throw new InvalidOperationException($"Invalid method: method={req.Method}");
                }
            }
            catch (Exception ex)
            {
                return new OkObjectResult(ex.Message);
            }

            return new OkObjectResult($"This HTTP triggered function executed successfully.\n{message}");
        }

        private static async Task<string> AddWebMeetings(
            IAsyncCollector<dynamic> documentsOut,
            WebMeeting webMeeting
            )
        {
            await documentsOut.AddAsync(webMeeting);
            return JsonConvert.SerializeObject(webMeeting);
        }
    }
}
