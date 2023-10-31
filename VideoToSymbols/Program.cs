using FastConsole;
using VideoToSymbols;

namespace ImageToSymbols
{
    class Program
    {
        public static String pathToExe = Environment.CurrentDirectory;
        public static bool programRunning = true;

        public static void Main(string[] args)
        {
            FConsole.Initialize("Video to symbols", ConsoleColor.White, ConsoleColor.Black);
            Directory.CreateDirectory(pathToExe + "\\videos");
            openMainMenu();
        }

        private static void openMainMenu()
        {
            while (programRunning)
            {
                Console.Title = "Video to symbols: Main menu";
                Console.SetCursorPosition(0, 0);
                Console.Clear();
                Console.WriteLine("1. Convert a video to symbols\n" +
                    "2. Play existing .vts (video to symbols) \n" +
                    "\nPress the number of command you want to activate");
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                        VideoConverter.openConvertVideoMenu();
                        break;
                    case ConsoleKey.D2:
                        VideoPlayer.openPlayVideoMenu();
                        break;
                    case ConsoleKey.Escape:
                        programRunning = false;
                        break;
                }
            }
        }
    }
}