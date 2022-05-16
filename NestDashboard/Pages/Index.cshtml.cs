using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.SmartDeviceManagement.v1;
using Google.Apis.SmartDeviceManagement.v1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NestDashboard.Pages
{
    [GoogleScopedAuthorize("https://www.googleapis.com/auth/pubsub", SmartDeviceManagementService.ScopeConstants.SdmService)]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IGoogleAuthProvider _googleAuthProvider;
        private readonly string _projectId = Environment.GetEnvironmentVariable("PROJECTID");

        public IList<GoogleHomeEnterpriseSdmV1Device>? Devices { get; set; }

        public IndexModel(ILogger<IndexModel> logger, [FromServices] IGoogleAuthProvider auth)
        {
            _logger = logger;
            _googleAuthProvider = auth;
        }

        
        public void OnGet()
        {
            GoogleCredential cred = _googleAuthProvider.GetCredentialAsync().Result;
            var service = new SmartDeviceManagementService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });

            var devices = service.Enterprises.Devices.List($"enterprises/{_projectId}").ExecuteAsync().Result;
            Devices = devices.Devices;
        }
    }
}