using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Sample.Simulation;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UANodesetWebViewer
{
    public class Program
    {
        public static ApplicationConfiguration _config;

        public static void Main(string[] args)
        {
            ConsoleServer(args).Wait();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static async Task ConsoleServer(string[] args)
        {
            // load the application configuration.
            ApplicationInstance application = new ApplicationInstance();
            _config = await application.LoadApplicationConfiguration(Path.Combine(Directory.GetCurrentDirectory(), "Application.Config.xml"), false).ConfigureAwait(false);

            // check the application certificate.
            await application.CheckApplicationInstanceCertificate(false, 0).ConfigureAwait(false);

            // create cert validator
            _config.CertificateValidator = new CertificateValidator();
            _config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);

            // start the server.
            await application.Start(new SimpleServer()).ConfigureAwait(false);
        }

        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                // accept all OPC UA client certificates
                Console.WriteLine("Automatically trusting client certificate " + e.Certificate.Subject);
                e.Accept = true;
            }
        }
    }
}
