using System;

namespace Touhou
{
#if WINDOWS || XBOX
    static class Program
    {
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