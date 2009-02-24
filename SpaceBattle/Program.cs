using System;

namespace SpaceBattle
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SpaceBattle game = new SpaceBattle())
            {
                game.Run();
            }
        }
    }
}

