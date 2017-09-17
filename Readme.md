## Space Engineers Toolbox
###### A toolbox utility for modifying and importing content in the Space Engineers Game.
The primary focus of the Toolbox is to allow importing of images and 3D models, and editing of save game content.

Created by Mid-Space productions. Est. 1993

![](http://i.imgur.com/429uvwe.jpg)

---

The **[Space Engineers Game](http://www.spaceengineersgame.com/)** is Copyright Keen Software House.
For more details on the game, please vist the following links.
* [www.spaceengineersgame.com](http://www.spaceengineersgame.com/)
* [Steam store](http://store.steampowered.com/app/244850/)
* [Keen Software House](http://www.keenswh.com/about.html)
* Space Engineers on [Facebook](https://www.facebook.com/SpaceEngineers/)

---

### Installation

* [Download the latest version](https://github.com/midspace/SEToolbox/releases/latest).
* Details on [Installation/Runtime requirements](https://github.com/midspace/SEToolbox/wiki/System-Requirements).
* We invite you to ask questions at the offical KeenSWH forum, on the [SEToolbox thread](http://forums.keenswh.com/threads/6638984/).

Before asking any question please read the [documentation (FAQ)](https://github.com/midspace/SEToolbox/wiki) first as your question may have already been answered.


### Issues
* Currently documented issues are [listed here](https://github.com/midspace/SEToolbox/wiki/Current-Issues).
* Please submit any new issues here [here](https://github.com/midspace/SEToolbox/issues/new).

### Changes from original version by Midspace
##### **Blueprint projection helpers**
* Change pivot position and orientation
 * selected block will be moved as the first block in the grid's block list (current projector behavior puts the first block at the 0-0-0 position)
 * the pivot will be moved to the selected block (old projector behavior used the pivot at the 0-0-0 position)
 * grid's forward and up will be the selected blocks' forward and up
 * overall the grid's position and orientation in the world will stay the same 
 * Optimize Model feature leaves first block at first position (because projector puts the first block at the projector's 0-0-0 position)
##### **Performance eater finder**
* New columns in the entity list
 * builder name
 * number of assemblers
 * number of refineries/furnaces
 * number of ship tools (drill, welder, grinder)
 * number of power sources (reactor, battery, solar panel)
 * number of thrusters/gyroscopes
 * number of turrets
* New tab showing number of blocks built by each player
 * details window shows block counts in a hierarchical view
* New tab showing all timers
 * timer name, owner and builder
 * grid name and builder
 * repeat interval
 * self-triggering mode (None, "Start" or "Trigger Now")
 * triggered programmable blocks, if any
 * source code of these programmable blocks
* New tab showing all projectors
 * projector name, owner and builder
 * grid name and builder
 * info if the projector is currently active (turned on AND has blueprint loaded)
 * number of blocks loaded
##### **Other**
* added block name to the grid's block list view
* ability to get GPS position of a single block (copy to clipboard)
* main window lists filled asynchronously (GUI is responsive sooner during loading, good for big worlds)

### Contributing
If you are interested in fixing issues and contributing directly to the code base, please see our document on [working with and developing SEToolbox](https://github.com/midspace/SEToolbox/wiki/Working-with-and-developing-SEToolbox).


### License

This application contains code expressly licenced to it, and should not be used in any other application without the permission of Keen Software House.

The Space Engineers logo was sourced from spaceengineerswiki.com under creative commons.

---

SEToolbox was started in 2013 soon after Space Engineers was released for Early Access, and has been worked on in my free time.
There is no requirement for any individual to donate.
[![](https://www.paypalobjects.com/en_AU/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=5V7JL6CDGHCYL)
