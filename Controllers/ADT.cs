using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UANodesetWebViewer.Models;

namespace UANodesetWebViewer.Controllers
{
    public class ADT : Controller
    {
        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        public ActionResult Upload(string instanceUrl, string tenantId, string clientId, string secret)
        {
            ADTModel adtModel = new ADTModel
            {
                InstanceUrl = instanceUrl,
                TenantId = tenantId,
                ClientId = clientId,
                Secret = secret
            };

            try
            {
                string authority = "https://login.microsoftonline.com";
                string resource = "0b07f429-9f4b-4714-9392-cc5e8e80c8b0";

                ClientCredential credentials = new ClientCredential(clientId, secret);
                AuthenticationContext authContext = new AuthenticationContext($"{authority}/{tenantId}");
                AuthenticationResult result = authContext.AcquireTokenAsync(resource, credentials).GetAwaiter().GetResult();

                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                byte[] buffer = Encoding.UTF8.GetBytes(DTDL.GeneratedDTDL);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = httpClient.PostAsync($"{instanceUrl}/models?api-version=2020-10-31", byteContent).GetAwaiter().GetResult();
                httpClient.Dispose();

                if (response.IsSuccessStatusCode)
                {
                    return View("Index", adtModel);
                }
                else
                {
                    adtModel.ErrorMessage = response.StatusCode.ToString();
                    return View("Error", adtModel);
                }
            }
            catch (Exception ex)
            {
                adtModel.ErrorMessage = ex.Message;
                return View("Error", adtModel);
            }
        }
    }
}
