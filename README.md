# NekoPuppet

## Related Repositories

### EmoteEngineNet

https://github.com/Nielk1/EmoteEngineNet

Managed wrapper for the [E-Mote engine](http://emote.mtwo.co.jp/).

### FunctionalNodeGraphWPF

https://github.com/Nielk1/FunctionalNodeGraphWPF

WPF Node Graph modified to execute basic functional flowcharts.

### DotNetZip.ZipReducedKeyDecrypt

https://github.com/Nielk1/DotNetZip.ZipReducedKeyDecrypt

Modified version of DotNetZip's PKZip decompression library for the purpose of using encryption keys rather than the password that generates them.  This is used for the "Mont daughter - Rem" character loader to read game assets directly.

## Required Additional Source

### SharpDX

The version in Nuget is not sufficent.  Please download directly from their website: http://sharpdx.org/

## Library Loading

Currently this program is hard-coded to use NEKOPARA Vol.0's emotedriver.dll, though other drivers will be supported in the future.
Copy the emotedriver.dll file from NEKOPARA Vol.0 to {NekoPuppetDir}\engines\neko0\emotedriver.dll

## Asset Loading

### NEKOPARA Vol.0

To utilize these files copy emotewin.xp3 to {NekoPuppetDir}\assets\NekoparaVol0

### NEKOPARA Vol.1

To utilize these files copy emotewin.xp3 to {NekoPuppetDir}\assets\NekoparaVol1

### NEKOPARA Vol.2

To utilize these files copy emotewin.xp3 to {NekoPuppetDir}\assets\NekoparaVol2

### Go! Go! Nippon! ~My First Trip to Japan~

To utilize these files copy resources.assets to {NekoPuppetDir}\assets\GoGoNippon

### Mont daughter - Rem

To access these files, you must first extract them from the game on Android after downloading the assets through play.
Copy files from \data\data\jp.furyu.moefan\ on an android device to {NekoPuppetDir}\assets\jp.furyu.moefan\, you may need to root your Android device to access these files.
The specific paths needed are \jp.furyu.moefan\files\DAT\Resources\Monster and \jp.furyu.moefan\files\DAT\Resources\NPC

### Uptown Boys

To access these files, you must first extract them from the game on Android after downloading the assets through play.
Copy files from \data\data\jp.co.eitarosoft.yd\ on an android device to {NekoPuppetDir}\assets\jp.co.eitarosoft.yd\, you may need to root your Android device to access these files.
The specific path needed is \jp.co.eitarosoft.yd\files\fc\resources\480x854