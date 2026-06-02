namespace Activity.IntegrationTests.Classes.Tokens
{
    public interface IAnimal
    {
        string Name { get; }
    }

    public class Animal : IAnimal
    {
        public string Name { get; set; }
    }

    public class Dog : Animal
    {
        public string Breed { get; set; }
    }

    public class Cat : Animal
    {
        public string Color { get; set; }
    }
}

