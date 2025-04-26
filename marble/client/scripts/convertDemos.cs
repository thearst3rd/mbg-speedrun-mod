//********************************************** SCRIPT-SIDE REC CONVERSION **********************************************
// Single File
function convertDemo(%sourceRec, %forced, %timescale) {
   convert_InitVariables(%forced, %timescale);
   $convert_NumConvertFiles++;
   $convert_File[$convert_FileIndex] = %sourceRec;
   convert_TargetValidationAndConversion(%sourceRec);
}

// Entire folder
function convertFolder(%source, %forced, %timescale) { 
   convert_InitVariables(%forced, %timescale);
   convert_InitBulkRecording(filePath(%source), 1);
}

// Folder and its sub-folders
function convertPath(%source, %forced, %timescale) { 
   convert_InitVariables(%forced, %timescale);
   convert_InitBulkRecording(filePath(%source), 0);
}

// All .rec files in the entire MBG tree
function convertAll(%forced, %timescale) { 
   convert_InitVariables(%forced, %timescale);
   convert_InitBulkRecording("", 0);
}

// INITIALIZATION ********************************************************************************************************
function convert_InitVariables(%forced, %timescale) {
   ToggleConsole(1);
   deleteVariables("$convert_*");
   $Game::State = "End";

   // Output
   $convert_FilenamePrefix = "[CVTD] ";
   $convert_OutputFinished = "******************* FINISHED RECORDINGS ******************\n";
   $convert_FinishCount = 0;
   $convert_OutputDesyncs  = "****************** UNFINISHED RECORDINGS *****************\n";
   $convert_DesyncCount = 0;
   $convert_OutputSkipped  = "******************** SKIPPED RECORDINGS ******************\n";
   $convert_SkippedCount = 0;
   
   // Static once set
   $convert_writeDir = "marble/client/demos/Converted/.tmp";
   $convert_StartTime = getRealTime();
   $convert_NumConvertFiles = 0;
   $convert_Forced = 0;
   if (%forced) {
      $convert_Forced = %forced;
   }
   $convert_Timescale = 10;
   if (%timescale) {
      $convert_Timescale = %timescale;
   }

   // Increments/Flags
   $convert_FileIndex = 0;
   $convert_Skipped = 0;

   // Pref storage
   $convert_ToggleParticles = $pref::showParticles;
   $pref::showParticles = 0;
}

// Creates array of recs that matches the rule, relies on resource manager
function convert_InitBulkRecording(%sourcePath, %folderOnly)
{
   %basePath = %sourcePath @ "*.rec";
   for(%file = findFirstFile(%basePath); %file !$= ""; %file = findNextFile(%basePath)) {
      %canConvert = strstr(%file, "demos/Converted") == -1 && strstr(%file, "demos/Exported") == -1;
      if (%canConvert) {
         $convert_File[$convert_NumConvertFiles] = %file;
         $convert_NumConvertFiles++;
      }

      %inSourceFolder = stricmp(filePath(%file), %sourcePath) == 0;
      if (%folderOnly && !%inSourceFolder)
         $convert_NumConvertFiles--;
   }

   // If matches found, begin validation and conversion; else, end the operation
   if ($convert_NumConvertFiles == 0) {
      echo("\n\c6*** CONVERSION HALTED ***\n\c6No demos were found.");
      ToggleConsole(1);
      clearConvert();
   } else {
      %sourceRec = $convert_File[$convert_FileIndex];
      convert_TargetValidationAndConversion(%sourceRec);
   }
}

// SCHEDULED VALIDATIONS AND CHECKS **************************************************************************************
function convert_TargetValidationAndConversion(%sourceRec) {
   //Set write location and destination location to be used in the loop
   %newName =  $convert_FilenamePrefix @ fileBase(%sourceRec) @ ".rec";
   %writePath = $convert_writeDir @ "/" @ %newName;
   %destinationRec = filePath(strreplace(%sourceRec, "demos/", "demos/Converted/")) @ "/" @ $convert_FilenamePrefix @ fileBase(%sourceRec) @ ".rec";
   if (strstr(%sourceRec, "demos") == -1)
      %destinationRec = "marble/client/demos/Converted/" @ $convert_FilenamePrefix @ fileName(%sourceRec);
   $convert_Schedule = schedule(50, 0, "convert_LoopedSchedule", %sourceRec, %writePath, %destinationRec);

   // If not forced...
   if (!$convert_Forced) {
      // Checks if filename is shared in the destination path
      %targetExists = getFileSize(%destinationRec) > 0;
      if (%targetExists) {
         echo("\n\c6*** CONVERSION HALTED ***\n\c6Converted demo already exists:\n" @ "\c6" @ %destinationRec);
         verify_FailedCheck(%sourceRec, "Previously Converted");
         return;
      }

      // Checks if source is the new format
      %newFormat = verifynewformat(%sourceRec);
      if (%newFormat) { 
         echo("\n\c6*** CONVERSION HALTED ***\n\c6Demo is of the new format:\n" @ "\c6" @ %sourceRec);
         verify_FailedCheck(%sourceRec, "New Format");
         return;
      }
   }

   // Checks if mission file stored in source demo is valid
   %misPath = getMissionPath(%sourceRec);
   if (strlen(%misPath) == 0) {
      echo("\n\c6*** CONVERSION HALTED ***\n\c6Demo could not be played:\n" @ "\c6" @ %sourceRec);
      verify_FailedCheck(%sourceRec, "Failed to Play");
      return;
   }

   // Ensure necessary directories exist, then begin conversion
   convert_createWriteDirectory();
   doConvertDemo(%misPath, %sourceRec, %writePath, $convert_Timescale);
}

function verify_FailedCheck(%sourceRec, %tag) {
   $convert_Skipped = 1;
   $convert_SkippedCount++;
   $convert_OutputSkipped = $convert_OutputSkipped @ %sourceRec SPC "(" @ %tag @ ")\n";    
}

function convert_createWriteDirectory() {
   %demoCreated = mkdir("marble/client/demos");
   %convertedCreated = mkdir("marble/client/demos/Converted");
   %writeDirCreated = mkdir($convert_writeDir);
   if (%demoCreated || %convertedCreated || %writeDirCreated)
      convert_createWriteDirectory();
}

// Loops to validate completion. These validations must come prior to incrementing 
// the index and must be skipped if pre-validation failed to avoid file
// management artifacts. Loop schedule is staggered from move/deletion functions.
function convert_LoopedSchedule(%sourceRec, %writePath, %destinationRec) {
   if (!$playingDemo) {
      if (!$convert_Skipped) {
         //Desync Validation
         if ($Game::State !$= "End" && !$convert_Forced) {
            $Game::State = "End";
            $convert_DesyncCount++;
            $convert_OutputDesyncs = $convert_OutputDesyncs @ %sourceRec @ "\n"; 
            convert_DeleteUnfinishedRecording(%writePath);
            $convert_Schedule = schedule(100, 0, "convert_LoopedSchedule", %sourceRec, %writePath, %destinationRec);
            return;  
         }
         // Completion/Forced Validation
         if (getFileSize(%writePath) > 0) {
            if (getFileSize(%destinationRec) > 0) {
               convert_DeleteUnfinishedRecording(%destinationRec);
               $convert_Schedule = schedule(100, 0, "convert_LoopedSchedule", %sourceRec, %writePath, %destinationRec);
               return;
            }
            if ($Game::State $= "End") { 
               $convert_FinishCount++;
               %finish = mfloatLength(PlayGui.elapsedTime / 1000, 3);
               $convert_OutputFinished = $convert_OutputFinished @ %sourceRec SPC "(" @ %finish @ ")\n";
            } else {
               $convert_DesyncCount++;
               $convert_OutputDesyncs = $convert_OutputDesyncs @ %sourceRec @ "\n"; 
            }
            convert_MoveToDestinationPath(%writePath, %destinationRec);
         }
      }
      
      // End process if all sources have been converted
      $convert_FileIndex++;
      if ($convert_FileIndex == $convert_NumConvertFiles) {
         clearConvert();
         return;
      }

      // If all checks are passed, play next source
      $convert_Skipped = 0;
      %sourceRec = $convert_File[$convert_FileIndex];  
      convert_TargetValidationAndConversion(%sourceRec); 
   } else {
      $convert_Schedule = schedule(50, 0, "convert_LoopedSchedule", %sourceRec, %writePath, %destinationRec);
   }
}

// FILE MANAGEMENT ********************************************************************************************************
// Builds directory tree before moving
function convert_MoveToDestinationPath(%writePath, %destinationRec) {
   %tokenPath = filePath(%destinationRec);
   %buildDirectories = "";
   while(%buildDirectories !$= filePath(%destinationRec))
   {
      %tokenPath = nextToken(%tokenPath, "theToken", "/" );
      if (strlen(%buildDirectories) == 0) {
         %buildDirectories = %theToken;
      } else {
         %buildDirectories = %buildDirectories @ "/" @ %theToken;
      }
      if (strstr(%buildDirectories, "demos/Converted/") > 0) {
         while(mkDir(%buildDirectories)) {}
      }
   }
   %moveFile = schedule(50, 0, "moveFile", %writePath, %destinationRec);
}

// Also used in onDemoPlayDone in playMissionGui when canceling the conversion loop
function convert_DeleteUnfinishedRecording(%deletionFile) {
   if (strlen(%deletionFile) == 0) {
      %sourceRec = $convert_File[$convert_FileIndex];
      %deletionFile = $convert_writeDir @ "/" @ $convert_FilenamePrefix @ fileBase(%sourceRec) @ ".rec";
   }
   if (getFileSize(%deletionFile) > 0) {
      %deleteFile = schedule(50, 0, "removeFile", %deletionFile);
   } else {
      %deleteFile = schedule(50, 0, "convert_DeleteUnfinishedRecording", %deletionFile);
   }
}

// END PROCESSES *********************************************************************************************************
function convert_GetDuration(%start) {
   %duration = mAbs(mfloor((getRealTime() - %start) / 1000));
   %hours = mfloor(%duration /3600);
   %mins = mfloor((%duration - mfloor(%duration/3600) * 3600)/60);
   %secs = mfloor((%duration - mfloor(%duration/60) * 60));
   %durationOutput = " ";
   if (%hours)
      %durationOutput = %hours @ " hr "; 
   if (%mins)
      %durationOutput = %durationOutput @ %mins @ " min ";
   %durationOutput = %durationOutput @ %secs @ " sec";
   return %durationOutput;
}

// This function utilizes resource manager to create the report and its directories
function convert_CreateReport(%f, %stats, %output) {
   %fpath = "marble/client/demos/Converted/Reports/";
   %fname = "fullReport (";
   %inc = 1;
   while(getFileSize(%fpath @ %fname @ %inc @ ").txt") > 0){
      %inc++;
   }
   %fullPath = %fpath @ %fname @ %inc @ ").txt";

   %f.openForWrite(%fullPath);
   %f.writeLine(%stats @ "\n" @ %output);
   %f.close();
   %f.delete();
   return "See your full report at:\n" @ %fullPath @ "\n";
}

function clearConvert() {
   // Output
   if ($convert_FinishCount || $convert_DesyncCount || $convert_SkippedCount) {
      echo("\n\c1******************* \c0FINISHED CONVERTING \c1******************\n");
      %duration = convert_GetDuration($convert_StartTime);
      %stats = "Conversion Duration:" @ %duration @ "\n";
      %stats = %stats @ "Finished Recordings:" SPC $convert_FinishCount @ "\n";
      %stats = %stats @ "Unfinished Recordings:" SPC $convert_DesyncCount @ "\n";
      %stats = %stats @ "Skipped Recordings:" SPC $convert_SkippedCount @ "\n";
      echo(%stats);
      
      %output = "";
      if ($convert_FinishCount) {
         %output = $convert_OutputFinished;
      }
      if ($convert_DesyncCount) {
         if (strlen(%output) > 0)
            %output = %output @ "\n";
         %output = %output @ $convert_OutputDesyncs;
      }
      if ($convert_SkippedCount) {
         if (strlen(%output) > 0)
            %output = %output @ "\n";
         %output = %output @ $convert_OutputSkipped;
      }
      %report = new FileObject();
      echo(convert_CreateReport(%report, %stats, %output));
      echo("\c1**********************************************************\n");
      ToggleConsole(1);
   }

   // Clean-up
   cancel($convert_Schedule);
   $pref::showParticles = $convert_ToggleParticles;
   deleteVariables("$convert_*");
}