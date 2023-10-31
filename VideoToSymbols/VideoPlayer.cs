using FastConsole;
using System.Diagnostics;

namespace VideoToSymbols
{
    class VideoPlayer
    {
        private String path;
        public VideoPlayer(String path) { 
            this.path = path;
        } 
        
        public static void openPlayVideoMenu()
        {
            Console.Clear();
            Console.Title = "Run video to symbols";
            String pathToExe = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(pathToExe + "\\videos");
            if (files.Length == 0) {
                Console.WriteLine("You do not have any saved files" + "\n" + 
                    "Press Enter to return to Main menu");
                Console.ReadLine();
                return; 
            }
            string[] fileNames = new string[files.Length];
            int[,] cursorPos = new int[files.Length, 2];
            for (int i = 0; i < files.Length; i++)
            {
                String[] arr = files[i].Split('\\');
                fileNames[i] = arr[arr.Length - 1];
                Console.Write(i + 1 + ". " + fileNames[i]);
                cursorPos[i, 0] = Console.CursorLeft + 1; cursorPos[i, 1] = Console.CursorTop;
                Console.Write("\n");
            }
            Console.WriteLine("\nChoose file buy pressing up/down arrows");
            Console.WriteLine("✔ - means that the file can be played with current font size");
            Console.WriteLine("❌-  means that the file cannot be played with current font size");
            int index = 0;
            Console.SetCursorPosition(cursorPos[0, 0], cursorPos[0, 1]);
            Console.Write("█<- ");
            string[] videoInfo = VideoPlayer.getVideoInfo(files[index]);
            Console.Write(videoInfo[0] + " " + videoInfo[1] + " " + videoInfo[2] + " " + videoInfo[3]);
            bool choosing = true;
            bool exit = false;
            while (choosing)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        Console.SetCursorPosition(cursorPos[index, 0], cursorPos[index, 1]);
                        Console.Write("                                     ");
                        index--;
                        if (index < 0) index = files.Length - 1;
                        Console.SetCursorPosition(cursorPos[index, 0], cursorPos[index, 1]);
                        Console.Write("█<- ");
                        videoInfo = VideoPlayer.getVideoInfo(files[index]);
                        Console.Write(videoInfo[0] + " " + videoInfo[1] + " " + videoInfo[2] + " " + videoInfo[3]);
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        Console.SetCursorPosition(cursorPos[index, 0], cursorPos[index, 1]);
                        Console.Write("                                      ");
                        index++;
                        if (index == files.Length) index = 0;
                        Console.SetCursorPosition(cursorPos[index, 0], cursorPos[index, 1]);
                        Console.Write("█<- ");
                        videoInfo = VideoPlayer.getVideoInfo(files[index]);
                        Console.Write(videoInfo[0] + " " + videoInfo[1] + " " + videoInfo[2] + " " + videoInfo[3]);
                        break;
                    case ConsoleKey.Enter:
                        VideoPlayer vp = new VideoPlayer(Directory.GetFiles(pathToExe + "\\videos")[index]);
                        Console.Clear();
                        vp.play();
                        Console.ReadLine();
                        choosing = false;
                        break;
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                }
                if (exit) break;
            }
        }

        public static string[] getVideoInfo(String path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            string[] result = new string[4];
            int width, height;
            width = fs.ReadByte() * 256 + fs.ReadByte();
            height = fs.ReadByte() * 256 + fs.ReadByte();
            result[0] = width + "x" + height;
            byte[] frameCountByte = new byte[3];
            fs.Read(frameCountByte, 0, 3);
            long frameCount = frameCountByte[2] + frameCountByte[1] * 256 + frameCountByte[0] * 256 * 256;
            int frameRate = fs.ReadByte();
            result[1] = Math.Round((double)frameCount / frameRate, 2).ToString() + " seconds";
            result[2] = frameRate.ToString();
            if (width <= Console.LargestWindowWidth && height <= Console.LargestWindowHeight) result[3] = "✔";
            else result[3] = "❌";
            fs.Close();
            return result;
        }

        public void play()
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            int width, height;
            width = fs.ReadByte() * 256;
            width += fs.ReadByte();
            height = fs.ReadByte() * 256;
            height += fs.ReadByte();
            if (width > Console.LargestWindowWidth || height > Console.LargestWindowHeight)
            {
                Console.WriteLine("The console font is to big for the choosen file, make the font smaller");
                Console.ReadLine();
                return;
            }
            Console.SetWindowSize(width, height);
            FConsole.Clear();
            FConsole.Initialize("Run video to symbols", ConsoleColor.White, ConsoleColor.Black);

            byte[] frameCountByte = new byte[3];
            fs.Read(frameCountByte, 0, 3);
            long frameCount = frameCountByte[0] * 256 * 256 + frameCountByte[1] * 256 + frameCountByte[2];
            int frameRate = fs.ReadByte();
            int millisecondsPerFrame = 1000 / frameRate;

            int conversionRate = fs.ReadByte();
            char[] symbolList = GradientProvider.getSymbolList(conversionRate).ToCharArray();
            
            short i = 0, j = 0;
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            int symId = -1;

            bool running = true;
            var source = new TaskCompletionSource<bool>();
            Task escapeButton = Task.Factory.StartNew(() =>
            {
                while (running) if (Console.ReadKey(true).Key == ConsoleKey.Escape) running = false;
                source.SetResult(true);
            });

            while (frameCount > 1)
            {
                if (!running) break;
                try
                {
                    PixelValue pixelValue;
                    switch (conversionRate)
                    {
                        case GradientProvider.CONVERSION_4_SYM:
                            int input4 = fs.ReadByte();
                            symId = input4 >> 6;
                            pixelValue = new PixelValue(ConsoleColor.White, ConsoleColor.Black, symbolList[symId]);
                            FConsole.SetChar(i++, j, pixelValue);
                            if (i == width) { i = 0; j++;
                                if (width % 4 == 1) break;
                            }
                            if (i == 0 && j == height) break;
                            
                            symId = (input4 >> 4) & 3;
                            pixelValue = new PixelValue(ConsoleColor.White, ConsoleColor.Black, symbolList[symId]);
                            FConsole.SetChar(i++, j, pixelValue);
                            if (i == width)
                            {
                                i = 0; j++;
                                if (width % 4 == 2) break;
                            }
                            if (i == 0 && j == height) break;
                            
                            symId = (input4 >> 2) & 3;
                            pixelValue = new PixelValue(ConsoleColor.White, ConsoleColor.Black, symbolList[symId]);
                            FConsole.SetChar(i++, j, pixelValue);
                            if (i == width)
                            {
                                i = 0; j++;
                                if (width % 4 == 3) break;
                            }
                            if (i == 0 && j == height) break;

                            symId = input4 & 3;
                            pixelValue = new PixelValue(ConsoleColor.White, ConsoleColor.Black, symbolList[symId]);
                            FConsole.SetChar(i++, j, pixelValue);
                            if (i == width) { i = 0; j++; }
                            break;
                        case GradientProvider.CONVERSION_8_SYM:
                            byte[] input8 = new byte[3];
                            fs.Read(input8, 0, input8.Length);
                            int input8Int = input8[0] * 256 * 256 + input8[1] * 256 + input8[2];
                            int[] symbolsIds = new int[8];
                            for (int z = 0; z < 8; z++)
                            {
                                symbolsIds[7 - z] = input8Int & 7;
                                input8Int >>= 3;
                            }

                            for (int z = 0; z < 8; z++)
                            {
                                symId = symbolsIds[z];
                                pixelValue = new PixelValue(ConsoleColor.White, ConsoleColor.Black, symbolList[symId]);
                                FConsole.SetChar(i++, j, pixelValue);
                                if (i == width)
                                {
                                    i = 0; j++;
                                    if (width % 8 == z + 1) break;
                                }
                                if (i == 0 && j == height) break;
                            }
                            break;
                        case GradientProvider.CONVERSION_16_SYM:
                            int input16 = fs.ReadByte();
                            symId = input16 >> 4;
                            pixelValue = new PixelValue(ConsoleColor.White, ConsoleColor.Black, symbolList[symId]);
                            FConsole.SetChar(i++, j, pixelValue);
                            if (i == width) { 
                                i = 0; j++;
                                if (width % 2 == 1) break;
                            }
                            if (i == 0 && j == height) break;

                            symId = input16 & 15;
                            pixelValue = new PixelValue(ConsoleColor.White, ConsoleColor.Black, symbolList[symId]);
                            FConsole.SetChar(i++, j, pixelValue);
                            if (i == width) { i = 0; j++; }
                            break;
                    }
                    if (i == 0 && j == height)
                    {
                        i = 0;
                        j = 0;
                        FConsole.DrawBuffer();
                        sw.Stop();
                        int elapsed = (int)(millisecondsPerFrame - sw.ElapsedMilliseconds - 10);
                        if (elapsed > 0) Thread.Sleep(elapsed);
                        sw = Stopwatch.StartNew();
                        sw.Start();
                        frameCount--;
                    }
                }
                catch (Exception e) { Console.Clear(); Console.Write(e.Message + " " + symId); Console.ReadLine(); running = false; }
            }
            running = false;
            escapeButton.Wait();
            escapeButton.Dispose();
            fs.Close();
        }
    }
}
