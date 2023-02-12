# RPPlus

This plugin is a enhanced version of the original RPPlus plugin.

## Installation

- Stop your server
- Put the [DLL](https://github.com/Antoniofo/RPPlus/releases) in the Plugin folder of your server.
- Start the server

## Inforamtion

The hacking system has changed. When you hack, every second will count in the final take. 
The hack can have a duration defined by the server admin inside the plugin's config. If you go to far from the hack location, the hack will stop but you will still gain money.
It will take a random number between the minMoney and maxMoney (can be chnaged) a will multiplie by the number of second you were hacking.
So for example if the hack is 60 second long and you take 30 beacause you went too far and the minMoney is 500 and maxMoney is 800, your gain will be 30*558 = 16'740€ if the random number is 558.
Note that when a hack is starting like the original plugin all LawEnforcement Biz will be alerted.

## Commands

- /pos            Show your position in the chat and the console
- /tp <x> <y> <z> Telepot you to the coordinate you've entered
- /bcr            Give you the BCR or remove if you have it
- /tpmenu	  Open a menu with all area that you can tp to
- /admintoolsreload Reload all the config from the file/Add new areas if your config is missing one

## Credit

Originally developed by [TeamNova](https://github.com/TeamNovaFR)
