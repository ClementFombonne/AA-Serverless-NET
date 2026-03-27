using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Data;
using System.Diagnostics;

namespace AA_Serverless_NET;

class Program
{
    static void Main(string[] args)
    {
        // 1-2. Hello World and Json Serialization
        Personne utilisateur = new Personne 
        { 
            Nom = "Clément", 
            Age = 22
        };

        string json = JsonConvert.SerializeObject(utilisateur, Formatting.Indented);

        Console.WriteLine(json);

        // 3. Images Processor
        var processor = new ImageProcessor();

        string input = "images_input";
        string output_seq = "images_output/sequentiel";
        string output_par = "images_output/parallele";

        processor.ProcessFolder(input, output_seq, false);
        processor.ProcessFolder(input, output_par, true);
        
        Console.WriteLine("Traitement terminé.");
    }
}

class Personne
{
    public required string Nom { get; set; }
    public int Age { get; set; }

    public string Hello(bool isLowercase)
    {
        string message = $"hello {Nom}, you are {Age}";
        return isLowercase ? message.ToLower() : message.ToUpper();
    }
}

class ImageProcessor
{
    public void ProcessFolder(string sourcePath, string outputPath, bool useParallel = true)

    {
        Directory.CreateDirectory(outputPath);
        string[] files = Directory.GetFiles(sourcePath, "*.*");

        if (files.Length == 0) return;

        Stopwatch sw = Stopwatch.StartNew();

        string type = useParallel ? "Parallèle" : "Séquentiel";
        if (useParallel)
        {
            Parallel.ForEach(files, filePath =>
            {
                ProcessSingleImage(filePath, outputPath);
            });
        } else
        {
            foreach (var filePath in files)
            {
                ProcessSingleImage(filePath, outputPath);
            }

        }

        sw.Stop();
        Console.WriteLine($"[{type}] {files.Length} images traitées en {sw.ElapsedMilliseconds} ms.");
    }

    private void ProcessSingleImage(string filePath, string outputPath)
    {
        try
        {
            using Image image = Image.Load(filePath);
            string fileName = Path.GetFileName(filePath);

            image.Mutate(x => x
                .Resize(image.Width / 2, image.Height / 2)
                .Grayscale());

            image.Save(Path.Combine(outputPath, fileName));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur sur {Path.GetFileName(filePath)} : {ex.Message}");
        }
    }
}