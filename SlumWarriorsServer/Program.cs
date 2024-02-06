namespace SlumWarriorsServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Starting engine...");

            var engine = new Engine();
            engine.Initialize();
        }
    }
}
