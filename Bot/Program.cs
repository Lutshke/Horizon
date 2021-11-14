using System;

namespace Horizon
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Console.IsOutputRedirected)
                Console.Title = "Music Bitch";

            new Bot().RunAsync().GetAwaiter().GetResult();
        }
    }
}
