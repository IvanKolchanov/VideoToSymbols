using Accord.Video.FFMPEG;
using ConsoleExtender;
using System.Diagnostics;
using System.Drawing;

namespace VideoToSymbols
{
    class VideoConverter
    {
        
        private static Dictionary<char, int> symbolLightness;
        private static Dictionary<int, char> lightnessToSymbol;
        private static int[] lightnessList;

        private int conversionRate = GradientProvider.CONVERSION_16_SYM;

        private int width, height;
        private int imageWidth, imageHeight;

        private int drawMode;
        public const int DRAW_SYMBOLS = 0, DRAW_IMAGE_COLOR = 1, DRAW_IMAGE_GREY = 2;
        private double coefficient = 1;
        private double fontAspect = 2.0;
        private bool colorInversion = false;
        
        private String videoPath;

        public VideoConverter(String videoPath, int drawMode)
        {
            this.videoPath = videoPath; 
            this.drawMode = drawMode;
        }

        private static int findSymbolIndexByLightness(double L)
        {
            int integerL = (int)(L * 10000.0);
            int id = Array.BinarySearch(lightnessList, integerL);
            if (id < 0) id = -id - 2;
            return id;
        }

        private static double sRGBtoLin(double colorChannel)
        {
            if (colorChannel <= 0.04045) return colorChannel / 12.92;
            else return Math.Pow((colorChannel + 0.055) / 1.055, 2.4);
        }

        private static double findLightness(long R, long G, long B)
        {
            double vR = R / 255.0, vG = G / 255.0, vB = B / 255.0;
            double lR = sRGBtoLin(vR), lG = sRGBtoLin(vG), lB = sRGBtoLin(vB);
            double Y = lR * 0.2126 + lG * 0.7152 + lB * 0.0722;
            if (Y <= 0.008856) return Y * 903.3;
            else return Math.Pow(Y, 1.0 / 3.0) * 116 - 16;
        }

        private void normalizeImage(Bitmap image)
        {
            imageWidth = image.Width;
            imageHeight = image.Height;
            fontAspect = (double)ConsoleHelper.GetConsoleFontSize().Height / ConsoleHelper.GetConsoleFontSize().Width;
            if (ConsoleHelper.GetConsoleFontSize().Height <= 5) fontAspect = 2;

            if (Console.LargestWindowWidth >= image.Width && Console.LargestWindowHeight >= image.Height / fontAspect)
            {
                Console.SetWindowSize(image.Width, (int)(image.Height / fontAspect));
                width = Console.WindowWidth;
                height = Console.WindowHeight;
                return;
            }

            double imageAspect = (double)image.Height / image.Width;

            if (Console.LargestWindowWidth >= image.Width)
            {
                height = Console.LargestWindowHeight;
                width = (int)(height / imageAspect * fontAspect);
                Console.SetWindowSize(width, height);
                return;
            }

            if (Console.LargestWindowHeight >= image.Height / fontAspect)
            {
                width = Console.LargestWindowWidth;
                height = (int)(width * imageAspect / fontAspect);
                Console.SetWindowSize(width, height);
                return;
            }

            if (Console.LargestWindowWidth / fontAspect >= Console.LargestWindowHeight / imageAspect) //choosing width and height according to maximum quality
            {
                height = Console.LargestWindowHeight;
                width = (int)(height / imageAspect * fontAspect);
            }
            else
            {
                width = Console.LargestWindowWidth;
                height = (int)(width * imageAspect / fontAspect);
            }
        }

        private int wAspect, hAspect;

        private int biggerAspectNumW, smallerAspectNumW;
        private int diffW, changeW;
        private bool aspectMoreW;

        private int biggerAspectNumH, smallerAspectNumH;
        private int diffH, changeH;
        private bool aspectMoreH;

        private void initializeImageVariables()
        {
            this.wAspect = Math.Max(imageWidth / width, 1); //getting width and height aspects for the conversion
            this.hAspect = Math.Max(imageHeight / height, 1);

            biggerAspectNumW = imageWidth - wAspect * width; smallerAspectNumW = width - biggerAspectNumW; //biggerAspectNumW and smallerAspectNumW
            int aspectNumMaxW = Math.Max(biggerAspectNumW, smallerAspectNumW), aspectNumMinW = biggerAspectNumW + smallerAspectNumW - aspectNumMaxW;
            int diffW, changeW;
            aspectMoreW = biggerAspectNumW > smallerAspectNumW;

            if (aspectNumMinW != 0)
            {
                changeW = aspectNumMaxW / aspectNumMinW + 1;
                diffW = changeW * aspectNumMinW;
            }
            else
            {
                diffW = -1;
                changeW = 1;
            }
            this.diffW = diffW;
            this.changeW = changeW;

            if (aspectMoreW) wAspect++;

            biggerAspectNumH = imageHeight - hAspect * height; smallerAspectNumH = height - biggerAspectNumH;
            int aspectNumMaxH = Math.Max(biggerAspectNumH, smallerAspectNumH), aspectNumMinH = biggerAspectNumH + smallerAspectNumH - aspectNumMaxH;
            int changeH, diffH;
            aspectMoreH = biggerAspectNumH > smallerAspectNumH;

            if (aspectNumMinH != 0)
            {
                changeH = aspectNumMaxH / aspectNumMinH + 1;
                diffH = changeH * aspectNumMinH;
            }
            else
            {
                diffH = -1; changeH = 1;
            }
            this.diffH = diffH;
            this.changeH = changeH;

            if (aspectMoreH) hAspect++;
        }

        private void convertImage(Bitmap image, FileStream fileStream)
        {
            int diffW = this.diffW;
            int diffH = this.diffH;
            int imageCordX = 0, imageCordY = 0;
            for (int j = 0; j < height; j++)
            {
                int maxSymbolCount = GradientProvider.getSymbolCount(conversionRate);
                int[] idSymbols = new int[maxSymbolCount];
                int symbolCount = 0;
                if (j % changeH == 0 && diffH > 0) { hAspect = aspectMoreH ? hAspect - 1 : hAspect + 1; diffH -= changeH; }
                for (int i = 0; i < width; i++)
                {
                    long sumR = 0, sumG = 0, sumB = 0;
                    if (i % changeW == 0 && diffW > 0) { wAspect = aspectMoreW ? wAspect - 1 : wAspect + 1; diffW -= changeW; }
                    for (int z = imageCordY; z < imageCordY + hAspect; z++)
                    {
                        for (int t = imageCordX; t < imageCordX + wAspect; t++)
                        {
                            Color pixel = image.GetPixel(t, z);
                            sumR += pixel.R;
                            sumG += pixel.G;
                            sumB += pixel.B;
                        }
                    }
                    sumR = (int)(sumR * coefficient / (wAspect * hAspect));
                    sumG = (int)(sumG * coefficient / (wAspect * hAspect));
                    sumB = (int)(sumB * coefficient / (wAspect * hAspect));
                    sumR = Math.Min(255, sumR);
                    sumG = Math.Min(255, sumG);
                    sumB = Math.Min(255, sumB);
                    int avgSum;
                    imageCordX += wAspect;
                    if (i % changeW == 0 && diffW >= 0) wAspect = aspectMoreW ? wAspect + 1 : wAspect - 1;
                    if (diffW == 0) { diffW = -1; }

                    
                    switch (drawMode)
                    {
                        case DRAW_SYMBOLS:
                            double L = findLightness(sumR, sumG, sumB);
                            L = Math.Min(L, 100);
                            if (colorInversion) L = 100 - L;
                            int symbolId = findSymbolIndexByLightness(L);
                            idSymbols[symbolCount++] = symbolId;
                            break;
                        case DRAW_IMAGE_COLOR:
                            if (colorInversion) { sumR = 255 - sumR; sumG = 255 - sumG; sumB = 255 - sumB; }
                            Console.Write("\x1b[48;2;" + sumR + ";" + sumG + ";" + sumB + "m");
                            Console.Write(" ");
                            break;
                        case DRAW_IMAGE_GREY:
                            avgSum = (int)(findLightness(sumR, sumG, sumB) * 255.0 / 100.0);
                            if (colorInversion) avgSum = 255 - avgSum;
                            Console.Write("\x1b[48;2;" + avgSum + ";" + avgSum + ";" + avgSum + "m");
                            Console.Write(" ");
                            break;
                    }
                    if (symbolCount == maxSymbolCount)
                    {
                        byte[] output = GradientProvider.getBytesFromSymbols(conversionRate, idSymbols);
                        fileStream.Write(output, 0, output.Length);
                        idSymbols = new int[maxSymbolCount];
                        symbolCount = 0;
                    }
                }
                if (symbolCount != 0) {
                    byte[] output = GradientProvider.getBytesFromSymbols(conversionRate, idSymbols);
                    fileStream.Write(output, 0, output.Length);
                }
                diffW = this.diffW;
                imageCordX = 0;
                imageCordY += hAspect;
                if (j % changeH == 0 && diffH >= 0) hAspect = aspectMoreH ? hAspect + 1 : hAspect - 1;
                if (diffH == 0) diffH--;
            }
            
        }

        private const int SUCCESS = 0, ERROR = -1, ESCAPE = 1;
        private int openConvertionInfoMenu(long frameCount)
        {
            Console.WriteLine("Choose the convertion method:");
            Console.WriteLine("1. Video to symbols\n" +
                "2. Video to RGB symbols\n" +
                "3. Video to black&white\n");
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    Console.Clear();
                    Console.WriteLine("Choose the convertion quality:");
                    
                    double defaultSize = (double)width / 1024.0 * height * frameCount / 1024.0;
                    Console.WriteLine("1. 16 symbols (4 bit) ~ " + Math.Round(defaultSize / 2, 2) + "MB\n" +
                        "2. 8 symbols (3 bit) ~ " + Math.Round(defaultSize / 8.0 * 3.0, 2) + "MB\n" +
                        "3. 4 symbols (2 bit) ~ " + Math.Round(defaultSize / 4, 2) + "MB\n");

                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.D1:
                            conversionRate = GradientProvider.CONVERSION_16_SYM;
                            break;
                        case ConsoleKey.D2:
                            conversionRate = GradientProvider.CONVERSION_8_SYM;
                            break;
                        case ConsoleKey.D3:
                            conversionRate = GradientProvider.CONVERSION_4_SYM;
                            break;
                        case ConsoleKey.Escape:
                            return ESCAPE;
                        default:
                            Console.Clear();
                            Console.WriteLine("Invalid input");
                            Console.ReadLine();
                            return ERROR;
                    }
                    lightnessList = GradientProvider.getLightnessList(conversionRate);
                    lightnessToSymbol = GradientProvider.getLightnessToSymbol(conversionRate);
                    Console.Clear();
                    return SUCCESS;
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                    Console.Clear();
                    return ERROR;
                case ConsoleKey.Escape:
                    return ESCAPE;
                default:
                    Console.Clear();
                    Console.WriteLine("Non existing option");
                    Console.ReadLine();
                    return ERROR;
            }
        }

        public static void openConvertVideoMenu()
        {
            Console.Clear();
            Console.Title = "Video to symbols: Video Converter";
            Console.WriteLine("Enter path to the file you want to convert");
            String filePath = Console.ReadLine().Replace("\"", "");
            Console.Clear();
            VideoConverter video = new VideoConverter(filePath, VideoConverter.DRAW_SYMBOLS);

            int state = video.convertVideoToSymbols();
            Console.Clear();
            switch (state)
            {
                case SUCCESS:
                    Console.WriteLine("Video converted successfully!");
                    break;
                case ESCAPE:
                    Console.WriteLine("You cancelled the program");
                    break;
                case ERROR:
                    Console.WriteLine("An error aaccured");
                    break;
            }
            Console.WriteLine("Press Enter to return to main menu");
            Console.ReadLine();
        }

        private static int openRenameFileMenu(String videoFileName)
        {
            Console.WriteLine("A file with the name " + videoFileName + ".vts already exists");
            Console.WriteLine("Do you want to change the name of the new file or do you want to delete the old one?");
            Console.WriteLine("If you want to change the name press Y. " +
                "If you want to delete the file press N. " +
                "If you want to exit press Escape.");
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.Y:
                    Console.Clear();
                    Console.WriteLine("Print the new name without the extension (.vts)");
                    videoFileName = Console.ReadLine();
                    Console.WriteLine("The new file name is {0}", videoFileName);
                    return SUCCESS;
                case ConsoleKey.Escape:
                    return ESCAPE;
                default:
                    return ERROR;
            }
        }

        private int convertVideoToSymbols()
        {
            using (VideoFileReader vFReader = new VideoFileReader())
            {
                try { vFReader.Open(videoPath);} 
                catch (Exception e) { Console.WriteLine(e.Message); return ERROR; }
                
                Bitmap frame = vFReader.ReadVideoFrame();
                normalizeImage(frame);

                int infoMenuResult = openConvertionInfoMenu(vFReader.FrameCount);
                if (infoMenuResult == ERROR) return ERROR;

                String[] arr = videoPath.Split('\\');
                String videoFileName = arr[arr.Length - 1].Split('.')[0] + (int)Math.Pow(2, conversionRate + 2);
                String path = Environment.CurrentDirectory + "\\videos\\" + videoFileName + ".vts"; //video to symbols
                if (File.Exists(path)) openRenameFileMenu(videoFileName);
                path = Environment.CurrentDirectory + "\\videos\\" + videoFileName + ".vts";

                FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                fileStream.Write(new byte[] { (byte)(width / 256), (byte)(width % 256), (byte)(height / 256), (byte)(height % 256) }, 0, 4);

                long frames = vFReader.FrameCount;
                int frameRate = (int)vFReader.FrameRate.Numerator / vFReader.FrameRate.Denominator;
                byte firstByte = (byte)(frames % 256);
                frames /= 256;
                byte secondByte = (byte)(frames % 256);
                frames /= 256;
                byte thirdByte = (byte)(frames % 256);
                byte[] framesCountByte = new byte[] { thirdByte, secondByte, firstByte};
                byte[] frameRateByte = new byte[] { (byte)frameRate };
                fileStream.Write(framesCountByte, 0, 3);
                fileStream.Write(frameRateByte, 0, 1);
                fileStream.Write(new byte[] { (byte)conversionRate }, 0, 1);

                initializeImageVariables();
                long averageTime = 0;
                Console.Clear();
                Console.WriteLine("1     frame out of     " + vFReader.FrameCount);
                Console.WriteLine("0  %/100%");
                Console.WriteLine("      seconds left");

                bool running = true;
                var source = new TaskCompletionSource<bool>();
                Task escapeButton = Task.Factory.StartNew(() =>
                { while (running)  if (Console.ReadKey(true).Key == ConsoleKey.Escape) running = false;
                  source.SetResult(true);
                });

                for (int i = 1; i < vFReader.FrameCount; i++)
                {
                    if (!running)
                    {
                        fileStream.Close();
                        vFReader.Close();
                        escapeButton.Dispose();
                        File.Delete(path);
                        return ESCAPE;
                    }
                    int percentageLeft = (int)((double)i * 100 / vFReader.FrameCount);
                    Stopwatch sw = Stopwatch.StartNew();
                    sw.Start();
                    convertImage(frame, fileStream);
                    sw.Stop();
                    averageTime = (long)((double)averageTime * (i - 1) / i + (double)sw.ElapsedMilliseconds / i);
                    long timeLeft = (vFReader.FrameCount - i) * averageTime / 1000;
                    Console.SetCursorPosition(0, 0);
                    Console.Write(i);
                    Console.SetCursorPosition(0, 1);
                    Console.Write(percentageLeft.ToString());
                    Console.SetCursorPosition(0, 2);
                    Console.Write("      ");
                    Console.SetCursorPosition(0, 2);
                    Console.Write(timeLeft);
                    Console.SetCursorPosition(0, 3);
                    frame = vFReader.ReadVideoFrame();
                }
                running = false;
                fileStream.Close();
                vFReader.Close();
                escapeButton.Wait();
                escapeButton.Dispose();
                return SUCCESS;
            }
        }
    }
}
