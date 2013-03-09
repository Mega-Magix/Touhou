using System;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

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
            Console.WriteLine(" 0 - Touhou Windowed");
            Console.WriteLine(" 1 - ExampleSprite.cs");
            Console.WriteLine(" 2 - (IronPython test)");
            Console.WriteLine(" 3 - Touhou Fullscreen");
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
                var ipy = Python.CreateRuntime();
                dynamic test = ipy.UseFile("../../PythonScripts/HelloWorld.py");
                int result = test.square(4);
                System.Console.WriteLine(result);
                System.Console.ReadLine();
            }
            if (runType == "3")
            {
                Console.Write("Enter screen width: ");
                int width = int.Parse(Console.ReadLine());
                Console.Write("Enter screen height: ");
                int height = int.Parse(Console.ReadLine());
                using (Touhou game = new Touhou(true, (double)width / (double)height)) game.Run();
            }
        }
    }
#endif
}