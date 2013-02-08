using System;

namespace Touhou
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            using (ExampleSprite.ExampleSprite game = new ExampleSprite.ExampleSprite())
            {
                game.Run();
            }
        }
    }
#endif
}