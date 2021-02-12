// DisplayName = Warfare

//--- GAME RULES BEGIN ---
//Teams try to touch the switch in the enemy base
//Generators help to protect the switch
//5 rounds of gameplay; 3 captures win the game
//<color:FFFFFF>Warfare<color:42DBEA> by Red Shifter
//--- GAME RULES END ---

package WarfareGame
{

function FlipFlop::playerTouch(%data, %flipflop, %player)
{
   if( !%flipflop.needsObjectiveWaypoint ) {
      Parent::playerTouch(%data, %flipflop, %player);
      return;
   }

   if(%player.team == %flipflop.team)
      return;

   // don't end the map during a mapping session!
   if( $testCheats ) {
      messageAll( 0, "\c2No capping when mapping!");
      return;
   }

   %defTeam = %flipflop.team;
   Game.capPlayer = stripChars( getTaggedString( %player.client.name ), "\cp\co\c6\c7\c8\c9" );

   // Let the observers know:
   messageTeam( 0, 'MsgSiegeTouchFlipFlop', '\c2%1 captured the %2 base!~wfx/misc/flipflop_taken.wav', %player.client.name, $TeamName[%defTeam] );
   // Let the teammates know:
   messageTeam( %player.team, 'MsgSiegeTouchFlipFlop', '\c2%1 captured the %2 base!~wfx/misc/flipflop_taken.wav', %player.client.name, $TeamName[%defTeam] );
   // Let the other team know:
   %losers = %player.team == 1 ? 2 : 1;
   messageTeam( %losers, 'MsgSiegeTouchFlipFlop', '\c2%1 captured your base!~wfx/misc/flipflop_lost.wav', %player.client.name);

   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") captured team "@%defTeam@" base");

   Game.winningTeam = %player.team;

   Game.allObjectivesCompleted();
}

function StaticShapeData::onDisabled(%data, %obj, %prevState)
{
	Parent::onDisabled(%data, %obj, %prevState);
   
	if(%obj.waypoint) {
		if(%obj.notRepairable) {
			%obj.waypoint.delete();
			%obj.setRepairRate(0);
			%obj.setDamageLevel(2.5);
		}
		else
			Game.switchWaypoint(%obj.waypoint);
	}
}

function StaticShapeData::onEnabled(%data, %obj, %prevState)
{
	if(%obj.waypoint)
		Game.switchWaypoint(%obj.waypoint);

	if(%obj.isPowered())
		%data.onGainPowerEnabled(%obj);

	Parent::onEnabled(%data, %obj, %prevState);
}

function SimGroup::WarfareSwitchCleaner(%this) {
   if( %this == nameToID("MissionCleanup") )
      return;

   for (%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).WarfareSwitchCleaner();   
}

function SimObject::WarfareSwitchCleaner(%this)
{
}

function GameBase::WarfareSwitchCleaner(%this) {
   if(%this.dataBlock $= "FlipFlop" && !%this.needsObjectiveWaypoint) {
      setTargetSkin(%this.getTarget(), Game.getTeamSkin(0));
      setTargetSensorGroup(%this.getTarget(), 0);

      // if there is a "projector" associated with this flipflop, axe it
      if(%this.projector > 0)
         if(%this.projector.holo > 0)
            %this.projector.holo.delete();
 
      Game.claimFlipFlopResources(%this, 0);
   }
}

// z0dd - ZOD, 5/17/02. Clean up deployables triggers, function in supportClassic.cs
function cleanTriggers(%group) {
   if (%group > 0)
      %depCount = %group.getCount();
   else
      return;

   for(%i = 0; %i < %depCount; %i++)
   {
      %deplObj = %group.getObject(%i);
      if(isObject(%deplObj))
      {
         if(%deplObj.trigger !$= "")
            %deplObj.trigger.schedule(0, "delete");
      }
   }
}

// added this function to allow a player to map without having to worry about time limit expiration
function toggleEditor(%val) {
   // kill the countdown at the end of the round
   if($TestCheats == 0)
      CancelEndCountdown();

   Parent::toggleEditor(%val);

   %game.result[%game.thisRound] = "<bitmap:bullet_2><lmargin:24>Round " @ %game.thisRound @ ": Someone wanted to map instead.<lmargin:0>";
}

}; // end WarfareGame package

function WarfareGame::initGameVars(%game)
{
	%game.SCORE_PER_SUICIDE = 0; 
	%game.SCORE_PER_TEAMKILL = 0;
	%game.SCORE_PER_DEATH = 0;  
	%game.SCORE_PER_KILL = 0;  
	%game.SCORE_PER_TURRET_KILL = 0;

	// gamestate variables
	%game.ROUND_COUNT = 5;  // number of rounds played
        %game.ROUND_TIME  = 10; // length of a round
}

function WarfareGame::findObjectiveWaypoints(%game, %group)
{
	if(!%group)
		%group = nameToId("MissionGroup/Teams");
	
	for (%i = 0; %i < %group.getCount(); %i++)
	{
		%obj = %group.getObject(%i);
		if(%obj.getClassName() $= SimGroup)
		{
			%game.findObjectiveWaypoints(%obj);
		}
 		else if(%obj.needsObjectiveWaypoint)
 		{
 			%game.initializeWaypointAtObjective(%obj);
 		}
	}
}


function WarfareGame::initializeWaypointAtObjective(%game, %object)
{
   // out with the old...jic
   if ( %object.waypoint )
   {
      if ( isObject( %object.waypoint ) )
         %object.waypoint.delete();
      else
         %object.waypoint = "";   
   }

   // to make the waypoint look a little prettier we are using the z from
   // position and the x and y from worldBoxCenter
   %posX = getWord(%object.getWorldBoxCenter(), 0);
   %posY = getWord(%object.getWorldBoxCenter(), 1);
   %posZ = getWord(%object.position, 2);

   %append = getTaggedString(%object.getDataBlock().targetTypeTag);

   %object.waypoint = new WayPoint() {
	position = %posX SPC %posY SPC %posZ;
	rotation = "1 0 0 0";
	scale = "1 1 1";
	dataBlock = "WayPointMarker";
	team = %object.team;
	name = getTaggedString(%object.nameTag) SPC %append;
   };
   MissionCleanup.add(%object.waypoint);
}

function WarfareGame::switchWaypoint(%game, %waypoint)
{
	%team = %waypoint.team;
	%newTeam = (%team == 1 ? 2 : 1);
	
	%waypoint.team = %newTeam; 
}

function WarfareGame::missionLoadDone(%game)
{
   //default version sets up teams - must be called first...
   DefaultGame::missionLoadDone(%game);

   //clear the scores
   $teamScore[1] = 0;
   $teamScore[2] = 0;
   %game.thisRound = 0;

   // save off turret bases' original barrels
   %game.checkTurretBases();

   // add objective waypoints
   %game.findObjectiveWaypoints();

   // quickpass forcefield functionality
   %game.generateQuickPass();

   MissionGroup.setupPositionMarkers(true);

   // special function to be overloaded
   %game.initScript();
}

function WarfareGame::startMatch(%game)
{
   DefaultGame::startMatch( %game );

   %game.startTimeMS = getSimTime();

   // set up the timer
   %game.timeLimitMS = %game.ROUND_TIME * 60 * 1000;

   %game.thisRound++;
   %game.result[%game.thisRound] = "<bitmap:bullet_2><lmargin:24>Round " @ %game.thisRound @ ": Was not finished<lmargin:0>";
   
   // schedule first timeLimit check for 20 seconds
   %game.timeSync = %game.schedule( 20000, "checkTimeLimit");
   %game.timeThread = %game.schedule( %game.timeLimitMS, "timeLimitReached");
   //updateClientTimes(%game.timeLimitMS);
   messageAll('MsgSystemClock', "", %game.ROUND_TIME, %game.timeLimitMS);

   // override the end countdown set in DefaultGame::startMatch; it's not right
   cancelEndCountdown();
   EndCountdown(%game.timeLimitMS);
}

function WarfareGame::timeLimitReached(%game)
{
   // don't end the map during a mapping session!
   if( $testCheats )
      return;

   %gameOver = 0;
   cancel( %game.timeThread );
   cancel( %game.timeSync );

   // if time has run out, nothing happens
   messageAll('MsgSiegeFailed', '\c2Time has expired.');

   %game.result[%game.thisRound] = "<bitmap:bullet_2><lmargin:24>Round " @ %game.thisRound @ ": No team made a capture.<lmargin:0>";

   %game.captureTime = 0;
   %game.winningTeam = 0;
   %game.capPlayer   = "";

   if( %game.gameDecided() ) {
      // game is over
      messageAll('MsgSiegeMisDone', '\c2Mission complete.');
      %gameOver = 1;
   }
   else {
      // it's halftime, let everyone know
      messageAll( 'MsgSiegeHalftime' );
   }

   logEcho("time limit reached");
   %game.halftime('time', %gameOver);
}

function WarfareGame::checkTimeLimit(%game)
{
   //if we're counting down, check back in 
   if (%game.secondHalfCountDown)
   {   
      %game.timeSync = %game.schedule(1000, "checkTimeLimit");
      return;
   }

   %timeElapsedMS = getSimTime() - %game.startTimeMS;
   %curTimeLeftMS = %game.timeLimitMS - %timeElapsedMS;
      
   if (%curTimeLeftMS <= 0)
   {
      // time's up, put down your pencils
      %game.timeLimitReached();                        
   }
   else
   {
      if(%curTimeLeftMS >= 20000)
         %game.timeSync = %game.schedule( 20000, "checkTimeLimit" );
      else
         %game.timeSync = %game.schedule( %curTimeLeftMS + 1, "checkTimeLimit" );
                                                                             
      //now synchronize everyone's clock
      messageAll('MsgSystemClock', "", %game.ROUND_TIME, %curTimeLeftMS);
   }
}

function WarfareGame::allObjectivesCompleted(%game)
{
   %gameOver = 0;
   Cancel( %game.timeSync );
   Cancel( %game.timeThread );
   cancelEndCountdown();

   // award some points
   $teamScore[%game.winningTeam]++;
   messageAll('MsgTeamScoreIs', "", %game.winningTeam, $TeamScore[%game.winningTeam]);

   //store the elapsed time...
   %game.captureTime = getSimTime() - %game.startTimeMS;
   messageAll('MsgSiegeCaptured', '\c2Team %1 captured the base in %2!', $teamName[%game.winningTeam], %game.formatTime(%game.captureTime, true));

   %losers = %game.winningTeam == 1 ? 2 : 1;
   %game.result[%game.thisRound] = "<bitmap:bullet_2><lmargin:24>Round " @ %game.thisRound @ ": " @ detag(%game.capPlayer) @ " (" @ getTaggedString($TeamName[%game.winningTeam]) @ ") captured " @ getTaggedString($TeamName[%losers]) @ " in " @ %game.formatTime( %game.captureTime, true ) @ ".<lmargin:0>";

   if( %game.gameDecided() ) {
      // game is over
      messageAll('MsgSiegeMisDone', '\c2Mission complete.');
      %gameOver = 1;
   }
   else {
      // it's halftime, let everyone know
      messageAll( 'MsgSiegeHalftime' );
   }

   logEcho("objective completed in "@%game.timeLimitMS);

   //setup the second half...
   // MES -- per MarkF, scheduling for 0 seconds will prevent player deletion-related crashes
   %game.schedule(0, halftime, 'objectives', %gameOver);
}

function WarfareGame::gameDecided(%game) {
   // game ends under 2 conditions

   // 1. out of rounds
   if( %game.thisRound == %game.ROUND_COUNT )
      return true;

   // 2. losing team cannot catch up in the number of remaining rounds
   %leadingTeam = $TeamScore[2] > $TeamScore[1] ? 2 : 1;
   %laggingteam = %leadingTeam == 1 ? 2 : 1;

   if( $TeamScore[%leadingTeam] > $TeamScore[%laggingTeam] + (%game.ROUND_COUNT - %game.thisRound) )
      return true;

   // neither condition was met?  continue playing!
   return false;
}

function WarfareGame::neutralizeObjectives(%game) {
   nameToID("MissionGroup").WarfareSwitchCleaner();
}

function WarfareGame::halftime(%game, %reason, %gameOver)
{
   //stop the game
   $MatchStarted = false;

   if (!%gameOver)
   {
      //reset stations and vehicles that players were using
      %game.resetPlayers();

      // zero out the counts for deployable items (found in defaultGame.cs)
      %game.clearDeployableMaxes();

      // get rid of triggers using a convenient function in classic
      // this will also work in base because i copied the function to this file
      cleanTriggers(nameToID("MissionCleanup/Deployables"));

      // clean up the MissionCleanup group - note, this includes deleting all the player objects
      %clean = nameToID("MissionCleanup");
      %clean.housekeeping();

      // make neutral objectives neutral again
      %game.neutralizeObjectives();

      // Non static objects placed in original position
      resetNonStaticObjPositions();

      //restore the objects
      %game.restoreObjects();

      %count = ClientGroup.getCount();
      for(%cl = 0; %cl < %count; %cl++)
      {
         %client = ClientGroup.getObject(%cl);
         if( !%client.isAIControlled() )
         {
            // Put everybody in observer mode:
            %client.camera.getDataBlock().setMode( %client.camera, "observerStaticNoNext" );
            %client.setControlObject( %client.camera );
            
            // Send the halftime result info:
            if ( %game.captureTime == 0 )
            {
               messageClient( %client, 'MsgSiegeResult', "", 'No team made a capture!' );   
            }
            else if ( %game.winningTeam == %client.team || %client.team == 0 ) {
               %losers = %game.winningTeam == 1 ? 2 : 1;

               messageClient( %client, 'MsgSiegeResult', "", '%1 captured the %2 base in %3!', %game.capPlayer, $teamName[%losers], %game.formatTime( %game.captureTime, true ) );
            }
            else {
               %losers = %game.winningTeam == 1 ? 2 : 1;

               messageClient( %client, 'MsgSiegeResult', "", '%1 captured your base in %2!', %game.capPlayer, %game.formatTime( %game.captureTime, true ) );

            }
            
            // List out the team rosters:
            messageClient( %client, 'MsgSiegeAddLine', "", '<spush><color:00dc00><font:univers condensed:18><clip%%:50>%1</clip><lmargin%%:50><clip%%:50>%2</clip><spop>', $TeamName[1], $TeamName[2] );
            %max = $TeamRank[1, count] > $TeamRank[2, count] ? $TeamRank[1, count] : $TeamRank[2, count];
            for ( %line = 0; %line < %max; %line++ )
            {
               %plyr1 = $TeamRank[1, %line] $= "" ? "" : $TeamRank[1, %line].name;
               %plyr2 = $TeamRank[2, %line] $= "" ? "" : $TeamRank[2, %line].name;
               messageClient( %client, 'MsgSiegeAddLine', "", '<lmargin:0><clip%%:50> %1</clip><lmargin%%:50><clip%%:50> %2</clip>', %plyr1, %plyr2 );
            }

            // Show observers:
            %header = false;
            for ( %i = 0; %i < %count; %i++ )
            {
               %obs = ClientGroup.getObject( %i );
               if ( %obs.team <= 0 )
               {
                  if ( !%header )
                  {
                     messageClient( %client, 'MsgSiegeAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>OBSERVERS<spop>' );
                     %header = true;
                  }

                  messageClient( %client, 'MsgSiegeAddLine', "", ' %1', %obs.name );
               }
            }
            
            commandToClient( %client, 'SetHalftimeClock', 10000 / 60000 );
            
            // Get the HUDs right:
            commandToClient( %client, 'setHudMode', 'SiegeHalftime' );
            commandToClient( %client, 'ControlObjectReset' );
            
            clientResetTargets(%client, true);
            %client.notReady = true;
         }
      }
      
      %game.schedule( 10000, halftimeOver );
   }
   else
   {
      // let's wrap it all up
      %game.gameOver();
      cycleMissions();
   }
}

function WarfareGame::halftimeOver( %game )
{
   // special function to be overloaded
   %game.initScript();

   // drop all players into mission
   %game.dropPlayers();

   // start the mission again (release players)
   %game.halfTimeCountDown( $Host::warmupTime );

   //redo the objective waypoints
   %game.findObjectiveWaypoints();
}

function WarfareGame::halfTimeCountDown(%game, %time)
{
   %game.secondHalfCountDown = true;
   $MatchStarted = false;

   %timeMS = %time * 1000;
   %game.schedule(%timeMS, "startMatch");
   notifyMatchStart(%timeMS);
   
   if(%timeMS > 30000)
      schedule(%timeMS - 30000, 0, "notifyMatchStart", 30000);
   if(%timeMS > 20000)
      schedule(%timeMS - 20000, 0, "notifyMatchStart", 20000);
   if(%timeMS > 10000)
      schedule(%timeMS - 10000, 0, "notifyMatchStart", 10000);
   if(%timeMS > 5000)
      schedule(%timeMS - 5000, 0, "notifyMatchStart", 5000);
   if(%timeMS > 4000)
      schedule(%timeMS - 4000, 0, "notifyMatchStart", 4000);
   if(%timeMS > 3000)
      schedule(%timeMS - 3000, 0, "notifyMatchStart", 3000);
   if(%timeMS > 2000)
      schedule(%timeMS - 2000, 0, "notifyMatchStart", 2000);
   if(%timeMS > 1000)
      schedule(%timeMS - 1000, 0, "notifyMatchStart", 1000);
}

function WarfareGame::dropPlayers( %game )
{
   %count = ClientGroup.getCount();
   for(%cl = 0; %cl < %count; %cl++)
   {
      %client = ClientGroup.getObject(%cl);
      if( !%client.isAIControlled() )
      {
         // keep observers in observer mode
         if(%client.team == 0)
            %client.camera.getDataBlock().setMode(%client.camera, "justJoined");
         else
         {
            %game.spawnPlayer( %client, false );
            
            %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
            %client.setControlObject( %client.camera );
            %client.notReady = false;
         }
      }
   }      
}

function WarfareGame::resetPlayers(%game)
{
   // go through the client group and reset anything the players were using
   // is most of this stuff really necessary?
   %count = ClientGroup.getCount();
   for(%cl = 0; %cl < %count; %cl++)
   {
      %client = ClientGroup.getObject(%cl);
      %player = %client.player;

      // clear the pack icon
      messageClient(%client, 'msgPackIconOff', "");
      // if player was firing, stop firing
      if(%player.getImageTrigger($WeaponSlot))
         %player.setImageTrigger($WeaponSlot, false);

      // if player had pack activated, deactivate it
      if(%player.getImageTrigger($BackpackSlot))
         %player.setImageTrigger($BackpackSlot, false);

      // if player was in a vehicle, get rid of vehicle hud
      commandToClient(%client, 'setHudMode', 'Standard', "", 0);

      // clear player's weapons and inventory huds
      %client.SetWeaponsHudClearAll();
      %client.SetInventoryHudClearAll();

      // if player was at a station, deactivate it
      if(%player.station)
      {
         %player.station.triggeredBy = "";
         %player.station.getDataBlock().stationTriggered(%player.station, 0);
         if(%player.armorSwitchSchedule)
            cancel(%player.armorSwitchSchedule);
      }

      // if piloting a vehicle, reset it (assuming it doesn't get deleted)
      if(%player.lastVehicle.lastPilot)
         %player.lastVehicle.lastPilot = "";
   }
}

function WarfareGame::resetScore(%game, %client) {
   // goodbye console spam
}

function WarfareGame::updateKillScores(%game) {
   // no scoring.
}

function WarfareGame::AIinit(%game) {
   // call this retarded function to keep the console from being spammed
   // solely for the benefit of stupid shit that nobody gives a flying fuck about
   AIInit();
}

function WarfareGame::checkTurretBases(%game)
{
   %mGroup = nameToID("MissionGroup/Teams");
   %mGroup.findTurretBase();
}

function WarfareGame::restoreObjects(%game)
{
   // restore all the "permanent" mission objects to undamaged state
   %group = nameToID("MissionGroup/Teams");
   // SimGroup::objectRestore is defined in DefaultGame.cs -- it simply calls
   // %game.groupObjectRestore
   %group.objectRestore();
}

function WarfareGame::groupObjectRestore(%game, %this)
{
   for(%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).objectRestore();
}

function WarfareGame::shapeObjectRestore(%game, %object)
{
   if(%object.getDamageLevel())
   {
      %object.setDamageLevel(0.0);
      %object.setDamageState(Enabled);
   }
   if(%object.getDatablock().getName() $= "TurretBaseLarge")
   {
      // check to see if the turret base still has the same type of barrel it had
      // at the beginning of the mission
      if(%object.getMountedImage(0))
         if(%object.getMountedImage(0).getName() !$= %object.originalBarrel)
         {
            // pop the "new" barrel
            %object.unmountImage(0);
            // mount the original barrel
            %object.mountImage(%object.originalBarrel, 0, false);
         }
   }
}

function WarfareGame::gameOver(%game)
{
   //turn off testcheats (in case of a mapping session)
   $testCheats = 0;

   //call the default
   DefaultGame::gameOver(%game);

   cancel(%game.timeThread);

   messageAll('MsgClearObjHud', "");
}

function WarfareGame::sendDebriefing( %game, %client )
{
   //if neither team captured
   %winnerName = "";
   if ( $teamScore[1] == $teamScore[2] )
      %winner = 0;

   //else see if team1 won
   else if ( $teamScore[1] > $teamScore[2])
   {
      %winner = 1;
      %winnerName = $teamName[1];
   }

   //else see if team2 won
   else if ($teamScore[2] > $teamScore[1])
   {
      %winner = 2;
      %winnerName = $teamName[2];
   }

   //send the winner message
   if (%winnerName $= 'Storm')
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.stowins.wav" );
   else if (%winnerName $= 'Inferno')
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.infwins.wav" );
   else if (%winnerName $= 'Blood Eagle')
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.bewin.wav" );
   else if (%winnerName $= 'Starwolf')
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.swwin.wav" );
   else if (%winnerName $= 'Diamond Sword')
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.dswin.wav" );
   else if (%winnerName $= 'Phoenix')
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.pxwin.wav" );
   else
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav" );

   // Mission result:
   if (%winner > 0)
   {
      if (%winner == 1)
      {  
         messageClient(%client, 'MsgDebriefResult', "", '<just:center>Team %1 wins!', $TeamName[1]);
      }
      else 
      {
         messageClient(%client, 'MsgDebriefResult', "", '<just:center>Team %1 wins!', $TeamName[2]);
      }
   }
   else
      messageClient( %client, 'MsgDebriefResult', "", '<just:center>The mission ended in a tie.' );

   // Game summary:
   messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>SUMMARY:<spop>' );

   for( %i = 1; %i <= %game.thisRound; %i++ ) {
	messageClient( %client, 'MsgDebriefAddLine', "", %game.result[%i]);
   }

   // List out the team rosters:
   messageClient( %client, 'MsgDebriefAddLine', "", '\n<spush><color:00dc00><font:univers condensed:18><clip%%:50>%1</clip><lmargin%%:50><clip%%:50>%2</clip><spop>', $TeamName[1], $TeamName[2] );
   %max = $TeamRank[1, count] > $TeamRank[2, count] ? $TeamRank[1, count] : $TeamRank[2, count];
   for ( %line = 0; %line < %max; %line++ )
   {
      %plyr1 = $TeamRank[1, %line] $= "" ? "" : $TeamRank[1, %line].name;
      %plyr2 = $TeamRank[2, %line] $= "" ? "" : $TeamRank[2, %line].name;
      messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:50> %1</clip><lmargin%%:50><clip%%:50> %2</clip>', %plyr1, %plyr2 );
   }

   // Show observers:
   %count = ClientGroup.getCount();
   %header = false;
   for ( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject( %i );
      if ( %cl.team <= 0 )
      {
         if ( !%header )
         {
            messageClient( %client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>OBSERVERS<spop>' );
            %header = true;
         }

         messageClient( %client, 'MsgDebriefAddLine', "", ' %1', %cl.name );
      }
   }
}

function WarfareGame::updateScoreHud(%game, %client, %tag)
{
      // Send header:
      messageClient( %client, 'SetScoreHudHeader', "", '<tab:15,315>\t%1<rmargin:260><just:right>%2<rmargin:560><just:left>\t%3<just:right>%4', 
            %game.getTeamName(1), $TeamScore[1], %game.getTeamName(2), $TeamScore[2] );

   // Send subheader:
   messageClient( %client, 'SetScoreHudSubheader', "", '<tab:15,315>\t%1 (%2)\t%3 (%4)',
         $TeamName[1], $TeamRank[1, count], $TeamName[2], $TeamRank[2, count] );

   %index = 0;
   while (true)
   {
      if (%index >= $TeamRank[1, count] && %index >= $TeamRank[2, count])
         break;

      //get the team1 client info
      %team1Client = "";
      %team1ClientScore = "";
      %col1Style = "";
      if (%index < $TeamRank[1, count])
      {
         %team1Client = $TeamRank[1, %index];
         %team1ClientScore = %team1Client.score $= "" ? 0 : %team1Client.score;
         if ( %team1Client == %client )
            %col1Style = "<color:dcdcdc>";
      }

      //get the team2 client info
      %team2Client = "";
      %team2ClientScore = "";
      %col2Style = "";
      if (%index < $TeamRank[2, count])
      {
         %team2Client = $TeamRank[2, %index];
         %team2ClientScore = %team2Client.score $= "" ? 0 : %team2Client.score;
         if ( %team2Client == %client )
            %col2Style = "<color:dcdcdc>";
      }


      //if the client is not an observer, send the message
      if (%client.team != 0)
      {
         messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20,320>\t<spush>%3<clip:200>%1</clip><spop>\t%4<clip:200>%2</clip>', 
               %team1Client.name, %team2Client.name, %col1Style, %col2Style );
      }
      //else for observers, create an anchor around the player name so they can be observed
      else
      {
         messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20,320>\t<spush>%3<clip:200><a:gamelink\t%5>%1</a></clip><spop>\t%4<clip:200><a:gamelink\t%6>%2</a></clip>', 
               %team1Client.name, %team2Client.name, %col1Style, %col2Style, %team1Client, %team2Client );
      }

      %index++;
   }

   // Tack on the list of observers:
   %observerCount = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.team == 0)
         %observerCount++;
   }

   if (%observerCount > 0)
   {
	   messageClient( %client, 'SetLineHud', "", %tag, %index, "");
      %index++;
		messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
      %index++;
      for (%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         //if this is an observer
         if (%cl.team == 0)
         {
            %obsTime = getSimTime() - %cl.observerStartTime;
            %obsTimeStr = %game.formatTime(%obsTime, false);
		      messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150>%1</clip><rmargin:260><just:right>%2',
		                     %cl.name, %obsTimeStr );
            %index++;
         }
      }
   }

   //clear the rest of Hud so we don't get old lines hanging around...
   messageClient( %client, 'ClearHud', "", %tag, %index );
}

function WarfareGame::clientMissionDropReady(%game, %client)
{
   messageClient(%client, 'MsgClientReady', "", CTFGame);
   %game.resetScore(%client);

   for(%i = 1; %i <= %game.numTeams; %i++) 
   {
      messageClient(%client, 'MsgCTFAddTeam', "", %i, $teamName[%i], "", $TeamScore[%i]);
      messageClient(%client, 'MsgTeamScoreIs', "", %i, $TeamScore[%i]);
   }

   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 

   DefaultGame::clientMissionDropReady(%game, %client);
}

function WarfareGame::generateQuickPass(%game) {
   // built-in quick pass forcefields
   %pzGroup = nameToID("MissionCleanup/PZones");
   if(%pzGroup > 0) {
      %ffp = -1;
      for(%i = 0; %i < %pzGroup.getCount(); %i++) {
         %pz = %pzGroup.getObject(%i);

         if(%pz.ffield.quickPass == 1) {
            %pz.delete();
            %i--;
         }
      }
   }
}


function WarfareGame::genOnRepaired(%game, %obj, %objName)
{
}

function WarfareGame::stationOnRepaired(%game, %obj, %objName)
{
}

function WarfareGame::sensorOnRepaired(%game, %obj, %objName)
{
}

function WarfareGame::turretOnRepaired(%game, %obj, %objName)
{
}

function WarfareGame::vStationOnRepaired(%game, %obj, %objName)
{
}

function WarfareGame::vehicleDestroyed(%game, %obj, %objName)
{
}

function WarfareGame::applyConcussion(%game, %player)
{
}

function WarfareGame::initScript(%game) {
   // SCRIPTS SHOULD OVERLOAD THIS FUNCTION WHEN THEY WANT TO SET SOMETHING UP AT THE BEGINNING OF ALL ROUNDS
   // this particular function is called right before the countdown

   // if you need to set something as the countdown ends (like a time-based event) use WarfareGame::startMatch instead
}