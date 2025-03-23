//***************************************** ENGINE-SIDE REC CONVERSION *****************************************
function convertDemo(%sourceRec, %forced, %timescale) {
   convert_InitVariables(%forced, %timescale);
   $convert_NumConvertFiles++;
   $convert_File[$convert_FileIndex] = %sourceRec;
   convert_createDestinationPath(%sourceRec);
}

function convertFolder(%source, %forced, %timescale) { 
   convert_InitVariables(%forced, %timescale);
   %sourcePath = filePath(%source);
   convert_BuildRecordingList(%sourcePath, 0);
   convert_InitBulkConversion();
}

function convertPath(%source, %forced, %timescale) { 
   convert_InitVariables(%forced, %timescale);
   %sourcePath = filePath(%source);
   convert_BuildRecordingList(%sourcePath, 1);
   convert_InitBulkConversion();
}

function convertAll(%forced, %timescale) { 
   convert_InitVariables(%forced, %timescale);
   convert_BuildRecordingList("", 2);
   convert_InitBulkConversion();
}

function convert_InitBulkConversion() {
   if ($convert_NumConvertFiles == 0) {
      echo($convert_OutputNoTargets);
      clearConvert();
   } else {
      %sourceRec = $convert_File[$convert_FileIndex];
      convert_createDestinationPath(%sourceRec);
   }
}

function convert_InitVariables(%forced, %timescale) {
   $Game::State = "End";
   deleteVariables("$convert_*");
   $convert_StartTime = getRealTime();
   $convert_FilenamePrefix = "[CVTD] ";
   $convert_OutputInvalidPath = "\nCONVERSION ABORTED: Recording not found.\n";
   $convert_OutputFileExists = "\nCONVERSION ABORTED: Converted recording already exists.\n";
   $convert_OutputNoTargets = "\nCONVERSION ABORTED: No recordings found.\n";
   $convert_OutputDesyncs = "****************** UNFINISHED RECORDINGS *****************\n";
   $convert_OutputSkipped = "******************* SKIPPED RECORDINGS *******************\n";

   $convert_NumConvertFiles = 0;
   $convert_FileIndex = 0;
   $convert_ConversionRunning = 0;
   $convert_DesyncCount = 0;
   $convert_NewFormat = 0;
   $convert_Skipped = 0;
   $convert_SkippedCount = 0;
   
   $convert_ToggleParticles = 0;
   if ($pref::showParticles) {
      $pref::showParticles = !$pref::showParticles;
      $convert_ToggleParticles = 1;
   }
   $convert_Forced = 0;
   if (%forced) {
      $convert_Forced = %forced;
   }
   $convert_Timescale = 10;
   if (%timescale) {
      $convert_Timescale = %timescale;
   }
}

function convert_BuildRecordingList(%sourcePath, %mode)
{
   switch(%mode)
   {
      case 0: // Single folder
         for(%file = findFirstFile(%sourcePath @ "*.rec"); %file !$= ""; %file = findNextFile(%sourcePath @ "*.rec"))
         {
            %checkPath = filePath(%file);
            if (stricmp(%checkPath, %sourcePath) == 0) {
               $convert_File[$convert_NumConvertFiles] = %file;
               $convert_NumConvertFiles++;
            }
         }
      case 1: // Drill directory
         for(%file = findFirstFile(%sourcePath @ "*.rec"); %file !$= ""; %file = findNextFile(%sourcePath @ "*.rec"))
         {
            $convert_File[$convert_NumConvertFiles] = %file;
            $convert_NumConvertFiles++;
         }
      case 2: // Entire MBG tree
         for(%file = findFirstFile("*.rec"); %file !$= ""; %file = findNextFile("*.rec"))
         {
            $convert_File[$convert_NumConvertFiles] = %file;
            $convert_NumConvertFiles++;
         }
   }
}

function convert_createDestinationPath(%sourceRec) {
   //Sets destination path
   %destinationPath = strreplace(%sourceRec, "demos/", "demos/Converted/");
   if (strstr(%sourceRec, "demos") == -1) {
      %destinationPath = "marble/client/demos/Converted/";
   } 

   // Builds directory tree
   %tokenPath = filePath(%destinationPath);
   %buildDirectories = "";
   while(%buildDirectories !$= filePath(%destinationPath))
   {
      %tokenPath = nextToken(%tokenPath, "theToken", "/" );
      if (strlen(%buildDirectories) == 0) {
         %buildDirectories = %theToken;
      } else {
         %buildDirectories = %buildDirectories @ "/" @ %theToken;
      }
      if (strstr(%buildDirectories, "demos") > 0) {
         mkDir(%buildDirectories);
      }
   }
   convert_TargetValidationAndConversion(%sourceRec, %destinationPath);
}

function convert_TargetValidationAndConversion(%sourceRec, %destinationPath) {
   //Set destination rec to be used in the loop
   %convertWritePath = filePath(%destinationPath);
   %newName =  $convert_FilenamePrefix @ fileBase(%sourceRec) @ ".rec";
   %destinationRec = %convertWritePath @ "/" @ %newName;
   $convert_Schedule = schedule(50, 0, "convert_LoopedSchedule", %sourceRec, %destinationRec);

   // Checks if target already exists
   %checkIfTargetExists = getFileSize(%destinationRec);
   if (%checkIfTargetExists > 0 && !$convert_Forced) {
      echo($convert_OutputFileExists);
      $convert_Skipped = 1;
      $convert_SkippedCount++;
      $convert_OutputSkipped = $convert_OutputSkipped @ %sourceRec @ " (Converted)\n";  
      return;
   }

   // Checks if source is playable before attempting to convert
   playdemo(%sourceRec);
   if ($playingDemo) {
      setTimeScale($convert_Timescale);
      doConvertDemo(%convertWritePath, %newName, $Server::MissionFile);
   } else {
      echo($convert_OutputInvalidPath);
   }
}

function convert_LoopedSchedule(%sourceRec, %destinationRec) {
   if (!$playingDemo) {
      //Validations, must come prior to incrementing to next index
      if (strlen($Game::State) > 0 && $Game::State !$= "End" && !$convert_Skipped && !$convert_Forced) {
         $Game::State = "End";
         $convert_DesyncCount++;
         $convert_OutputDesyncs = $convert_OutputDesyncs @ %sourceRec @ "\n";  
         convert_DeleteUnfinishedRecording(%destinationRec);
         convert_LoopedSchedule(%sourceRec, %destinationRec);
         return;
      }
      if ($convert_NewFormat && !$convert_Forced) {
         $convert_SkippedCount++;
         $convert_OutputSkipped = $convert_OutputSkipped @ %sourceRec @ " (New Format)\n";  
      }
      $convert_FileIndex++;

      // End process if all sources have been converted
      if ($convert_FileIndex == $convert_NumConvertFiles) {
         clearConvert();
         return;
      }

      //Convert next demo if fail all other checks
      %sourceRec = $convert_File[$convert_FileIndex];
      $convert_Skipped = 0;
      $convert_NewFormat = 0;
      convert_createDestinationPath(%sourceRec);
   } else {
      $convert_Schedule = schedule(50, 0, "convert_LoopedSchedule", %sourceRec, %destinationRec);
   }
}

function convert_DeleteUnfinishedRecording(%destinationRec) {
   %deletionFile = %destinationRec;
   if (strlen(%deletionFile == 0)) {
      %sourceRec = $convert_File[$convert_FileIndex];
      %deletionFile = filePath(strreplace(%sourceRec, "demos/", "demos/Converted/")) @ "/" @ $convert_FilenamePrefix @ fileBase(%sourceRec) @ ".rec";
   }
   if (getFileSize(%deletionFile) > 0) {
      %deleteFile = schedule(50, 0, "removeFile", %deletionFile);
   } else {
      convert_DeleteUnfinishedRecording(%deletionFile);
   }
}

function convert_getDuration(%start) {
   %duration = mAbs(mfloor((getRealTime() - %start) / 1000));
   %hours = mfloor(%duration /3600);
   %mins = mfloor((%duration - mfloor(%duration/3600) * 3600)/60);
   %secs = mfloor((%duration - mfloor(%duration/60) * 60));
   %durationOutput = "";
   if (%hours)
      %durationOutput = %hours @ "h"; 
   if (%mins)
      %durationOutput = %durationOutput SPC %mins @ "m";
   %durationOutput = %durationOutput SPC %secs @ "s";
   return %durationOutput;
}

function convert_CreateReport(%f, %stats, %output) {
   %fpath = "marble/client/demos/Converted/_Reports/";
   %fname = "fullReport (";
   mkdir(%fpath);

   %inc = 1;
   while(getFileSize(%fpath @ %fname @ %inc @ ").txt") > 0){
      %inc++;
   }
   %fullPath = %fpath @ %fname @ %inc @ ").txt";
   %f.openForWrite(%fullPath);
   %f.writeLine(%stats @ "\n" @ %output);
   %f.close();
   %f.delete();
   echo("See your full report at:\n" @ %fullPath @ "\n");
}

function clearConvert() {
   // Output
   echo("\n******************* FINISHED CONVERTING ******************\n");
   %duration = convert_getDuration($convert_StartTime);
   %stats = "Conversion Duration:" SPC %duration @ "\n";
   %omitted = $convert_DesyncCount + $convert_SkippedCount;
   %stats = %stats @ "Number of Conversions:" SPC ($convert_FileIndex - %omitted) @ "\n";
   %stats = %stats @ "Unfinished Recordings:" SPC $convert_DesyncCount @ "\n";
   %stats = %stats @ "Skipped Recordings:" SPC $convert_SkippedCount @ "\n";
   echo(%stats);
   if ($convert_DesyncCount || $convert_SkippedCount) {
      %output = "";
      if ($convert_DesyncCount || $convert_SkippedCount) {
         if ($convert_DesyncCount)
            %output = $convert_OutputDesyncs;
         if ($convert_SkippedCount) {
            if (strlen(%output) > 0)
               %output = %output @ "\n";
            %output = %output @ $convert_OutputSkipped;
         }
         %report = new FileObject();
         convert_CreateReport(%report, %stats, %output);
      }
   }
   echo("**********************************************************\n");

   // Clean-up
   cancel($convert_Schedule);
   if ($convert_ToggleParticles) {
      $pref::showParticles = !$pref::showParticles;
   }
   deleteVariables("$convert_*");
}