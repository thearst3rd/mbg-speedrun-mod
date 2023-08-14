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
   // Actually set FOV...
   setFov($pref::Player::defaultFov);

   // Timer visibilities
   TimerHundredths.setVisible(!$pref::extendedTimer);
   TimerThousandths.setVisible($pref::extendedTimer);
   %this.updateTimeTravelCountdown();

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
      setTimeScale(1);
   }
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

   TimerHundredths.setVisible(!$pref::extendedTimer);
   TimerThousandths.setVisible($pref::extendedTimer);

   %this.updateControls();
   %this.updateTimeTravelCountdown();
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

// From PlatinumQuest, written by main_gi. Tweaked for MBG, and to optionally show thousandths
function PlayGui::updateTimeTravelCountdown(%this)
{
   if (!$pref::timeTravelDisplay) {
      PGCountdownTT.setVisible(false);
      return;
   }

   %timeUsed = %this.bonusTime;
   if ($pref::timeTravelDisplay == 2) {
      %timeUsed += 99; // When you pick up a 5s timer, it should start by displaying 5.0, instead of 4.9. This also prevents the TT timer from showing 0.0. But if you add 100, picking up a 5s timer can show "5.1". Turns out adding 99 actually works perfectly here.
   } else if (!$pref::extendedTimer) {
      %timeUsed += 9;
   }

   if (%timeUsed > 999999)
      %timeUsed = 999999; // Seems time bonuses in vanilla MBG glitch out this high anyway lol

   %secondsLeft = mFloor(%timeUsed/1000);
   %tenths = mFloor(%timeUsed/100) % 10;
   %hundredths = mFloor(%timeUsed/10) % 10;
   %thousandths = %timeUsed % 10;

   %one = mFloor(%secondsLeft) % 10;
   %ten = mFloor(%secondsLeft / 10) % 10;
   %hun = mFloor(%secondsLeft / 100);

   %offsetIfThousandths = $pref::extendedTimer ? 12 : 0;

   if (%secondsLeft < 10) {
      PGCountdownTTFirstDigit.setNumber(%one);
      PGCountdownTTSecondDigit.setNumber(%tenths);
      PGCountdownTTSecondDigit.setPosition("397" + %offsetIfThousandths SPC "0");
      PGCountdownTTThirdDigit.setNumber(%hundredths);
      PGCountdownTTThirdDigit.setPosition("413" + %offsetIfThousandths SPC "0");
      PGCountdownTTFourthDigit.setNumber(%thousandths);
      PGCountdownTTPoint1.setVisible(true);
      PGCountdownTTPoint1.setPosition("388" + %offsetIfThousandths SPC "0");
      PGCountdownTTPoint2.setVisible(false);
      PGCountdownTTPoint3.setVisible(false);
      %digits = 4;
   } else if (%secondsLeft < 100) {
      PGCountdownTTFirstDigit.setNumber(%ten);
      PGCountdownTTSecondDigit.setNumber(%one);
      PGCountdownTTSecondDigit.setPosition("391" + %offsetIfThousandths SPC "0");
      PGCountdownTTThirdDigit.setNumber(%tenths);
      PGCountdownTTThirdDigit.setPosition("413" + %offsetIfThousandths SPC "0");
      PGCountdownTTFourthDigit.setNumber(%hundredths);
      PGCountdownTTFifthDigit.setNumber(%thousandths);
      PGCountdownTTPoint1.setVisible(false);
      PGCountdownTTPoint2.setVisible(true);
      PGCountdownTTPoint2.setPosition("404" + %offsetIfThousandths SPC "0");
      PGCountdownTTPoint3.setVisible(false);
      %digits = 5;
   } else {
      PGCountdownTTFirstDigit.setNumber(%hun);
      PGCountdownTTSecondDigit.setNumber(%ten);
      PGCountdownTTSecondDigit.setPosition("391" + %offsetIfThousandths SPC "0");
      PGCountdownTTThirdDigit.setNumber(%one);
      PGCountdownTTThirdDigit.setPosition("407" + %offsetIfThousandths SPC "0");
      PGCountdownTTFourthDigit.setNumber(%tenths);
      PGCountdownTTFifthDigit.setNumber(%hundredths);
      PGCountdownTTSixthDigit.setNumber(%thousandths);
      PGCountdownTTPoint1.setVisible(false);
      PGCountdownTTPoint2.setVisible(false);
      PGCountdownTTPoint3.setVisible(true);
      PGCountdownTTPoint3.setPosition("420" + %offsetIfThousandths SPC "0");
      %digits = 6;
   }

   PGCountdownTTImage.setPosition("348" + %offsetIfThousandths SPC "3");
   PGCountdownTTFirstDigit.setPosition("375" + %offsetIfThousandths SPC "0");
   PGCountdownTTFourthDigit.setPosition("429" + %offsetIfThousandths SPC "0");
   PGCountdownTTFifthDigit.setPosition("445" + %offsetIfThousandths SPC "0");
   PGCountdownTTSixthDigit.setPosition("461" + %offsetIfThousandths SPC "0");

   if ($pref::timeTravelDisplay == 2) {
      %digits -= 2;
   } else if (!$pref::extendedTimer) {
      %digits -= 1;
   }

   //PGCountdownTTFirstDigit.setVisible(%digits >= 1); // Always true
   //PGCountdownTTSecondDigit.setVisible(%digits >= 2); // Always true
   PGCountdownTTThirdDigit.setVisible(%digits >= 3);
   PGCountdownTTFourthDigit.setVisible(%digits >= 4);
   PGCountdownTTFifthDigit.setVisible(%digits >= 5);
   PGCountdownTTSixthDigit.setVisible(%digits >= 6);

   PGCountdownTT.setVisible(%this.bonusTime);
}


//-----------------------------------------------------------------------------

function GuiBitmapCtrl::setNumber(%this,%number)
{
   %this.setBitmap($Con::Root @ "/client/ui/game/numbers/" @ %number @ ".png");
}

function GuiControl::setPosition(%gui, %position) {
   if (%gui.position $= %position)
      return;

   %p1 = getWord(%position, 0);
   %p2 = getWord(%position, 1);
   %e1 = getWord(%gui.extent, 0);
   %e2 = getWord(%gui.extent, 1);

   %gui.resize(%p1, %p2, %e1, %e2);
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
