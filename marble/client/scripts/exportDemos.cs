//***************************************** SCRIPT-SIDE REC EXPORT *****************************************
function exportDemos() {
   ToggleConsole(1);
   deleteVariables("$export_*");
   $export_StartTime = getRealTime();
   $export_NumberofExports = 0;
   $export_NumExportFiles = 0;
   $export_FileIndex = 0;

   export_BuildRecordingList();
   if ($export_NumExportFiles == 0) {
      clearExport("No recordings found.");
   } else {
      export_LoopedSchedule();
   }
}

function export_BuildRecordingList()
{
   for(%file = findFirstFile("*.rec"); %file !$= ""; %file = findNextFile("*.rec"))
   {
      if (strstr(%file, "Exported") == -1) {
         $export_File[$export_NumExportFiles] = %file;
         $export_NumExportFiles++;
      }
   }
}

function export_LoopedSchedule(%sourceRec) {
   if ($export_FileIndex == $export_NumExportFiles) {
      clearExport();
      return;
   } else {
      %currentIndex = "EXPORT IN PROGRESS\n" @ $export_FileIndex @ "/" @ $export_NumExportFiles; 
      MessageBoxOK(" ", %currentIndex);

      %sourceRec = $export_File[$export_FileIndex];
      %newFormat = verifynewformat(%sourceRec);
      if (%newFormat) {
         export_createDestinationPath(%sourceRec); 
      } else {
         $export_Schedule = schedule(1, 0, "export_LoopedSchedule");
      }
      $export_FileIndex++;
   }
}

function export_createDestinationPath(%sourceRec) {
   //Sets destination path
   %destinationRec = strreplace(%sourceRec, "demos/", "demos/Exported/");
   if (strstr(%sourceRec, "demos") == -1) {
      %destinationRec = "marble/client/demos/Exported/" @ fileName(%sourceRec);
   } 

   // Builds directory tree
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
      if (strstr(%buildDirectories, "demos") > 0) {
         mkDir(%buildDirectories);
      }
   }

   //Copies file if it doesn't already exist
   %exists = getFileSize(%destinationRec);
   if (!%exists) {
      copyRec(%sourceRec, %destinationRec);
      $export_NumberofExports++;
   }
   $export_Schedule = schedule(1, 0, "export_LoopedSchedule");
}

function clearExport(%output) {
   %title = "EXPORT ABORTED";
   if (strlen(%output) == 0){
      %title = "EXPORT COMPLETE";
      %duration = mfloatLength(((getRealTime() - $export_StartTime) / 1000), 3) @ "s";
      %output = "Duration:" SPC %duration @ "\n";
      %output = %output @ "Exports:" SPC $export_NumberofExports @ "/" @ $export_NumExportFiles;
   }
   MessageBoxOK(%title, %output);
   deleteVariables("$export_*");
}