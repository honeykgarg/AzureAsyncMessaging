using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureAsyncMessaging
{
    public static class AsyncMessageCollector
    {
        [FunctionName("AsyncMessageCollector")]
        //[return: Queue("asyncmessagecollector")] 
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "messages")] HttpRequest req,
            [ServiceBus("messagequeue", Connection = "ConnectionString")] IAsyncCollector<Message> OutMessage,

            ILogger log)
        {
            log.LogInformation("AsyncMessageCollector function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var message = Encoding.UTF8.GetBytes(requestBody);
            Message m = new Message(message);
            string id = Guid.NewGuid().ToString();
            m.UserProperties["RequestId"] = id;
            var url = Environment.GetEnvironmentVariable("API_URL");
            var uri = url ?? url + "/status/" + id;
            m.UserProperties["RequestSubmittedAt"] = DateTime.UtcNow;

            await OutMessage.AddAsync(m);

            return (ActionResult)new AcceptedResult(uri, $"Request Accepted and result can be accesed using { url + "/status/" + id}");
        }
    }
}
