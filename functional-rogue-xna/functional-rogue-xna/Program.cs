using System;

namespace functional_rogue_xna
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                // Force domain to load assembly ued as resource
                AppDomain.CurrentDomain.Load("Xna.Gui.Controls");
                game.Run();
            }
        }
    }
#endif
}

