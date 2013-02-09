using System;

namespace Touhou
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            String runType = "";

            Console.WriteLine("Please specify which main project file to run: ");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine(" 0 - Touhou.cs");
            Console.WriteLine(" 1 - ExampleSprite.cs");
            Console.WriteLine("-------------------------------------------------");
            Console.Write(" :: ");
            runType = Console.ReadLine();

            if (runType == "0")
            {
                using (Touhou game = new Touhou()) game.Run();
            }
            if (runType == "1")
            {
                using (ExampleSprite.ExampleSprite game = new ExampleSprite.ExampleSprite()) game.Run();
            }
        }
    }
#endif
}