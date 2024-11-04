LAST UPDATED: May 3, 2024

=============================================================================================================================
INSTALLATION
=============================================================================================================================
Download the correct version of the mod based on your operating system. To keep things simple and consistent, the unzipped 
package is a ready to play version of MBG Speedrun Edition created by replacing only modified files into a vanilla copy of 
the game. The parent folder should be located outside of the downloads folder (i.e. Documents or Desktop). Mac users may need
to move the app itself into the Applications folder on later versions.  

There may be an error that occurs on Windows that can be rectified by downloading the appropriate Visual C++ Distributable 
here: https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170.

If you would like to keep your preferences, you will need the do the following:
	1. Delete ~/marble/client/pref.cs and ~/marble/client/config.cs and their dso files from the modded version. 
	   (Note: On Mac you will need to right click the MBG application and "Show Packages.")
	2. Copy and paste the files from the same location of your current MBG version back into the modded version. 
	3. On boot, preferences not previously used should be filled in by default.cs and the dso files rebuilt.

All data files (skins, missions, etc.) will need to be transferred as well. 

Additionally, we highly recommend bookmarking these helpful links for rec analysis:
	- https://higuy.me/rect/
	- https://higuy.me/rect/rec_fps.html


=============================================================================================================================
In-Game Modifications
=============================================================================================================================
There are various new tools that are available in this package. A mostly comprehensive list with short descriptions is below:

- Color-coded echo console output of grabs, trigger entries and other useful time markers
- Start analysis of inputs at the start
- Third decimal extended timer and high scores
- Time travel timer with variable decimal places
- Input display
- Custom FOV support
- Time scale and time skip functions
- Velocity, position and acceleration output printed each frame when toggled
- Particle display toggle to prevent crashing
- Vertically dynamic console window sizing
- Max FPS function as an alternative to third party frame rate lockers
- Hybrid cross-platform and backwards compatible rec format with improved file management
- Various hotkeys with restart and respawn player bindable by player preference

A help() function was added to allow users to read about these changes in-game.


=============================================================================================================================
Hotkeys
=============================================================================================================================
These can be found at the bottom of main.cs of the Marble Blast Gold folder and can be modified at the user's discretion.

control + 1 = Toggles FPS overlay
control + 2 = Toggles extended timer
control + 3 = Toggles time travel display; round robin between no display, one or three decimal places
control + 4 = Toggles input display; round robin between nothing, display during playback, or both playback and gameplay
control + 5 = Toggles third decimal for high scores
control + 6 = Toggles particle Toggles particle visibility

NumPad0 = Time skip, default is 5000 ms but can be set with $pref::timeskip
NumPad1 = 5% speed
NumPad2 = 25% speed
NumPad3 = 100% speed
NumPad4 = 1 second time skip
NumPad5 = 400% speed
NumPad6 = Toggles cheats
NumPad7 = Sets resolution to 1920x1080
NumPad8 = Sets resolution to 1280x720
NumPad9 = Toggles printing marble physics each frame in the console

R = Restarts by fully reloading the mission, can be changed with setRestartKeybind('VALUE');
Shift + R = Resets the mission at the start (desyncs), can be changed with setRespawnKeybind('VALUE');


=============================================================================================================================
New Preferences
=============================================================================================================================
$pref::showFPS = 0;			Preference to show FPS in bottom right corner
$pref::extendedTimer = 1;		Preference to show third decimal.
$pref::timeTravelDisplay = 1;		Preference to show time travel timer, 1 = .000 and 2 = .0
$pref::inputDisplay = 1;		Preference to show input display, 1 = demo only and 2 = both demo and gameplay
$pref::showThousandths = 1;		Preference to show third decimal for high scores
$pref::showParticles = 0;		Preference to show particles, recommended to be off to prevent crashes
$pref::timeskip = 5000;			Preference to set timeskip duration, time is in milliseconds
$pref::restartKeybind = R;		setRestartKeybind('VALUE');
$pref::respawnKeybind = Shift R;	setRespawnKeybind('VALUE');


=============================================================================================================================
New Commands
=============================================================================================================================
playLastRec();          	Plays most recently saved rec
setMaxFPS();            	Limits FPS to value, RTSS replacement, value of 0 resets to unlocked FPS
setTickInterval();		Sets frame duration to integer
setTimeScale();			Changes the speed of the physics during demo playback only
timeskip();			Skips ahead the value in milliseconds
setTimeskip();          	Sets timeskip pref in ms
fov();                  	Sets field of view and saves as a pref
printSpeedrunVersion(); 	Outputs mod version into console
videoRecordDemo();		Assists with recording single recs, function arguments are (path, scale, fireworksPeriod)
forceRecDeltas(true);		Forces engine to sync to rec frame durations during demos, playback not in real time
forceRecPhysics(true);		Corrects marble physics using physics stored in rec if marble desyncs
setRestartKeybind('');		Changes restart keybind to value
setRespawnKeybind('');		Changes respawn keybind to value
toggleBlackGems();      	Turns on/off all gems being black, resets on reboot


=============================================================================================================================
Other Useful Commands
=============================================================================================================================
playDemo("marble/client/demos/demo.rec");
playnextdemo();
echo(PlayGui.bonusTime);
echo(PlayGui.totalBonus);
$pref::Environmentmaps = 1;


=============================================================================================================================
About New Recs
=============================================================================================================================
The greatest achievement through the collaborative effort of many is the creation of a rec format that has cross-platform
support while maintaining backwards compatibility. Recs recorded on v2.01 on Windows or v2.02 on Mac will be of this new 
format. Recs from these versions will have a stamp at the beginning showing the version number of Speedrun Edition to 
confirm. 

If the rec is a hybrid rec, then the forceRecPhysics(1); command can be entered to force rec syncing via stored physics. If 
the rec is not a hybrid rec, then forceRecDeltas(1); can assist with rec sync, however, recs must be played back using the 
same operating system and aspect ratio of the recording and will not necessarily play back in real time. 

In addition, demo file management has been completely remodeled to better fit the needs of Marble Blast speedrunners. Recs
will automatically be recorded upon boot eliminating the need for $doRecordDemo. Each attempt will create a new file to avoid 
the possibility of personal bests being overwritten. Restarts will automatically delete unfinished runs to avoid clutter. For
runs that the player wants saved, there are the following options:
	- Replay will save the recording with the level name and elapsed time in a _Trash folder, then will reload the 
	  mission to immediately continue attempts.
	- Continue will save the recording with the level name and elapsed time, then exit to level select.
	- If the player exits a level, the player will be prompted if they would like to keep the attempt as a blooper or to 
	  delete the recording.


=============================================================================================================================
recverify (Windows Only)
=============================================================================================================================
This tool speeds up .rec playback and provides statistics of your rec. This executable must reside in the Marble Blast Gold 
parent folder. Optimal use of this executable by HiGuy is to right a rec, choose open with another app, and set recverify as 
the application to open the recs with. Make sure to select "Always use this app to open .rec files" to make this change 
permanent. If for some reason this application crashes, check to see if there was a directory created in and delete it.

There are also two versions of this application: recverify.exe and recverifyFAST.exe. By default, recverify will provide an
output from command prompt with run statistics that will need to be closed. If you simply want a quick playback solution, 
then the FAST version will be your preferred choice.


=============================================================================================================================
Creating Triggers
=============================================================================================================================
Triggers.cs was modified to allow the user to be able to get echo commands in console upon entering a check-point made in
editor. They are useful for getting feedback on levels lacking in grabs that can help a player/TASer reference if any
improvements can be made. 

To fully utilize these triggers, you will need to modify a mission. The easiest way to do that is to go to
"C:\Program Files (x86)\Marble Blast Gold\marble\data\missions", copy the mission you want, and paste it into the "custom" 
folder with something like mission_MOD. As you are TASing, you will then need to make sure to reference 
marble/data/missions/custom/mission_MOD.mis for your first line of code, and be sure to name it back to the correct
extension when done TASing. 

You will then need to physically create the trigger in this modded mission. The steps are as follows:
	1. Go to your modded mission and type $testcheats = 1; (or F7 if using modded main.cs)
	2. Press F11 to open up the editor. 
	3. At the top, select Window -> World Editor Creator F4
	4. At the bottom window to the right, select Mission Objects -> Mission -> Trigger
	5. The Object Name can be whatever you want without spaces. Change the Data Block to
	   "FrameTimeTrigger"
	6. At the top, select Window -> World Editor Inspector F3
	7. Select your newly created Trigger and next to Dynamic Fields click Add
	8. Name it displayName, and then give it a numerical value. 
	9. You will see a new window created called displayName and your numerical value to 
	   the right. Change the value to whatever meaningful name you want to call it.
	10. Now just move the trigger exactly where you want it, and save the mission!


=============================================================================================================================
https://higuy.me/rect/
=============================================================================================================================
Scripting TAS tool created by Higuy. The webpage contains some notes on what each bit of code does. A quick breakdown of a 
simple script is also below for reference. 

{
	"marble/data/missions/beginner/powerjump.mis"	// File extension of mission, can by most easily found by importing 
	{						// runs for that mission into higuy.me/rect
	"Load Buffer & Camera Calibration"
		frames 14 //------------------------------ Loading frames, usually between 14-21 frames. Defaults to 1ms
		moveframe 1 ms //------------------------- Platform loading frame, cannot turn camera
		{
		   camera (0 0 0)			// camera (x, y, does nothing)
		   move (0 0 0)				// move (x, y, does nothing)
		   triggers (0 0 0 0 0 0)		// triggers (powerup, nothing useful, jump, nothing, nothing)
		}
		{} // ------------------------------------ Leave this with nothing and it will continue the action for
		moveframe 1 ms				// however many frames you specify.
		{
		   camera (-0.784 0 0)
		   move (1 1 0)
		   triggers (0 0 0 0 0 0)
		}
		{
		   camera (0 0 0)			// Put in stuff in this section and it will continue doing that
		   move (1 1 0)				// action next frame onward. So above we snapped the camera left, 
		   triggers (0 0 0 0 0 0)		// but after that we will not. If we left this area blank, we'd be 
		}					// snapping 90 degrees every frame. TLDR for snaps, you'll want to 
	}						// reset the camera to 0 here, but for gradual turns leave it empty.
	{
        "Jump (-.043), then Natural Bounce (1.034) & Jump to the Finish (this is how we create sections for organization)"
		frames 3455 1 ms 
		moveframe 1 ms // ------------------------  Oh yeah, these are moveframes btw. We use these to move. 
		{
		   camera (0 0 0)
		   move (1 1 0)
		   triggers (0 0 1 0 0 0)
		}
		{}
		frames 1076 1 ms // ---------------------- And these are any old frames. We can specify their duration with
        	moveframe 1 ms //------------------------- either "frames [count] [duration] ms" or "frame [duration] ms". 
         	{					// moveframes are always "moveframe [duration] ms". This moveframe
            	   camera (0 0 0)			// in particular is special because it shows what a frame perfect
            	   move (0 0 0)				// natural bounce looks like. Note how for this frame we specify no
            	   triggers (0 0 0 0 0 0)		// controls, but fill in the second set of brackets with movements
        	}					// and a jump to guarantee we will be holding WD ASAP and jumping.
        	{
            	   camera (0 0 0)
            	   move (1 1 0)
            	   triggers (0 0 1 0 0 0)
        	}
        	frames 10000 1 ms //---------------------- You're going to want to add a bunch of frames at the end of each 
    	}						// script or it'll just end abruptly and you'll be sad.
}
  
That's really all there is to it. Combinations of frames and moveframes where you figure out how fast to turn the camera
and when to jump. Feel free to copy and paste from the first { to the bottom } into the tool to see what it looks like.


=============================================================================================================================
CHANGE LOG
=============================================================================================================================
v2.03
BUG FIXES
- FPS display did not show correct fps during rec playback if playback device was less powerful than rec creator's device


V2.02
NEW ADDITIONS/CHANGES
- Mac compatibility of new rec format
- videoRecordDemo() created to assist with single rec video recording
- Console font size platform specific and made larger for Mac users

BUG FIXES
- Console window width no longer fixed to default if adjusted
- "Level 0" no longer displayed entering level select if demo is played before clicking play for the first time

REMOVALS
- bulkRecord() removed due to fireworks particles causing crashes and requiring engine side fixes


V2.01
NEW ADDITIONS
- New backwards compatible hybrid rec format
	- Version, date timestamp, fps, resolution and FOV of the time of recording displayed at the start
	- Physics stored to assist with desyncs during playback
	- Update only affects recs recorded from this version onward
	- New recs can play on vanilla MBG and vice versa
- forceRecPhysics(); added to allow users to force playback to sync using physics stored in recs
- fov() and setTimeskip() functions added to more easily modify these prefs
- toggleBlackGems() added to more intuitively modify the global variable

CHANGES
- realtimeOverride() changed to forceRecDelta(); to more accurately describe the function

BUG FIXES
- Fixed bug where fps overlay was not updating if timescaling during demo playback, accuracy varies on degree of scaling


V1.01
NEW ADDITIONS
- Start analysis for the first two seconds of the run, output in console
- $blackGems = 1; to force all gems to be black
- New FPS UI with improved accuracy and toggleable via a pref and hotkey
- New FPS UI shows the framerate of the demo during playback
- setMaxFPS() created to act as built in FPS limiter to replace RTSS
	- Handles 30-1000 FPS with precision up to 3 decimals
- realtimeOverride(true); created to help limit FPS desyncs
	- Forces engine to fully calculate each frame of a rec
	- Results in recs not playing back in real time
- bulkRecord() and stopBulkRecord() added to assist compilation creators

REMOVALS
- Hotkeys and preferences related to locking FPS removed
- Unlocker can no longer be disabled, setMaxFPS(64); is the equivalent
- Timeskip and timescale moved to demo playing only


V0.02
NEW ADDITIONS
- Color-coded echo console output:
	a. Gems and power-ups
	b. Entering moving platform triggers
	c. Frame landed on pad after go
	d. Finishing a level
	e. "Not enough gems" at finish pad
	e. Entering custom checkpoints made in level editor
- Extended timer with third decimal point
- Time travel timer with one to three decimal places
- Third decimal high scores and completion times
- Input display
- Custom FOV support
- Time scale and time skip functions
- Velocity, position and acceleration output
- Particle toggle to prevent crashing
- Vertically dynamic console window sizing
- Revamped demo handling
	a. New rec created each attempt
	b. Completely new file handling structure that is non-destructive
		i. Restart deletes previous attempt
		ii. "Replay" moves rec into auto-generated _Trash folder
		iii. "Continue" saves rec in demos folder
		iv. Exiting during attempt prompts user to save or delete blooper
	c. Disabled demo auto-play at main menu
	d. Disabled $doRecordDemo, recording is always active
	e. Fully restart level on replay buttons to prevent desyncs
	f. Exiting to main menu or from demo saves place in level select
- Various hotkeys, restart and respawn player bindable
- New preferences and commands to toggle most things back to vanilla
- help(); function to learn everything about the mod in-game

KNOWN LIMITATIONS
- If frame rate unlocker is disabled, timeskip and timescale are also disabled
- The new rec system cannot be reverted to vanilla at this time
- The replay button will cause a crash if particles are enabled