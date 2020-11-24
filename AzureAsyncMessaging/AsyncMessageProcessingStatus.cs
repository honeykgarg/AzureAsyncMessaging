using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureAsyncMessaging
{
    public static class AsyncMessageProcessingStatus
    {
        [FunctionName("AsyncMessageProcessingStatus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status/{id}")] HttpRequest req,
            [Blob("messagestorage/{id}.json", FileAccess.Read, Connection = "StorageConnectionString")] CloudBlockBlob cloudBlockBlob,
                string id,
                ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            if (await cloudBlockBlob.ExistsAsync())
            {
                SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
                policy.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-15);
                policy.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(+10);
                policy.Permissions = SharedAccessBlobPermissions.Read;
                string token = cloudBlockBlob.GetSharedAccessSignature(policy);
                //return new RedirectResult(cloudBlockBlob + token, true);
                return new AcceptedResult(cloudBlockBlob.Uri + token, $"Request has been processed. Download file using : {cloudBlockBlob.Uri + token}");
            }
            var url = Environment.GetEnvironmentVariable("API_URL");
            var uri = url ?? url + "/status/" + id;
            return new AcceptedResult { 
                Location = uri,
                Value = $"Request not yet processed. Try after some time with this url : {uri}"
            };
            //return new OkObjectResult();
        }
    }
}
