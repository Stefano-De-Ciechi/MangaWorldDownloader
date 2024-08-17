# About:
A set of applications written in C# + .NET Core to scrape informations and download images from the site "MangaWorld"

It can be used as a cli tool or with a simple gui made in Python + DearPyGui

# Installation and Usage:

First, clone the repository
```
git clone <url of this repository>
cd MangaWorldDownloader
```

Second, cd in both mangaDownloader and mangaScraper directories and build the projects (all additional depencencies should get downloaded automatically)
```
dotnet build
```

Finally, cd in mangaWorld.gui and install the python dependencies
```
pip install dearpygui
```

then you can execute the gui with python3 mangaWorldGui.py
