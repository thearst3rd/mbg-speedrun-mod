package Real {

function setPlayMissionGui()
{
   Canvas.setContent(playMissionGui);
   betterFPS($pref::showFPS);
}

function MainMenuQuit()
{
   quit();
}


function runIgnition()
{
   Canvas.setContent(MainMenuGui);
}

};

activatePackage(Real);