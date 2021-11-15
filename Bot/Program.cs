using System;

namespace Horizon
{
    class Program
    {
        static void Main(string[] args)
        {
            // if (!Console.IsOutputRedirected)
            //     Console.Title = "Music Bitch";

            new Bot().RunAsync().GetAwaiter().GetResult();

            // var mananger = new Extensions.Database.DatabaseManager();
            // var guid = mananger.AddPlaylist(new Extensions.Database.DBPlaylist()
            // {
            //     Title = "Booba Seggs",
            //     Author = 234704361740173313,
            //     Tracks = new()
            //     {
            //         "https://open.spotify.com/track/5Hyr47BBGpvOfcykSCcaw9?si=87dfd2139d654778",
            //         "https://soundcloud.com/kryptobeatss/ssio-x-farid-bang-popsmoke-dance-david-guetta-sexy-bitch-ehrenloser-club-remix-original",
            //         "https://www.youtube.com/watch?v=F3kPFSodPIE"
            //     }
            // }).GetAwaiter().GetResult();
            // Console.WriteLine();
            // mananger.SavePlaylist().GetAwaiter().GetResult();
            // Console.ReadLine();
        }
    }
}
