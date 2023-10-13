//-----------------------------------------------------------------------------
// Torque Game Engine
// 
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------

datablock StaticShapeData(StartPad)
{
   className = "Pad";
   category = "Pads";
   shapeFile = "~/data/shapes/pads/startArea.dts";
   scopeAlways = true;
   emap = false;
};

function StartPad::onAdd(%this, %obj)
{
   $Game::StartPad = %obj;
   %obj.setName("StartPoint");
   %obj.playThread(0,"ambient");
}

function StartPad::onClientCollision(%this,%obj,%col,%vec, %vecLen, %material)
{
	%time = PlayGui.elapsedTime + PlayGui.TotalBonus;

	if (%time > 0) {
		$go = $go + 1; 
		if ($go < 2) {
			echo(" ");
			echo("\c9First Bounce: " @ %time);
			echo(" ");
		}
	}
}

	


//-----------------------------------------------------------------------------

datablock StaticShapeData(EndPad)
{
   className = "Pad";
   category = "Pads";
   shapeFile = "~/data/shapes/pads/endArea.dts";
   scopeAlways = true;
   emap = false;
};

function EndPad::onAdd(%this, %obj)
{
   $Game::EndPad = %obj;
   %obj.setName("EndPoint");
   %obj.playThread(0,"ambient");
}

