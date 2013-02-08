using System;

namespace Touhou
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Touhou game = new Touhou())
            {
                game.Run();
            }
        }
    }
#endif
}

