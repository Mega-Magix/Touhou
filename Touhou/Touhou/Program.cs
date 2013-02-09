using System;

namespace Touhou
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            IronPython.IronPython ironPython = new IronPython.IronPython();

            String runType = "";

            Console.WriteLine("Please specify which main project file to run: ");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine(" 0 - Touhou.cs");
            Console.WriteLine(" 1 - ExampleSprite.cs");
            Console.WriteLine(" 2 - (IronPython test)");
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
            if (runType == "2")
            {
                Console.Write("Choose a python script to run: ");
                String moduleName;
                moduleName = Console.ReadLine();
                ironPython.RunFile(moduleName);
            }
        }
    }
#endif
}