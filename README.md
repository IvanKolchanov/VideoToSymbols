# VideoToSymbols
## Planned development:
  - implement parallel processing for frames during convertion to lower time
  - implement parallel processing for next frame during play to stabilize fps
  - implement colorful and black&white video

## Main goal of the project: converting a video to a file, that then could be replayed at any time as a video consisting of symbols
## Features:
  - converting a video to symbols of three formats (4 symbols (2-bit), 8 symbols (3-bit), 16 symbols (4-bit))
  - replaying any videos saved in the \\videos folder of the project in format .vts

## How does the program work:
  1. When provided a video the program uses library Accord.Video.FFMPEG as a way to slice the video into separate frames
  2. After that the frames are one after one converted into images depending on the current console size (the video would have the biggest possible size for the given font size when replayed)
  3. Depending on the chosen convertion format every group of pixels that is converted into one symbol is recorded into a file as (2 to 4 bits). (When first developing the app I only recorded pixels that are changing in the video, however, it is inefficient because saving coordinates would take at least 2 bytes per symbol, but recording only changing symbols on average removes only a few percent of changed symbols per frame).
  4. Program saves memory by saving every bit on recording data. Even though it is only possible to write bytes in files, by using a byte to store 4 2-bit characters (or 3 bytes to store 8 3-bit characters, or a byte to store 2 4-bit characters) a lot of space is saved.
  5. When the video is played after convertion it simple reads all the data from the file and adjusts the speed of playing by using a timer and trying to follow the frame rate. Also the program uses a class that allows to manually update the Console buffer, because otherwise it updates after every changed symbol significantly slowing down the work of the program and creating a strange visual effect.

## Main menu:
  1. Convert a video to symbols
  2. Play existing .vts (video to symbols) <br>

![image](https://github.com/IvanKolchanov/VideoToSymbols/assets/83294629/bba958ab-dc57-4660-996c-1353d5b6bb19)

## Convertion menu: <br>
![image](https://github.com/IvanKolchanov/VideoToSymbols/assets/83294629/e8721e9a-3e7e-471d-b24a-db23e6ace822) <br>
After choosing the convertion settings user sees the menu which shows information about the convertion progress and estimated time left. 

## File play menu: <br>
![image](https://github.com/IvanKolchanov/VideoToSymbols/assets/83294629/f9d2dd7c-3d63-4966-a26b-d96cac4c36c7)

The menu displays all of the .vts files saved in the /video folder of the application. To chose between files the user need to use arrows and depending on the size of the font used when converting the user might need to change current font size to play the video (playablitiy of the video is showed by the tick/cross near it, along with other parameters).

## Examples:
### Original video (720x720, 60fps): <br>
https://github.com/IvanKolchanov/VideoToSymbols/assets/83294629/a8e40ebf-f2ec-4d89-a535-b1e6e9f1e0bf
### 16 symbols, 4-bit convertion ():

### 8 symbols, 3-bit convertion ():

### 4 symbols, 2-bit convertion ():





