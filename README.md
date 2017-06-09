﻿# Ori DE: Randomizer Tracker

## Description
This is a tracker for Ori DE. This tracker is intended for use in
All Trees NMG runs with the Ori DE Randomizer (https://github.com/sigmasin/OriDERandomizer).
Currently, it features the ability to auto update skill, event, and tree pickups.

## Prerequisites
+ .NET Framework 4.5 (https://www.microsoft.com/en-us/download/details.aspx?id=40779)

## Help
For any questions or issues please post here:
https://github.com/david-c-miller/OriDERandomizerTracker/issues

In order to drag the window around right click and select 'Move'. This will disable
the ability to change any of the items on the tracker until you disable it. To start
the auto updating mode, right click and select 'Auto Update'. This will override any
manual changes with the values from the game. Currently, if you start auto tracking after
you've started a run, it is impossible to track the trees correctly. Also, you can unselect the
'Always on Top' option in the right click context menu to stop the tracker form always being the 
top most form.

There are some known issues with the tracker using a lot of CPU time. I recommend setting the
affinity to only 1 or 2 CPU cores to help alleviate this negatively affecting performance in Ori.

## Thanks
Thanks to SigmaSin for creating the Ori DE Randomizer! Additionally, thanks to DevilSquirrel
for help with setting up the auto updating component and for writing the memory related
classes. Also, thanks to ViresMajores for designing all the visual components of the tracker.
