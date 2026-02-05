[![Support me on Ko-fi](https://img.shields.io/badge/Ko--fi-Support%20Me-ff5e5b?style=flat-square&logo=ko-fi&logoColor=white)](https://ko-fi.com/grimbakor)
# MODxHUNTER
Mod for HUNTERxHUNTER NENxIMPACT on the PC.

# How To Install
1. Go to the releases page [here](https://github.com/grimkor/MODxHUNTER/releases/latest) and download the MODxHUNTER.zip file. Extract the contents of the file to the game's install directory.
2. Run the game, it will take a short time during the first launch to unpack some of the game files; be patient!
3. ???
4. Profit.

# Features
## Collision Box Viewer
Found in the F1 menu and only toggle-able inside a match. A feature every FG needs.
## Green Screen background
Found in the F1 menu and only toggle-able inside a match. Can change the colour of the screen and was mostly added for people to get clean screenshots for the wiki.
## Post-Processing Effects
Found in the F1 menu. Change the game's post-processing that's controlled within Unity. Has the following features and I recommend changing the diffusion filter down to ~0.30 and trying the sub-pixel antialiasing.
  - Change Diffusion filter (remove the vaseline from the camera)
  - Enable/Disable bloom
  - Change antialiasing type
## Character Texture Modding**
Found in the F1 menu. Allows modifying the textures of characters in the game, loads the textures directory when the game launches and caches them for overriding when matches load. There will be a small stutter but I've minimised it sufficiently.
  - Texture Export feature, exports current characters costume colour for every character. When using this please avoid using the same character on both teams.
  - "Reload From Disk" feature to live reload changes in textures, handy for those working on a new skin.

Textures are unique for each Costume-Colour and are exported in the layout they want to be imported in. There are a few textures that overlap between costume-colours such as Gon costume 1 & 2 which share the same "Base" textures. I don't have an exhaustive list of each case but pay attention to what is exported from the export tool and make sure the directory structure is the same in the "textures" directory.

The in-game window for this will tell you where the location of the export & textures directories are, they are located in `<Game Directory>/BepInEx/plugins/[textures|export]/<Character Name>/<Costume ID>/<Colour ID>/`
