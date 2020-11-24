using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace AzureAsyncMessaging
{
    public static class AsyncMessageProcessor
    {
        [FunctionName("AsyncMessageProcessor")]
        public async static Task Run(
            [ServiceBusTrigger("messagequeue", Connection = "ConnectionString")] Message myQueueItem,
            [Blob("messagestorage", FileAccess.Write, Connection = "StorageConnectionString")] CloudBlobContainer container,
            ILogger log
        )
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            var messageId = myQueueItem.UserProperties["RequestId"] as string;

            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference($"{messageId}.json");
            await cloudBlockBlob.UploadFromByteArrayAsync(myQueueItem.Body, 0, myQueueItem.Body.Length);
        }
    }
}
