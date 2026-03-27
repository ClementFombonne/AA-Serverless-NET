using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Company.Function
{
    public static class ResizeHttpTrigger
    {
        [FunctionName("ResizeHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!int.TryParse(req.Query["w"], out int w) || w <= 0)
                return new BadRequestObjectResult("Paramètre w invalide ou manquant.");

            if (!int.TryParse(req.Query["h"], out int h) || h <= 0)
                return new BadRequestObjectResult("Paramètre h invalide ou manquant.");

            byte[] targetImageBytes;

            try
            {
                using (var msInput = new MemoryStream())
                {
                    await req.Body.CopyToAsync(msInput);
                    msInput.Position = 0;

                    using (var image = Image.Load(msInput))
                    {
                        image.Mutate(x => x.Resize(w, h));

                        using (var msOutput = new MemoryStream())
                        {
                            image.SaveAsJpeg(msOutput);
                            targetImageBytes = msOutput.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Erreur lors du traitement de l'image.");
                return new BadRequestObjectResult("Le fichier envoyé n'est pas une image valide.");
            }

            return new FileContentResult(targetImageBytes, "image/jpeg");
        }
    }
}