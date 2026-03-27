using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

namespace Sample1;

class Personne
{
    public string nom { get; set; }
    public int age { get; set; }

    public Personne(string nom, int age)
    {
        this.nom = nom;
        this.age = age;
    }

    public string Hello(bool isLowercase)
    {
        string message = $"hello {nom}, you are {age}";
        return isLowercase ? message : message.ToUpper();
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Afficher les infos Personne
        Personne personne = new Personne("Clément", 24);
        string json = JsonConvert.SerializeObject(personne, Formatting.Indented);
        Console.WriteLine("=== Sérialisation JSON ===");
        Console.WriteLine(json);
        
        // Créer des images de test
        Console.WriteLine("\n=== Création des images de test ===");
        CreateTestImages();
        
        // Redimensionner une seule image
        Console.WriteLine("\n=== Redimensionnement simple ===");
        ResizeImage(@"./images/test1.png", @"./images_resized/test1_resized.png", 200, 200);
        
        // Redimensionner plusieurs images en parallèle et mesurer les performances
        Console.WriteLine("\n=== Traitement parallèle ===");
        ProcessImagesInParallel();
    }
    
    static void CreateTestImages()
    {
        // Créer 5 images de test
        for (int i = 1; i <= 5; i++)
        {
            using (var image = new Image<Rgba32>(800, 600))
            {
                // Remplir l'image avec une couleur basée sur le numéro
                image.Mutate(x => x.BackgroundColor(new Rgba32((byte)(50 * i), (byte)(100 + 30 * i), (byte)(200 - 30 * i))));
                image.SaveAsPng($@"./images/test{i}.png");
                Console.WriteLine($"Image test{i}.png créée");
            }
        }
    }
    
    static void ResizeImage(string sourcePath, string destinationPath, int width, int height)
    {
        using (var image = Image.Load(sourcePath))
        {
            int originalWidth = image.Width;
            int originalHeight = image.Height;
            
            image.Mutate(x => x.Resize(width, height));
            image.SaveAsPng(destinationPath);
            
            Console.WriteLine($"Image redimensionnée: {originalWidth}x{originalHeight} → {width}x{height}");
            Console.WriteLine($"Sauvegardée dans: {destinationPath}");
        }
    }
    
    static void ProcessImagesInParallel()
    {
        var imageFiles = Directory.GetFiles(@"./images", "test*.png");
        
        Console.WriteLine($"Traitement de {imageFiles.Length} images...");
        
        // Test séquentiel
        Console.WriteLine("\n--- Traitement SÉQUENTIEL ---");
        var stopwatchSequential = Stopwatch.StartNew();
        
        foreach (var imageFile in imageFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(imageFile);
            string outputPath = $@"./images_resized/{fileName}_sequential.png";
            
            using (var image = Image.Load(imageFile))
            {
                int newWidth = 300;
                int newHeight = 225;
                image.Mutate(x => x.Resize(newWidth, newHeight));
                image.SaveAsPng(outputPath);
            }
            
            Console.WriteLine($"✓ {fileName} redimensionnée (300x225)");
        }
        
        stopwatchSequential.Stop();
        Console.WriteLine($"Temps total (séquentiel): {stopwatchSequential.ElapsedMilliseconds}ms");
        
        // Test parallèle
        Console.WriteLine("\n--- Traitement PARALLÈLE ---");
        var stopwatchParallel = Stopwatch.StartNew();
        
        Parallel.ForEach(imageFiles, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, imageFile =>
        {
            string fileName = Path.GetFileNameWithoutExtension(imageFile);
            string outputPath = $@"./images_resized/{fileName}_parallel.png";
            
            using (var image = Image.Load(imageFile))
            {
                int newWidth = 300;
                int newHeight = 225;
                image.Mutate(x => x.Resize(newWidth, newHeight));
                image.SaveAsPng(outputPath);
            }
            
            Console.WriteLine($"✓ {fileName} redimensionnée (300x225)");
        });
        
        stopwatchParallel.Stop();
        Console.WriteLine($"Temps total (parallèle): {stopwatchParallel.ElapsedMilliseconds}ms");
        
        // Comparaison
        Console.WriteLine("\n--- RÉSULTATS ---");
        double improvement = (double)stopwatchSequential.ElapsedMilliseconds / stopwatchParallel.ElapsedMilliseconds;
        double timeSaved = stopwatchSequential.ElapsedMilliseconds - stopwatchParallel.ElapsedMilliseconds;
        
        Console.WriteLine($"Séquentiel: {stopwatchSequential.ElapsedMilliseconds}ms");
        Console.WriteLine($"Parallèle:  {stopwatchParallel.ElapsedMilliseconds}ms");
        Console.WriteLine($"Amélioration: {improvement:F2}x plus rapide");
        Console.WriteLine($"Temps économisé: {timeSaved}ms ({(timeSaved/stopwatchSequential.ElapsedMilliseconds*100):F1}%)");
        Console.WriteLine($"Nombre de processeurs utilisés: {Environment.ProcessorCount}");
    }
}
