using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using UANodesetWebViewer.Models;

namespace UANodesetWebViewer.Controllers
{
    public class UACL : Controller
    {
        public ActionResult Index()
        {
            UACLModel uaclModel = new UACLModel
            {
                StatusMessage = ""
            };

            return View("Index", uaclModel);
        }

        [HttpPost]
        public ActionResult Upload(string instanceUrl, string tenantId, string clientId, string secret)
        {
            UACLModel uaclModel = new UACLModel
            {
                InstanceUrl = instanceUrl,
                ClientId = clientId,
                Secret = secret
            };

            try
            {
                if (BrowserController._nodeSetFilename.Count < 1)
                {
                    throw new Exception("No nodeset file is currently loaded!");
                }

                // call the UA cloud lib REST endpoint for info model upload
                WebClient webClient = new WebClient
                {
                    BaseAddress = instanceUrl
                };

                AddressSpace uaAddressSpace = new AddressSpace();
                string nodesetFileName = BrowserController._nodeSetFilename[BrowserController._nodeSetFilename.Count - 1];
                uaAddressSpace.Nodeset.NodesetXml = System.IO.File.ReadAllText(nodesetFileName);

                webClient.Headers.Add("Content-Type", "application/json");
                webClient.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + secret)));
                string body = JsonConvert.SerializeObject(uaAddressSpace);
                string response = webClient.UploadString(webClient.BaseAddress + "InfoModel/upload", "PUT", body);
                webClient.Dispose();

                AddressSpace returnedAddressSpace = (AddressSpace) JsonConvert.DeserializeObject(response);
                if (!string.IsNullOrEmpty(returnedAddressSpace.Nodeset.AddressSpaceID))
                {
                    uaclModel.StatusMessage = "Upload successful!";
                    return View("Index", uaclModel);
                }
                else
                {
                    uaclModel.StatusMessage = response;
                    return View("Error", uaclModel);
                }
            }
            catch (Exception ex)
            {
                uaclModel.StatusMessage = ex.Message;
                return View("Error", uaclModel);
            }
        }
    }
}
