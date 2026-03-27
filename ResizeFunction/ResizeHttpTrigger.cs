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

namespace Company.Function;

public static class ResizeHttpTrigger
{
    /// <summary>Redimensionne une image reçue via POST.</summary>
    [FunctionName("ResizeHttpTrigger")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", "patch", "options", Route = null)] HttpRequest req,
        ILogger log)
    {
        // 0. Validation stricte de la méthode HTTP (405)
        if (!req.Method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
        {
            log.LogWarning($"Méthode HTTP non autorisée tentée : {req.Method}");
            return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
        }

        // 1. Validation de la syntaxe (400)
        if (string.IsNullOrEmpty(req.Query["w"]) || !int.TryParse(req.Query["w"], out int w) ||
            string.IsNullOrEmpty(req.Query["h"]) || !int.TryParse(req.Query["h"], out int h))
        {
            return new BadRequestObjectResult("Paramètres 'w' et 'h' manquants ou mal formattés.");
        }

        // 2. Validation de la sémantique et protection DoS (422)
        if (w <= 0 || h <= 0 || w > 4000 || h > 4000)
        {
            return new UnprocessableEntityObjectResult("Les dimensions doivent être comprises entre 1 et 4000 pixels.");
        }

        // 3. Validation de la présence de données (400)
        if (req.ContentLength == null || req.ContentLength == 0)
        {
            return new BadRequestObjectResult("Le corps de la requête est vide.");
        }

        byte[] targetImageBytes;

        try
        {
            using var msInput = new MemoryStream();
            await req.Body.CopyToAsync(msInput);
            msInput.Position = 0;

            using var image = Image.Load(msInput);
            image.Mutate(x => x.Resize(w, h));

            using var msOutput = new MemoryStream();
            image.SaveAsJpeg(msOutput);
            targetImageBytes = msOutput.ToArray();
        }
        catch (UnknownImageFormatException ex)
        {
            // 4. Validation du type de média (415)
            log.LogWarning(ex, "Format de fichier non supporté.");
            return new ObjectResult("Le type de fichier n'est pas une image supportée.") { StatusCode = 415 };
        }
        catch (ImageFormatException ex)
        {
            // Fichier reconnu comme image mais données corrompues (422)
            log.LogWarning(ex, "Données d'image corrompues.");
            return new UnprocessableEntityObjectResult("Le contenu du fichier image est invalide ou corrompu.");
        }
        catch (Exception ex)
        {
            // Filet de sécurité strict pour éviter les 500
            log.LogError(ex, "Erreur interne interceptée.");
            return new BadRequestObjectResult("Impossible de traiter la requête avec les données fournies.");
        }

        return new FileContentResult(targetImageBytes, "image/jpeg");
    }
}
