using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.SmartDeviceManagement.v1;
using Google.Apis.Util;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Auth;
using Microsoft.AspNetCore.Mvc;

namespace NestDashboard.Pages
{
    [Route("[controller]")]    
    public class NestController : Controller
    {
        private readonly string _projectId = Environment.GetEnvironmentVariable("PROJECTID");
        private readonly string _eventProjectId = Environment.GetEnvironmentVariable("EVENTPROJECTID");

        //[GoogleScopedAuthorize(SmartDeviceManagementService.ScopeConstants.SdmService)]
        //public async Task<IActionResult> Index([FromServices] IGoogleAuthProvider auth)
        //{
        //    GoogleCredential cred = await auth.GetCredentialAsync();
        //    var service = new SmartDeviceManagementService(new BaseClientService.Initializer
        //    {
        //        HttpClientInitializer = cred
        //    });

        //    var devices = await service.Enterprises.Devices.List($"enterprises/{_projectId}").ExecuteAsync();            

        //    return View(devices.Devices);
        //}       

        [Route("Watch")]        
        public async Task<IActionResult> Watch([FromServices] IGoogleAuthProvider auth, string id)
        {
            GoogleCredential cred = await auth.GetCredentialAsync();
            var service = new SmartDeviceManagementService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });

            var device = await service.Enterprises.Devices.Get(id).ExecuteAsync();            

            return View("Watch", device);
        }

        [Route("GenerateWebRtc")]
        [HttpPost]        
        public async Task<IActionResult> GenerateWebRtc([FromServices] IGoogleAuthProvider auth, [FromBody] SdpRequest req)
        {
            GoogleCredential cred = await auth.GetCredentialAsync();
            var service = new SmartDeviceManagementService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });

            var devices = await service.Enterprises.Devices.ExecuteCommand(new Google.Apis.SmartDeviceManagement.v1.Data.GoogleHomeEnterpriseSdmV1ExecuteDeviceCommandRequest()
            {
                Command = "sdm.devices.commands.CameraLiveStream.GenerateWebRtcStream",
                Params__ = new Dictionary<string, object>()
                {
                    {"offer_sdp", req.offerSdp }
                }
            }, req.id).ExecuteAsync();

            return Json(devices.Results);
        }

        [Route("StopWebRtc")]
        [HttpPost]        
        public async Task<IActionResult> StopWebRtc([FromServices] IGoogleAuthProvider auth, [FromBody] StopStream req)
        {
            GoogleCredential cred = await auth.GetCredentialAsync();
            var service = new SmartDeviceManagementService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });

            var devices = await service.Enterprises.Devices.ExecuteCommand(new Google.Apis.SmartDeviceManagement.v1.Data.GoogleHomeEnterpriseSdmV1ExecuteDeviceCommandRequest()
            {
                Command = "sdm.devices.commands.CameraLiveStream.StopWebRtcStream",
                Params__ = new Dictionary<string, object>()
                {
                    {"mediaSessionId", req.mediaId }
                }
            }, req.id).ExecuteAsync();

            return Json(devices.Results);
        }

        [Route("GetEvents")]       
        public async Task<IActionResult> GetEvents([FromServices] IGoogleAuthProvider auth, string subscriptionId, bool acknowledge)      
        {
            GoogleCredential cred = await auth.GetCredentialAsync();           
            var settings = new SubscriberClient.ClientCreationSettings(credentials: cred.ToChannelCredentials());

            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(_eventProjectId, subscriptionId);

            ProjectName projectName = ProjectName.FromProject(_projectId);
            SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName, settings);

            string text = "[";      

            // SubscriberClient runs your message handle function on multiple
            // threads to maximize throughput.
            int messageCount = 0;
            Task startTask = subscriber.StartAsync((PubsubMessage message, CancellationToken cancel) =>
            { 
                text += System.Text.Encoding.UTF8.GetString(message.Data.ToArray()) + ",\r\n";
                Console.WriteLine($"Message {message.MessageId}: {text}");
                Interlocked.Increment(ref messageCount);
                return Task.FromResult(acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
            });
            // Run for 5 seconds.
            await Task.Delay(5000);
            await subscriber.StopAsync(CancellationToken.None);
            // Lets make sure that the start task finished successfully after the call to stop.
            await startTask;
            return Content(text + "]");
        }


    }

    public class SdpRequest
    {
        public string id { get; set; }
        public string offerSdp { get; set; }
    }

    public class StopStream
    {
        public string id { get; set; }
        public string mediaId { get; set; }
    }
}
