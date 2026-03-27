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
        Personne personne = new Personne("Clément", 24);
        Console.WriteLine(personne.Hello(true));
    }
}
