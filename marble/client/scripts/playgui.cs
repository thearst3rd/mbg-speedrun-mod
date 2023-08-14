//-----------------------------------------------------------------------------
// Torque Game Engine
// 
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// PlayGui is the main TSControl through which the game is viewed.
// The PlayGui also contains the hud controls.
//-----------------------------------------------------------------------------

function PlayGui::onWake(%this)
{
   // Turn off any shell sounds...
   // alxStop( ... );

   $enableDirectInput = "1";
   activateDirectInput();

   // Message hud dialog
   Canvas.pushDialog( MainChatHud );
   chatHud.attach(HudMessageVector);

   // Make sure the display is up to date
   %this.setGemCount(%this.gemCount);
   %this.setMaxGems(%this.maxGems);
   %this.timerInc = 50;

   // just update the action map here
   if($playingDemo)
      demoMap.push();
   else
      moveMap.push();
   
   // hack city - these controls are floating around and need to be clamped
   schedule(0, 0, "refreshCenterTextCtrl");
   schedule(0, 0, "refreshBottomTextCtrl");
   playGameMusic();
}

function PlayGui::onSleep(%this)
{
   Canvas.popDialog(MainChatHud);
   // Terminate all playing sounds
   alxStopAll();

   playShellMusic();

   // pop the keymaps
   moveMap.pop();
   demoMap.pop();
}

//-----------------------------------------------------------------------------

function PlayGui::setMessage(%this,%bitmap,%timer)
{
   // Set the center message bitmap
   if (%bitmap !$= "")  {
      CenterMessageDlg.setBitmap($Con::Root @ "/client/ui/game/" @ %bitmap @ ".png",true);
      CenterMessageDlg.setVisible(true);
      cancel(CenterMessageDlg.timer);
      if (%timer)
         CenterMessageDlg.timer = CenterMessageDlg.schedule(%timer,"setVisible",false);
   }
   else
      CenterMessageDlg.setVisible(false);
}


//-----------------------------------------------------------------------------

function PlayGui::setPowerUp(%this,%shapeFile)
{
   // Update the power up hud control
   if (%shapeFile $= "")
      HUD_ShowPowerUp.setEmpty();
   else
      HUD_ShowPowerUp.setModel(%shapeFile, "");
}   


//-----------------------------------------------------------------------------

function PlayGui::setMaxGems(%this,%count)
{
   %this.maxGems = %count;

   %one = %count % 10;
   %ten = (%count - %one) / 10;
   GemsTotalTen.setNumber(%ten);
   GemsTotalOne.setNumber(%one);

   %visible = %count != 0;
   HUD_ShowGem.setVisible(%visible);
   GemsFoundTen.setVisible(%visible);
   GemsFoundOne.setVisible(%visible);
   GemsSlash.setVisible(%visible);
   GemsTotalTen.setVisible(%visible);
   GemsTotalOne.setVisible(%visible);
   HUD_ShowGem.setModel("marble/data/shapes/items/gem.dts","");
}

function PlayGui::setGemCount(%this,%count)
{
   %this.gemCount = %count;
   %one = %count % 10;
   %ten = (%count - %one) / 10;
   GemsFoundTen.setNumber(%ten);
   GemsFoundOne.setNumber(%one);
}   


//-----------------------------------------------------------------------------
// Elapsed Timer Display

function PlayGui::setTime(%this,%dt)
{
   %this.elapsedTime = %dt;
   %this.updateControls();
}

function PlayGui::resetTimer(%this,%dt)
{
   %this.elapsedTime = 0;
   %this.bonusTime = 0;
   %this.totalBonus = 0;
   if($BonusSfx !$= "")
   {
      alxStop($BonusSfx);
      $BonusSfx = "";
   }

   %this.updateControls();
   %this.stopTimer();
}

function PlayGui::adjustTimer(%this,%dt)
{
   %this.elapsedTime += %dt;
   %this.updateControls();
}

function PlayGui::addBonusTime(%this, %dt)
{
   %this.bonusTime += %dt;
   if($BonusSfx $= "")
      $BonusSfx = alxPlay(TimeTravelLoopSfx);
}

function PlayGui::startTimer(%this)
{
   $PlayTimerActive = true;
}

function onFrameAdvance(%timeDelta)
{
   if($PlayTimerActive) {
      PlayGui.updateTimer(%timeDelta);
   }

   if ($pref::showParticles) {
      $particles = 1;
   } else {
      $particles = 0;
   }
   
   %marbleExists = isObject(ServerConnection) && isObject(ServerConnection.getControlObject()) && ServerConnection.getControlObject();
   if (%marbleExists && $Game::State !$= "End" && !$playingDemo) {
      enforceTimeScale();
   }

   TimerHundredths.setVisible(!$pref::extendedTimer);
   TimerThousandths.setVisible($pref::extendedTimer);
}

function PlayGui::stopTimer(%this)
{
   $PlayTimerActive = false;
   if($BonusSfx !$= "")
   {
      alxStop($BonusSfx);
      $BonusSfx = "";
   }
}

function PlayGui::updateTimer(%this, %timeInc)
{
   if(%this.bonusTime)
   {
      if(%this.bonusTime > %timeInc)
      {
         %this.bonusTime -= %timeInc;
         %this.totalBonus += %timeInc;
         %timeInc = 0;
      }
      else
      {
         %timeInc -= %this.bonusTime;
         %this.totalBonus += %this.bonusTime;
         %this.bonusTime = 0;
         alxStop($BonusSfx);
         $BonusSfx = "";
      }
   }
   %this.elapsedTime += %timeInc;

   // Some sanity checking
   if (%this.elapsedTime > 3600000)
      %this.elapsedTime = 3599999;

   %this.updateControls();
}   

function PlayGui::updateControls(%this)
{
   %et = %this.elapsedTime;
   %drawNeg = false;
   if(%et < 0)
   {
      %et = - %et;
      %drawNeg = true;
   }

   %hundredth = mFloor((%et % 1000) / 10);
   %totalSeconds = mFloor(%et / 1000);
   %seconds = %totalSeconds % 60;
   %minutes = (%totalSeconds - %seconds) / 60;

   %secondsOne      = %seconds % 10;
   %secondsTen      = (%seconds - %secondsOne) / 10;
   %minutesOne      = %minutes % 10;
   %minutesTen      = (%minutes - %minutesOne) / 10;
   %hundredthOne    = %hundredth % 10; 
   %hundredthTen    = (%hundredth - %hundredthOne) / 10;
   %thousandths	    = %et % 10;

   // Update the controls
   Min_Ten1.setNumber(%minutesTen);
   Min_One1.setNumber(%minutesOne);
   Sec_Ten1.setNumber(%secondsTen);
   Sec_One1.setNumber(%secondsOne);
   Sec_Tenth1.setNumber(%hundredthTen);
   Sec_Hundredth1.setNumber(%hundredthOne);
   Sec_Thousandth1.setNumber(%thousandths);
   PG_NegSign1.setVisible(%drawNeg);

   Min_Ten2.setNumber(%minutesTen);
   Min_One2.setNumber(%minutesOne);
   Sec_Ten2.setNumber(%secondsTen);
   Sec_One2.setNumber(%secondsOne);
   Sec_Tenth2.setNumber(%hundredthTen);
   Sec_Hundredth2.setNumber(%hundredthOne);
   PG_NegSign2.setVisible(%drawNeg);
}


//-----------------------------------------------------------------------------

function GuiBitmapCtrl::setNumber(%this,%number)
{
   %this.setBitmap($Con::Root @ "/client/ui/game/numbers/" @ %number @ ".png");
}


//-----------------------------------------------------------------------------

function refreshBottomTextCtrl()
{
   BottomPrintText.position = "0 0";
}

function refreshCenterTextCtrl()
{
   CenterPrintText.position = "0 0";
}

function enforceTimeScale() {
   %et = playGui.elapsedTime + playGui.totalBonus;
   %rt = getRealTime(); 

   if (%et < 100 || $wasPaused) {
      $recTimeOffset = %rt - %et;
      $wasPaused = 0;
   } else if (%et > 100) {
      %timeCompare = %rt - %et;
      %diff = mFloor(%timeCompare - $recTimeOffset);

      if (mAbs(%diff) > 1000) {
         disconnect();
      }
   }
}