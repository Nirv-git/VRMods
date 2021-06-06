
This is a collection of a few simple mods I have made. 
They all require [Melon Loader](https://melonwiki.xyz/#/README?id=installation-on-il2cpp-games) and [UIExpansionKit](https://github.com/knah/VRCMods/)

**USE IT AT YOUR OWN RISK. Modding the client is against VRChat's ToS. I am not responsible for any bans or any punishments you may get by using these mods**

**This was made in my free time, and is provided AS IS without warranty of any kind, express or implied. Any use is at your own risk and you accept all responsibility**


# VRC-CameraResChanger
Another simple mod that does just one thing! This uses [UIExpansionKit](https://github.com/knah/VCRCMods/) to add a few buttons to your camera menu, allowing you to set the VRC camera resolution to, Default (1920x1080), 4K (3840x2160), 6k (5760x3240), 8k (7680x4320).
![image](https://user-images.githubusercontent.com/4786654/86955451-370c8080-c11d-11ea-8038-4b39c7c10979.png)

Couple notes: 
* Larger resolutions will cause your game to hang for a few seconds while it renders the image, I have not had VRC crash outright though, but in one instance an 8k photo took 30 seconds.   
* Thanks to [Lag Free Screenshots](https://github.com/knah/VRCMods#lag-free-screenshots) I was silly and added a 16k option if that mod is also installed.
  *  Note, make sure you have a good amount of RAM free. A friend of mine recently mentioned that he was having issues with this and I checked and a 16k photo spikes RAM usage by 6GB  
  * __Resolutions greater than 8k may randomly break with new VRC versions__
* This will not work with Virtual Lens, that camera has a lot of hard coded resolution values in it's shaders. Using anything but the default resolution will just result in a picture of you. **VRCLens** will work with the 4k option though (if you set the camera prefab to use it when you add it to your avatar)

I would also recommend [CameraMinus](https://github.com/knah/VRCMods) for some other camera QoL. 

