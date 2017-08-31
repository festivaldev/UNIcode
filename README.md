# ṴꞤȊcode
ṴꞤȊcode is a modern character map interpretation created because of the unendurable handling the default Windows Charactermap provides.  
Also let's face it, nobody really likes the default character map: https://twitter.com/vainamov/status/893573260541128704.

##### IMPORTANT, UNIcode only runs on the .NET Framework v4.7 due to NuGet package issues with .NET Standard on any lower framework! Download the framework here: https://www.microsoft.com/de-de/download/details.aspx?id=55167.

## Features
- Full unicode support from `0000` to `10FFFF`
- Simple and easy to use thanks to a clean UI
- Rich filtering options including searching by name or displaying multiple specified characters (or character-ranges)
- Detailed character information
- Customizible grid-map
- Better performance and scrolling behaviour than the Windows Charactermap
- Supports different font weights

## Screenshots
![screenshot](https://polaroids.festival.ml/5470dce8ab17425013d04dcd8cfc0ef8.png)  
![screenshot](https://polaroids.festival.ml/77a733b246dad0f2d22fd9ac4242d411.png)  
![screenshot](https://polaroids.festival.ml/2cc1b41a21ab290545b1d64ede21f85f.png)  
![screenshot](https://polaroids.festival.ml/5c814d7907dbeeab2ee89f3b8e3d10b9.png)  
![screenshot](https://polaroids.festival.ml/cb661e16e10698fe37f09c658d959980.png)![screenshot](https://polaroids.festival.ml/c3e384a3d8a8fd2fe5991b0050d35bc4.png)  

## Download
Just download the ZIP file included in the latest release. https://github.com/festivaldev/UNIcode/releases/latest

## Known Issues
- Performance may get drastically worse when displaying many tiles.
- In 60 out of 100 cases the ALT+ codes provided aren't correct. Haven't figured out yet, how to correctly determine them.
- Due to problems with overwide characters like `U+FDFD` (﷽) the HTML structure used for PDF export crashes, which is why PDF export is disabled at the moment.

## Roadmap
See [UNIcode/Issues](https://github.com/festivaldev/UNIcode/issues).

## Credits
Special Thanks to [@GoldenCrystal](https://github.com/GoldenCrystal) for the UnicodeInformation library (https://github.com/GoldenCrystal/NetUnicodeInfo).
