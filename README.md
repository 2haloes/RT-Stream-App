# RT-Stream-App
A crossplatform program to play Rooster Teeth videos on low power devices

![Image of the RT stream app](https://sites.2haloes.co.uk/images/rt_stream_app.PNG)

## Minimum requirements 
OS: Windows, OSX, Linux (no dotnet core required) or any OS that can install dotnet core

Screen Resolution: 640x480

Other requirements: 

Video player that can play m3u8 files (If you don't know what this means, use VLC Media Player)
                    
Internet connection


## Features
* Play videos from any Rooster Teeth based channel (ScrewAttack, JT Music ect.)
* Works with most media players (Windows Media Player does **not** work)
* Light and dark themes with custom theme support
* Supports subtitles (tested with VLC)
* Quality selector
* Log in and play videos for FIRST members (RT FIRST membership required)
* Save login details or only use them for the current session

## Why make this?

I am a very big fan of Rooster Teeth, however, my laptop just isn't powerful enough to run videos from the RT site properly. Then I found that everything worked great in VLC (even in HD which my laptop struggled with badly) so I decided to make a script that could pull the data and automatically open VLC.

From that start, I have created, improved and built upon the idea of a program that makes it easier to view Rooster Teeth's content, making it one of my main developed programs.


# Credits
* JSON.NET for JSON handling
* NetCore.Encrypt for making encryption much easier
* Prisem.Core for making MVVM code easier to create
* AvaloniaUI for making a dotnet core program a possible option
* Youtube-DL for being an amazing public domain project, if it wasn't for this, I likely wouldn't have gotten anywhere
