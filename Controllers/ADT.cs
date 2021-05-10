
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.IO.Packaging;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
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
                if (response.GetRawResponse().Status == 201)
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

        public ActionResult GenerateAAS()
        {
            try
            {
                string packagePath = Path.Combine(Directory.GetCurrentDirectory(), "DTDL.aasx");
                using (Package package = Package.Open(packagePath, FileMode.Create))
                {
                    // add package origin part
                    PackagePart origin = package.CreatePart(new Uri("/aasx/aasx-origin", UriKind.Relative), MediaTypeNames.Text.Plain, CompressionOption.Maximum);
                    using (Stream fileStream = origin.GetStream(FileMode.Create))
                    {
                        var bytes = Encoding.ASCII.GetBytes("Intentionally empty.");
                        fileStream.Write(bytes, 0, bytes.Length);
                    }
                    package.CreateRelationship(origin.Uri, TargetMode.Internal, "http://www.admin-shell.io/aasx/relationships/aasx-origin");

                    // create package spec part
                    string packageSpecPath = Path.Combine(Directory.GetCurrentDirectory(), "aasenv-with-no-id.aas.xml");
                    using (StringReader reader = new StringReader(System.IO.File.ReadAllText(packageSpecPath)))
                    {
                        XmlSerializer aasSerializer = new XmlSerializer(typeof(AasEnv));
                        AasEnv aasEnv = (AasEnv)aasSerializer.Deserialize(reader);

                        aasEnv.AssetAdministrationShells.AssetAdministrationShell.SubmodelRefs.Clear();
                        aasEnv.Submodels.Clear();

                        string filename = Path.GetFileNameWithoutExtension(BrowserController._nodeSetFilename[0]);

                        string submodelPath = Path.Combine(Directory.GetCurrentDirectory(), "submodel.adt.xml");
                        using (StringReader reader2 = new StringReader(System.IO.File.ReadAllText(submodelPath)))
                        {
                            XmlSerializer aasSubModelSerializer = new XmlSerializer(typeof(AASSubModel));
                            AASSubModel aasSubModel = (AASSubModel)aasSubModelSerializer.Deserialize(reader2);

                            SubmodelRef nodesetReference = new SubmodelRef();
                            nodesetReference.Keys = new Keys();
                            nodesetReference.Keys.Key = new Key
                            {
                                IdType = "URI",
                                Local = true,
                                Type = "Submodel",
                                Text = "http://www.microsoft.com/type/dtdl/" + filename.Replace(".", "").ToLower()
                            };

                            aasEnv.AssetAdministrationShells.AssetAdministrationShell.SubmodelRefs.Add(nodesetReference);

                            aasSubModel.Identification.Text += filename.Replace(".", "").ToLower();
                            aasSubModel.SubmodelElements.SubmodelElement.SubmodelElementCollection.Value.SubmodelElement.File.Value =
                                aasSubModel.SubmodelElements.SubmodelElement.SubmodelElementCollection.Value.SubmodelElement.File.Value.Replace("TOBEREPLACED", filename);
                            aasEnv.Submodels.Add(aasSubModel);
                        }

                        XmlTextWriter aasWriter = new XmlTextWriter(packageSpecPath, Encoding.UTF8);
                        aasSerializer.Serialize(aasWriter, aasEnv);
                        aasWriter.Close();
                    }

                    // add package spec part
                    PackagePart spec = package.CreatePart(new Uri("/aasx/aasenv-with-no-id/aasenv-with-no-id.aas.xml", UriKind.Relative), MediaTypeNames.Text.Xml);
                    using (FileStream fileStream = new FileStream(packageSpecPath, FileMode.Open, FileAccess.Read))
                    {
                        CopyStream(fileStream, spec.GetStream());
                    }
                    origin.CreateRelationship(spec.Uri, TargetMode.Internal, "http://www.admin-shell.io/aasx/relationships/aas-spec");

                    // add DTDL file
                    string dtdlPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(BrowserController._nodeSetFilename[0]) + ".dtdl.json");

                    PackagePart supplementalDoc = package.CreatePart(new Uri("/aasx/" + BrowserController._nodeSetFilename[0], UriKind.Relative), MediaTypeNames.Text.Xml);
                    string documentPath = Path.Combine(Directory.GetCurrentDirectory(), BrowserController._nodeSetFilename[0]);
                    using (FileStream fileStream = new FileStream(documentPath, FileMode.Open, FileAccess.Read))
                    {
                        CopyStream(fileStream, supplementalDoc.GetStream());
                    }
                    package.CreateRelationship(supplementalDoc.Uri, TargetMode.Internal, "http://www.admin-shell.io/aasx/relationships/aas-suppl");
                }

                return File(new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "DTDL.aasx"), FileMode.Open, FileAccess.Read), "APPLICATION/octet-stream", "DTDL.aasx");
            }
            catch (Exception ex)
            {
                ADTModel model = new ADTModel
                {
                    ErrorMessage = HttpUtility.HtmlDecode(ex.Message)
                };

                return View("Error", model);
            }
        }

        private void CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {
                target.Write(buf, 0, bytesRead);
            }
        }
    }
}
