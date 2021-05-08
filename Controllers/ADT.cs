
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public ActionResult Upload(string instanceUrl, string tenantId, string clientId)
        {
            ADTModel adtModel = new ADTModel
            {
                InstanceUrl = instanceUrl,
                TenantId = tenantId,
                ClientId = clientId
            };

            try
            {
                // authenticate with ADT service
                DigitalTwinsClient client = new DigitalTwinsClient(new Uri(instanceUrl), new InteractiveBrowserCredential(tenantId, clientId));

                // upload
                Azure.Response<DigitalTwinsModelData[]> response = client.CreateModelsAsync(new[] { DTDL.GeneratedDTDL }).GetAwaiter().GetResult();
                if (response.GetRawResponse().Status == 200)
                {
                    return View("Index", adtModel);
                }
                else
                {
                    adtModel.ErrorMessage = response.GetRawResponse().ReasonPhrase;
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
