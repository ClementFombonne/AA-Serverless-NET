using Newtonsoft.Json;

namespace AA_Serverless_NET;

class Program
{
    static void Main(string[] args)
    {
        Personne utilisateur = new Personne 
        { 
            Nom = "Clément", 
            Age = 22
        };

        string json = JsonConvert.SerializeObject(utilisateur, Formatting.Indented);

        Console.WriteLine(json);
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
