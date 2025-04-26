//-----------------------------------------------------------------------------
// Torque Game Engine
// 
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

$baseMods   = "common";
$userMods   = "marble";
$displayHelp = false;


//-----------------------------------------------------------------------------
// Support functions used to manage the mod string

function pushFront(%list, %token, %delim)
{
   if (%list !$= "")
      return %token @ %delim @ %list;
   return %token;
}

function pushBack(%list, %token, %delim)
{
   if (%list !$= "")
      return %list @ %delim @ %token;
   return %token;
}

function popFront(%list, %delim)
{
   return nextToken(%list, unused, %delim);
}

function onFrameAdvance()
{

}

//------------------------------------------------------------------------------
// Process command line arguments

for ($i = 1; $i < $Game::argc ; $i++)
{
   $arg = $Game::argv[$i];
   $nextArg = $Game::argv[$i+1];
   $hasNextArg = $Game::argc - $i > 1;
   $logModeSpecified = false;
   
   switch$ ($arg)
   {
      //--------------------
      case "-log":
         $argUsed[$i]++;
         if ($hasNextArg)
         {
            // Turn on console logging
            if ($nextArg != 0)
            {
               // Dump existing console to logfile first.
               $nextArg += 4;
            }
            setLogMode($nextArg);
            $logModeSpecified = true;
            $argUsed[$i+1]++;
            $i++;
         }
         else
            error("Error: Missing Command Line argument. Usage: -log <Mode: 0,1,2>");

      //--------------------
      case "-mod":
         $argUsed[$i]++;
         if ($hasNextArg)
         {
            // Append the mod to the end of the current list
            $userMods = strreplace($userMods, $nextArg, "");
            $userMods = pushFront($userMods, $nextArg, ";");
            $argUsed[$i+1]++;
            $i++;
         }
         else
            error("Error: Missing Command Line argument. Usage: -mod <mod_name>");
            
      //--------------------
      case "-compileguis":
         $compileGuis = true;
         $argUsed[$i]++;
         echo("Compile guis!");

      case "-compileall":
         $compileGuis = true;
         $compileScripts = true;
         $argUsed[$i]++;
         echo("Compile all!");
   

      case "-game":
         $argUsed[$i]++;
         if ($hasNextArg)
         {
            // Remove all mods, start over with game
            $userMods = $nextArg;
            $argUsed[$i+1]++;
            $i++;
         }
         else
            error("Error: Missing Command Line argument. Usage: -game <game_name>");
            
      //--------------------
      case "-show":
         // A useful shortcut for -mod show
         $userMods = strreplace($userMods, "show", "");
         $userMods = pushFront($userMods, "show", ";");
         $argUsed[$i]++;

      //--------------------
      case "-console":
         enableWinConsole(true);
         $argUsed[$i]++;

      //--------------------
      case "-jSave":
         $argUsed[$i]++;
         if ($hasNextArg)
         {
            echo("Saving event log to journal: " @ $nextArg);
            saveJournal($nextArg);
            $argUsed[$i+1]++;
            $i++;
         }
         else
            error("Error: Missing Command Line argument. Usage: -jSave <journal_name>");

      //--------------------
      case "-jPlay":
         $argUsed[$i]++;
         if ($hasNextArg)
         {
            playJournal($nextArg,false);
            $argUsed[$i+1]++;
            $i++;
         }
         else
            error("Error: Missing Command Line argument. Usage: -jPlay <journal_name>");

      //--------------------
      case "-jDebug":
         $argUsed[$i]++;
         if ($hasNextArg)
         {
            playJournal($nextArg,true);
            $argUsed[$i+1]++;
            $i++;
         }
         else
            error("Error: Missing Command Line argument. Usage: -jDebug <journal_name>");

      //-------------------
      case "-help":
         $displayHelp = true;
         $argUsed[$i]++;
   }
}


//-----------------------------------------------------------------------------
// The displayHelp, onStart, onExit and parseArgs function are overriden
// by mod packages to get hooked into initialization and cleanup. 

function onStart()
{
   // Default startup function
}

function onExit()
{
   // OnExit is called directly from C++ code, whereas onStart is
   // invoked at the end of this file.
}

function parseArgs()
{
   // Here for mod override, the arguments have already
   // been parsed.
}   

package Help {
   function onExit() {
      // Override onExit when displaying help
   }
};

function displayHelp() {
   activatePackage(Help);

      // Notes on logmode: console logging is written to console.log.
      // -log 0 disables console logging.
      // -log 1 appends to existing logfile; it also closes the file
      // (flushing the write buffer) after every write.
      // -log 2 overwrites any existing logfile; it also only closes
      // the logfile when the application shuts down.  (default)

   error(
      "Marble Blast command line options:\n"@
      "  -log <logmode>         Logging behavior; see main.cs comments for details\n"@
      "  -game <game_name>      Reset list of mods to only contain <game_name>\n"@
      "  -mod <mod_name>        Add <mod_name> to list of mods\n"@
      "  -console               Open a separate console\n"@
      "  -show <shape>          Launch the TS show tool\n"@
      "  -jSave  <file_name>    Record a journal\n"@
      "  -jPlay  <file_name>    Play back a journal\n"@
      "  -jDebug <file_name>    Play back a journal and issue an int3 at the end\n"@
      "  -help                  Display this help message\n"
   );
}

//--------------------------------------------------------------------------

// Default to a new logfile each session.
if (!$logModeSpecified) {
   setLogMode(6);
}

// Set the mod path which dictates which directories will be visible
// to the scripts and the resource engine.
$modPath = pushback($userMods, $baseMods, ";");
setModPaths($modPath);

// Get the first mod on the list, which will be the last to be applied... this
// does not modify the list.
nextToken($modPath, currentMod, ";");

// Execute startup scripts for each mod, starting at base and working up
echo("--------- Loading MODS ---------");
function loadMods(%modPath)
{
   %modPath = nextToken(%modPath, token, ";");
   if (%modPath !$= "")
      loadMods(%modPath);

   exec(%token @ "/main.cs");
}
loadMods($modPath);
echo("");

// Parse the command line arguments
echo("--------- Parsing Arguments ---------");
parseArgs();

// Either display the help message or startup the app.
if ($compileGuis)
{
   enableWinConsole(true);
   activatePackage(Help);
   for($file = findFirstFile("*.gui"); $file !$= ""; $file = findNextFile("*.gui"))
   {
      echo($file);
      compile($file);
   }
   if($compileScripts)
   {
      for($file = findFirstFile("*.cs"); $file !$= ""; $file = findNextFile("*.cs"))
      {
         echo($file);
         compile($file);
      }
   }
   quit();
} else if ($displayHelp) {
   enableWinConsole(true);
   displayHelp();
   quit();
}
else {
   onStart();
   echo("Engine initialized...");
}

// Display an error message for unused arguments
for ($i = 1; $i < $Game::argc; $i++)  {
   if (!$argUsed[$i])
      error("Error: Unkown command line argument: " @ $Game::argv[$i]);
}

function GuiMLTextCtrl::onURL(%this, %url)
{
   gotoWebPage( %url );
}   

package TASInfo {
    // Function that is done every frame 
    function onFrameAdvance(%delta) {
        Parent::onFrameAdvance(%delta);

        // Check if the marble exists
        %marbleExists = isObject(ServerConnection) && isObject(ServerConnection.getControlObject()) && ServerConnection.getControlObject();
        if (%marbleExists && $printInfo) {
            // Marble object
            %marble = ServerConnection.getControlObject();
            // Get the physics variables
            %position = %marble.getPosition();
            %velocity = %marble.getVelocity();
            %angularVelocity = %marble.getAngularVelocity();
            // Current time since start
            %time = PlayGui.elapsedTime + PlayGui.bonusTime;
            // Log to console
            echo("V:" SPC %velocity SPC "P:" SPC %position SPC "A:" SPC %angularVelocity SPC "Frame:" SPC $currentFrame SPC "Time:" SPC %time);
        } else {}

        // Frame counter
        %gameRunning = isObject(ServerConnection);
        if (!%gameRunning) {
            $currentFrame = 0;
        } else {
            $currentFrame = $currentFrame + 1;
        }
    }
};
activatePackage(TASInfo);

function togglePrintInfo() {
   %marbleExists = isObject(ServerConnection) && isObject(ServerConnection.getControlObject()) && ServerConnection.getControlObject();
   if (%marbleExists && $ConsoleActive) {
      if ($printInfo == true) {
         $printInfo = false;
         resetConsoleWindow();
         echo("");
      } else {
         $printInfo = true;
         consoleWindowPrintInfo();
      }
   }
   return $printInfo;
}

function help()
{
   echo("\nCall one of the functions below for more information:");
   echo("" SPC "hotKeys();"); 
   echo("" SPC "newPreferences();");
   echo("" SPC "newCommands();"); 
   echo("" SPC "helpfulCommands();");
   echo("" SPC "aboutNewRecs();\n"); 
}

function hotKeys()
{
   echo("\nHotkeys:");
   echo("" SPC "Control + 1 = Toggles FPS overlay");
   echo("" SPC "Control + 2 = Toggles extended timer");
   echo("" SPC "Control + 3 = Round robin time travel display options");
   echo("" SPC "Control + 4 = Round robin input display options");
   echo("" SPC "Control + 5 = Toggles third decimal for high scores");
   echo("" SPC "Control + 6 = Toggles particle visibility");
   echo("" SPC "NumPad0 = Time skip, can be set with $pref::timeskip");
   echo("" SPC "NumPad1 = 5% speed");
   echo("" SPC "NumPad2 = 25% speed");
   echo("" SPC "NumPad3 = 100% speed");
   echo("" SPC "NumPad4 = One second time skip");
   echo("" SPC "NumPad5 = 400% speed");
   echo("" SPC "NumPad6 = Toggles $testcheats");
   echo("" SPC "NumPad7 = Sets resolution to 1920x1080");
   echo("" SPC "NumPad8 = Sets resolution to 1280x720");
   echo("" SPC "NumPad9 = Toggles marble physics in console");
   echo("" SPC "R = Restart by fully reloading the mission*");
   echo("" SPC "Shift + R = Resets the mission at the start (desyncs)**\n");
   echo("" SPC " * Can be changed with setRestartKeybind('VALUE');");
   echo("" SPC "** Can be changed with setRespawnKeybind('VALUE');\n");
}

function newPreferences()
{
   echo("\nNewly introduced preferences, defaults and options:");
   echo("" SPC "$pref::showFPS = 0;                 0 or 1");
   echo("" SPC "$pref::extendedTimer = 1;           0 or 1");
   echo("" SPC "$pref::timeTravelDisplay = 1;       0, 1 or 2");
   echo("" SPC "$pref::inputDisplay = 1;            0, 1 or 2");
   echo("" SPC "$pref::showThousandths = 1;         0 or 1");
   echo("" SPC "$pref::showParticles = 0;           0 or 1*");
   echo("" SPC "$pref::timeskip = 5000;             Value in ms");
   echo("" SPC "$pref::restartKeybind = R;          setRestartKeybind('');");
   echo("" SPC "$pref::respawnKeybind = Shift + R;  setRespawnKeybind('');\n");
   echo("" SPC "*Default recommended as particles crash replay button\n");
}

function newCommands()
{
   echo("\nNewly introduced commands and descriptions:");
   echo("" SPC "playLastRec();          Plays most recently saved rec");
   echo("" SPC "setMaxFPS();            Limits FPS to value*");
   echo("" SPC "setTickInterval();      Sets frame duration to integer"); 
   echo("" SPC "setTimeScale();         Changes the speed during demos");
   echo("" SPC "timeskip();             Skips ahead the value in ms");
   echo("" SPC "setTimeskip();          Sets timeskip pref in ms"); 
   echo("" SPC "fov();                  Sets field of view, saves as pref");
   echo("" SPC "printSpeedrunVersion(); Outputs mod version into console"); 
   echo("" SPC "forceRecDeltas(true);   Syncs using rec frame durations**");
   echo("" SPC "forceRecPhysics(true);  Syncs using physics stored in rec");
   echo("" SPC "convertDemo();          (sourceRec, [force], [timescale])");
   echo("" SPC "convertFolder();        (sourceDir, [force], [timescale])");
   echo("" SPC "convertPath();          (sourcePath, [force], [timescale])");
   echo("" SPC "convertAll();           ([force], [timescale])");
   echo("" SPC "exportDemos();          Copies all new recs to \"Exported\"");
   echo("" SPC "setRestartKeybind(str); Set restart keybind to str value");
   echo("" SPC "setRespawnKeybind(str); Set respawn keybind to str value");
   echo("" SPC "toggleBlackGems();      Turns on/off all gems being black\n");
   echo("" SPC "*  setMaxFPS(0) sets FPS to unlocked");
   echo("" SPC "** Rec will not necessarily play back in real time\n");
}

function helpfulCommands() 
{
   echo("\nHelpful commands in vanilla Marble Blast Gold:");
   echo("" SPC "playDemo(\"marble/client/demos/demo.rec\");");
   echo("" SPC "playnextdemo();");
   echo("" SPC "echo(PlayGui.bonusTime);");
   echo("" SPC "echo(PlayGui.totalBonus);");
   echo("" SPC "$pref::Environmentmaps = 1; (Reflections)\n");
}

function aboutNewRecs()
{
   echo("\nThe greatest achievement through the collaborative effort");
   echo("of many is the creation of a rec format that has");
   echo("cross-platform support while maintaining backwards");
   echo("compatibility. Recs recorded on v2.01 on Windows or v2.02");
   echo("on Mac will be of this new format. Recs from these versions");
   echo("will have a stamp at the beginning showing the version");
   echo("number of Speedrun Edition to confirm.");
   echo("\nIf the rec is a hybrid rec, then the forceRecPhysics(1);");
   echo("command can be entered to force rec syncing via stored");
   echo("physics. If the rec is not a hybrid rec, then");
   echo("forceRecDeltas(1); can assist with rec sync, however, recs");
   echo("must be played back using the same operating system and");
   echo("aspect ratio of the recording and will not necessarily play");
   echo("back in real time.");
   echo("\nIn addition, demo file management has been completely");
   echo("remodeled to better fit the needs of Marble Blast");
   echo("speedrunners. Recs will automatically be recorded upon boot");
   echo("eliminating the need for $doRecordDemo. Each attempt will");
   echo("create a new file to avoid the possibility of personal");
   echo("bests being overwritten. Restarts will automatically delete");
   echo("unfinished runs to avoid clutter. For runs that the player");
   echo("wants saved, there are the following options:");
   echo("" SPC "- Replay will save the recording with the level name");
   echo("  " SPC "and elapsed time in the _Trash folder, then reload");
   echo("  " SPC "the mission to immediately continue attempts.");
   echo("" SPC "- Continue will save the recording with the level name");
   echo("  " SPC "and elapsed time, then exit to level select.");
   echo("" SPC "- If the player exits a level, the player will be");
   echo("  " SPC "prompted if they would like to keep the attempt as a");
   echo("  " SPC "blooper or to delete the recording.\n");
}

function toggleFPS() {
   $pref::showFPS = !$pref::showFPS;
   return betterFPS($pref::showFPS);
}

function toggleExtendedTimer() {
   $pref::extendedTimer = !$pref::extendedTimer;
   return $pref::extendedTimer;
}

function toggletimeTravelDisplay() {
   $pref::timeTravelDisplay++;
   if ($pref::timeTravelDisplay > 2) {
      $pref::timeTravelDisplay = 0;
   }
   return $pref::timeTravelDisplay;
}

function toggleInputDisplay() {
   $pref::inputDisplay++;
   if ($pref::inputDisplay > 2) {
      $pref::inputDisplay = 0;
   }
   return $pref::inputDisplay;
}

function toggleShowThousandths() {
   $pref::showThousandths = !$pref::showThousandths;
   return $pref::showThousandths;
}

function refreshThousandths() {
   %row = PM_MissionList.getRowNumById(PM_MissionList.getSelectedId());
   PM_setSelected(%row);
   reformatGameEndText();
}

function toggleShowParticles() {
   $pref::showParticles = !$pref::showParticles;
   return $pref::showParticles;
}

function toggleCheats() {
   $testcheats = !$testcheats;
   return $testcheats;
}

function restartKeybind() {
   %marbleExists = isObject(ServerConnection) && isObject(ServerConnection.getControlObject()) && ServerConnection.getControlObject();
   if (%marbleExists && $Game::State !$= "End") {
      resumeGame();
      restartLevel();
   }
}

function setRestartKeybind(%this)
{
   moveMap.bind(keyboard, $pref::restartKeybind, "");
   $pref::restartKeybind = %this;
   moveMap.bind(keyboard, $pref::restartKeybind, restartKeybind);
}

function respawnKeybind() {
   %marbleExists = isObject(ServerConnection) && isObject(ServerConnection.getControlObject()) && ServerConnection.getControlObject();
   if (%marbleExists && $Game::State !$= "End") {
      LocalClientConnection.respawnPlayer();
   }
}

function setRespawnKeybind(%this)
{
   moveMap.bind(keyboard, $pref::respawnKeybind, "");
   $pref::respawnKeybind = %this;
   moveMap.bind(keyboard, $pref::respawnKeybind, respawnKeybind);
}

GlobalActionMap.bindCmd(keyboard, "ctrl 1", "toggleFPS();", "");
GlobalActionMap.bindCmd(keyboard, "ctrl 2", "toggleExtendedTimer();", "");
GlobalActionMap.bindCmd(keyboard, "ctrl 3", "toggletimeTravelDisplay();", "");
GlobalActionMap.bindCmd(keyboard, "ctrl 4", "toggleInputDisplay();", "");
GlobalActionMap.bindCmd(keyboard, "ctrl 5", "toggleShowThousandths(); refreshThousandths();", "");
GlobalActionMap.bindCmd(keyboard, "ctrl 6", "toggleShowParticles(); schedule(10,0,restartKeybind);", "");

GlobalActionMap.bindCmd(keyboard, NumPad0, "timeskip($pref::timeskip);", "");
GlobalActionMap.bindCmd(keyboard, NumPad1, "setTimeScale(0.05);", "");
GlobalActionMap.bindCmd(keyboard, NumPad2, "setTimeScale(0.25);", "");
GlobalActionMap.bindCmd(keyboard, NumPad3, "setTimeScale(1);", "");
GlobalActionMap.bindCmd(keyboard, NumPad4, "timeskip(1000);", "");
GlobalActionMap.bindCmd(keyboard, NumPad5, "setTimeScale(4);", "");
GlobalActionMap.bindCmd(keyboard, NumPad6, "toggleCheats();", "");
GlobalActionMap.bindCmd(keyboard, NumPad7, "setResolution(1920,1080);", "");
GlobalActionMap.bindCmd(keyboard, NumPad8, "setResolution(1280,720);", "");
GlobalActionMap.bindCmd(keyboard, NumPad9, "togglePrintInfo();", "");
moveMap.bind(keyboard, $pref::restartKeybind, restartKeybind);
moveMap.bind(keyboard, $pref::respawnKeybind, respawnKeybind);