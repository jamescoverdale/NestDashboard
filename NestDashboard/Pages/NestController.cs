using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.SmartDeviceManagement.v1;
using Microsoft.AspNetCore.Mvc;

namespace NestDashboard.Pages
{
    [Route("[controller]")]
    public class NestController : Controller
    {
        private readonly string _projectId = Environment.GetEnvironmentVariable("PROJECTID");      
       
        [GoogleScopedAuthorize(SmartDeviceManagementService.ScopeConstants.SdmService)]
        public async Task<IActionResult> Index([FromServices] IGoogleAuthProvider auth)
        {
            GoogleCredential cred = await auth.GetCredentialAsync();
            var service = new SmartDeviceManagementService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });
            
            var devices = await service.Enterprises.Devices.List($"enterprises/{_projectId}").ExecuteAsync();            
           
            return View(devices.Devices);
        }
        [Route("GetThermostatDetails")]
        [GoogleScopedAuthorize(SmartDeviceManagementService.ScopeConstants.SdmService)]
        public async Task<IActionResult> GetThermostatDetails([FromServices] IGoogleAuthProvider auth, string id)
        {
            GoogleCredential cred = await auth.GetCredentialAsync();
            var service = new SmartDeviceManagementService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });

            var thermostat = await service.Enterprises.Devices.Get(id).ExecuteAsync();

            return View(thermostat);
        }

        [Route("Watch")]
        [GoogleScopedAuthorize(SmartDeviceManagementService.ScopeConstants.SdmService)]
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
        [GoogleScopedAuthorize(SmartDeviceManagementService.ScopeConstants.SdmService)]
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
        [GoogleScopedAuthorize(SmartDeviceManagementService.ScopeConstants.SdmService)]
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
