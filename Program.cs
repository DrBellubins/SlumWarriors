namespace SlumWarriors
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting engine...");
            Engine.IsServer = args[0] == "--server";

            var engine = new Engine();
            engine.Initialize();
        }
    }
}
