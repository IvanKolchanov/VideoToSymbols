# VideoToSymbols
##Main goal of the project: converting a video to a file, that then could be replayed at any time as a video consisting of symbols
##Features:
  - converting a video to symbols of three formats (4 symbols (2-bit), 8 symbols (3-bit), 16 symbols (4-bit))
  - replaying any videos saved in the \\videos folder of the project in format .vts

##How does the program work:
  1. When provided a video the program uses library Accord.Video.FFMPEG as a way to slice the video into separate frames
  2. After that the frames are one after one converted into images depending on the current console size (the video would have the biggest possible size for the given font size when replayed)
  3. Depending on the chosen convertion format the every frame would be turned into one of 
