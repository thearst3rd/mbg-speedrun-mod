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
         consoleWindowDefault();
         echo("");
      } else {
         $printInfo = true;
         consoleWindowPrintInfo();
      }
   }
   return $printInfo;
}

function toggleFPS() {
   if($fpsEnabled) {
      $fpsEnabled = false;
      return metrics();
   } else {
      $fpsEnabled = true;
      return metrics(fps);
   }
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

function restartHotKey() {
   %marbleExists = isObject(ServerConnection) && isObject(ServerConnection.getControlObject()) && ServerConnection.getControlObject();
   if (%marbleExists && $Game::State !$= "End") {
      resumeGame();
      restartLevel();
   }
}

function toggleFrameRateLocker() {
   $pref::enableFrameRateUnlock = !$pref::enableFrameRateUnlock;
   enableFrameRateUnlock($pref::enableFrameRateUnlock);
}

function toggleVerticalSync() {
   $pref::setVerticalSync = !$pref::setVerticalSync;
   setVerticalSync($pref::setVerticalSync);
}

GlobalActionMap.bindCmd(keyboard, "alt 1", "toggleFPS();", "");
GlobalActionMap.bindCmd(keyboard, "alt 2", "toggleExtendedTimer();", "");
GlobalActionMap.bindCmd(keyboard, "alt 3", "toggletimeTravelDisplay();", "");
GlobalActionMap.bindCmd(keyboard, "alt 4", "toggleInputDisplay();", "");
GlobalActionMap.bindCmd(keyboard, "alt 5", "toggleShowThousandths(); refreshThousandths();", "");
GlobalActionMap.bindCmd(keyboard, "alt 6", "toggleShowParticles(); schedule(10,0,restartHotKey);", "");
GlobalActionMap.bindCmd(keyboard, "alt 7", "toggleFrameRateLocker();", "");
GlobalActionMap.bindCmd(keyboard, "alt 8", "toggleVerticalSync();", "");

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
moveMap.bindCmd(keyboard, "R", "restartHotKey();", "");