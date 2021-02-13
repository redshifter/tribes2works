//--- GAME RULES BEGIN ---
//One team defends base, other team tries to conquer it as quickly as possible
//Game has two rounds: Round 1 ends when base is conquered or time runs out
//Round 2: Teams switch sides, play again -- to win, attackers MUST beat the time set by the attackers in Round 1
//Touching base switch conquers base
//--- GAME RULES END ---

//Siege type script for TRIBES 2
//
//The two teams each take a turn at defense and offense.
//
//If initial defending team's objective is captured, then roles switch
//and new offense team gets same amount of time to attempt to capture the
//objective.
//
//If time runs out before initial defending team's objective is captured,
//then roles switch and new offense team has to try to capture the
//objective before time runs out.
//
//The winner is either the team who captures the objective in least amount of time.
//
// In the mission file, Team 1 will be offense team, and team 2 will be the defense team.
// When the game actually starts, either team could start on offense, and the objects must
// have their team designation set accordingly.
//
// This mission type doesn't have a scoreLimit because, well, it really doesn't
// need one or lend itself to one.

// ai support
exec("scripts/aiSiege.cs");

package SiegeGame
{
   function ShapeBaseData::onDestroyed(%data, %obj, %prevstate)
   {
      %scorer = %obj.lastDamagedBy;
      if(!isObject(%scorer))
         return;

      if((%scorer.getType() & $TypeMasks::GameBaseObjectType) && %scorer.getDataBlock().catagory $= "Vehicles")
      {
         %name = %scorer.getDatablock().getName();
         if(%name $= "BomberFlyer" || %name $= "AssaultVehicle")
            %gunnerNode = 1;
         else
            %gunnerNode = 0;

         if(%scorer.getMountNodeObject(%gunnerNode))
         {
            %destroyer = %scorer.getMountNodeObject(%gunnerNode).client;
            %scorer = %destroyer;
            %damagingTeam = %scorer.team;
         }
      }
      else if(%scorer.getClassName() $= "Turret")
      {
         if(%scorer.getControllingClient())
         {
            %destroyer = %scorer.getControllingClient();
            %scorer = %destroyer;
            %damagingTeam = %scorer.team;
         }
         else
            %scorer = %scorer.owner;
      }
      if(!%damagingTeam)
         %damagingTeam = %scorer.team;

      if(%damagingTeam != %obj.team)
      {
         if(!%obj.soiledByEnemyRepair)
         {
            Game.awardScoreStaticShapeDestroy(%scorer, %obj);
         }
      }
      else
      {
         if(!%obj.getDataBlock().deployedObject)
            Game.awardScoreTkDestroy(%scorer, %obj);

         return;
      }
   }

   function FlipFlop::objectiveInit(%data, %flipflop)
   {
      Game.regObjective(%flipflop);
      setTargetSkin(%flipflop.getTarget(), $teamSkin[0]);
   }

   function FlipFlop::playerTouch(%data, %flipflop, %player)
   {
      if(%player.team != Game.offenseTeam)
         return;

      %defTeam = Game.offenseTeam == 1 ? 2 : 1;
      Game.capPlayer[Game.offenseTeam] = stripChars( getTaggedString( %player.client.name ), "\cp\co\c6\c7\c8\c9" );

      // Let the observers know:
      messageTeam( 0, 'MsgSiegeTouchFlipFlop', '\c2%1 captured the %2 base!~wfx/misc/flipflop_taken.wav', %player.client.name, $TeamName[%defTeam] );
      // Let the teammates know:
      messageTeam( %player.team, 'MsgSiegeTouchFlipFlop', '\c2%1 captured the %2 base!~wfx/misc/flipflop_taken.wav', %player.client.name, $TeamName[%defTeam] );
      // Let the other team know:
      %losers = %player.team == 1 ? 2 : 1;
      messageTeam( %losers, 'MsgSiegeTouchFlipFlop', '\c2%1 captured the %2 base!~wfx/misc/flipflop_lost.wav', %player.client.name, $TeamName[%defTeam]);

      logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") captured team "@%defTeam@" base");
      Game.allObjectivesCompleted();
   }

   function StaticShapeData::onDisabled(%data, %obj, %prevState)
   {
      %obj.wasDisabled = true;
      Parent::onDisabled(%data, %obj, %prevState);
      if(%obj.waypoint)
         game.switchWaypoint(%obj.waypoint);
   }

   function StaticShapeData::onEnabled(%data, %obj, %prevState)
   {
      if(%obj.waypoint)
         game.switchWaypoint(%obj.waypoint);

      if(%obj.isPowered())
         %data.onGainPowerEnabled(%obj);

      Parent::onEnabled(%data, %obj, %prevState);
   }

   function RepairGunImage::onRepair(%this, %obj, %slot)
   {
      Parent::onRepair(%this, %obj, %slot);
      %target = %obj.repairing;
      if(%target && %target.team != %obj.team)
      {
         %target.soiledByEnemyRepair = true;
      }
   }

   function Observer::onTrigger(%data,%obj,%trigger,%state)
   {
      if (%state == 0)
         return;

      if (!Game.ObserverOnTrigger(%data, %obj, %trigger, %state))
         return;

      if (%trigger >= 4)
         return;

      %client = %obj.getControllingClient();
      if (%client == 0)
         return;

      switch$ (%obj.mode)
      {
         case "pre-game":
            if(Game.firstHalf)
            {
               if(!$Host::TournamentMode || $CountdownStarted)
                  return;

               if(%client.notReady)
               {
                  %client.notReady = "";
                  MessageAll( 0, '\c1%1 is READY.', %client.name );
                  if(%client.notReadyCount < 3)
                     centerprint( %client, "\nWaiting for match start (FIRE if not ready)", 0, 3);
                  else 
                     centerprint( %client, "\nWaiting for match start", 0, 3);
               }
               else
               {
                  %client.notReadyCount++;
                  if(%client.notReadyCount < 4)
                  {
                     %client.notReady = true;
                     MessageAll( 0, '\c1%1 is not READY.', %client.name );
                     centerprint( %client, "\nPress FIRE when ready.", 0, 3 );
                  }
                  return;
               }
               Game.checkMatchStart();
            }
            else
               Parent::onTrigger(%data,%obj,%trigger,%state);

         default:
            Parent::onTrigger(%data,%obj,%trigger,%state);
      }
   }
};

//--------- Siege SCORING INIT ------------------
function SiegeGame::initGameVars(%game)
{
   %game.SCORE_PER_SUICIDE = 0; 
   %game.SCORE_PER_TEAMKILL = 0;
   %game.SCORE_PER_DEATH = 0;  
   %game.SCORE_PER_KILL = 0;  
   %game.SCORE_PER_TURRET_KILL = 0;

   %game.SCORE_PER_TK_DESTROY = -5;

   %game.SCORE_PER_REPAIR_SOLAR = 4;
   %game.SCORE_PER_DESTROY_SOLAR = 6;
   %game.SCORE_PER_REPAIR_GEN = 8;
   %game.SCORE_PER_DESTROY_GEN = 10;
   %game.SCORE_PER_GEN_DEFEND = 5;
   %game.RADIUS_GEN_DEFENSE = 20;

   %game.SCORE_PER_FLIPFLOP_DEFEND = 5;
   %game.RADIUS_FLIPFLOP_DEFENSE = 20;
}

function SiegeGame::regObjective(%game, %object)
{
   %objSet = nameToID("MissionCleanup/Objectives");
   if(!isObject(%objSet))
   {
      %objSet = new SimSet("Objectives");
      MissionCleanup.add(%objSet);
   }
   %objSet.add(%object);
}

function SiegeGame::claimFlipflopResources(%game, %flipflop, %team)
{
   // equipment shouldn't switch teams when flipflop is touched
}

function SiegeGame::missionLoadDone(%game)
{
   if( $Host::timeLimit == 0 )
      $Host::timeLimit = 999;

   //default version sets up teams - must be called first...
   DefaultGame::missionLoadDone(%game);

   //clear the scores
   $teamScore[1] = 0;
   $teamScore[2] = 0;

   //decide which team is starting first
   if (getRandom() > 0.5)
   {
      %game.offenseTeam = 1;
      %defenseTeam = 2;
   }
   else
   {
      %game.offenseTeam = 2;
      %defenseTeam = 1;
   }

   //send the message
   messageAll('MsgSiegeStart', '\c2Team %1 is starting on offense', $teamName[%game.offenseTeam]);

   //if the first offense team is team2, switch the object team designation
   if (%game.offenseTeam == 2)
   {
      %group = nameToID("MissionGroup/Teams");
      %group.swapTeams();
      // search for vehicle pads also
      %mcg = nameToID("MissionCleanup");
      %mcg.swapVehiclePads();
   }

   //also ensure the objectives are all on the defending team
   %objSet = nameToId("MissionCleanup/Objectives");
   for(%j = 0; %j < %objSet.getCount(); %j++) 
   {
      %obj = %objSet.getObject(%j);
      %obj.team = %defenseTeam;
      setTargetSensorGroup(%obj.getTarget(), %defenseTeam);
   }

   //indicate we're starting the game from the beginning...
   %game.firstHalf = true;
   %game.timeLimitMS = $Host::TimeLimit * 60 * 1000;
   %game.secondHalfCountDown = false;
   %game.capPlayer[1] = "";
   %game.capPlayer[2] = "";

   // save off turret bases' original barrels
   %game.checkTurretBases();

   // add objective waypoints
   %game.findObjectiveWaypoints();

   MissionGroup.setupPositionMarkers(true);
}

function SiegeGame::checkTurretBases(%game)
{
   %mGroup = nameToID("MissionGroup/Teams");
   %mGroup.findTurretBase();
}

function SimGroup::findTurretBase(%this)
{
   for (%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).findTurretBase();
}

function InteriorInstance::findTurretBase(%this)
{
   // sorry, we're not looking for interiors
}

function AIObjective::findTurretBase(%this)
{
   // prevent console error spam
}

function TSStatic::findTurretBase(%this)
{
   // prevent console error spam
}

function GameBase::findTurretBase(%this)
{
   // apparently, the initialBarrel attribute gets overwritten whenever the
   // barrel gets replaced.  :( So we have to save it again under "originalBarrel".
   if(%this.getDatablock().getName() $= "TurretBaseLarge")
      %this.originalBarrel = %this.initialBarrel;
}

function TSStatic::findTurretBase(%this)
{
   // prevent console error spam
}

function SiegeGame::selectSpawnSphere(%game, %team)
{ 
   //for siege, the team1 drops are offense, team2 drops are defense
   %sphereTeam = %game.offenseTeam == %team ? 1 : 2;

   return DefaultGame::selectSpawnSphere(%game, %sphereTeam);
}

function SiegeGame::startMatch(%game)
{
   DefaultGame::startMatch( %game );

   %game.startTimeMS = getSimTime();
   
   // schedule first timeLimit check for 20 seconds
   %game.timeSync = %game.schedule( 20000, "checkTimeLimit");
   %game.timeThread = %game.schedule( %game.timeLimitMS, "timeLimitReached");
   //updateClientTimes(%game.timeLimitMS);
   messageAll('MsgSystemClock', "", $Host::TimeLimit, %game.timeLimitMS);
   
//    %count = ClientGroup.getCount();
//    for ( %i = 0; %i < %count; %i++ )
//    {
//       %cl = ClientGroup.getObject( %i );
//       if ( %cl.team == %game.offenseTeam )
//          centerPrint( %cl, "\nTouch the enemy control switch to capture their base!", 5, 3 );
//       else
//          centerPrint( %cl, "\nPrevent the enemy from touching your control switch!", 5, 3 );   
//    }

   //make sure the AI is started
   AISystemEnabled(true);
}

function SiegeGame::allObjectivesCompleted(%game)
{
   Cancel( %game.timeSync );
   Cancel( %game.timeThread );
   cancelEndCountdown();

   //store the elapsed time in the teamScore array...
   $teamScore[%game.offenseTeam] = getSimTime() - %game.startTimeMS;
   messageAll('MsgSiegeCaptured', '\c2Team %1 captured the base in %2!', $teamName[%game.offenseTeam], %game.formatTime($teamScore[%game.offenseTeam], true));

   //set the new timelimit
   %game.timeLimitMS = $teamScore[%game.offenseTeam];

   if (%game.firstHalf)
   {
      // it's halftime, let everyone know
      messageAll( 'MsgSiegeHalftime' );
   }
   else
   {
      // game is over
      messageAll('MsgSiegeMisDone', '\c2Mission complete.');
   }
   logEcho("objective completed in "@%game.timeLimitMS);

   //setup the second half...
   // MES -- per MarkF, scheduling for 0 seconds will prevent player deletion-related crashes
   %game.schedule(0, halftime, 'objectives');
}

function SiegeGame::timeLimitReached(%game)
{
   cancel( %game.timeThread );
   cancel( %game.timeSync );

   // if time has run out, the offense team gets no score (note, %game.timeLimitMS doesn't change)
   $teamScore[%game.offenseTeam] = 0;
   messageAll('MsgSiegeFailed', '\c2Team %1 failed to capture the base.', $teamName[%game.offenseTeam]);
   
   if (%game.firstHalf)
   {
      // it's halftime, let everyone know
      messageAll( 'MsgSiegeHalftime' );
   }
   else
   {
      // game is over
      messageAll('MsgSiegeMisDone', '\c2Mission complete.');
   }
   
   logEcho("time limit reached");
   %game.halftime('time');
}

function SiegeGame::checkTimeLimit(%game)
{
   //if we're counting down to the beginning of the second half, check back in 
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
      messageAll('MsgSystemClock', "", $Host::TimeLimit, %curTimeLeftMS);
   }
}

function SiegeGame::halftime(%game, %reason)
{
   //stop the game and the bots
   $MatchStarted = false;
   AISystemEnabled(false);
   $countdownStarted = false; // z0dd - ZOD, 5/12/04. Allow players to switch spawn points. 
   if (%game.firstHalf)
   {
      //switch the game variables
      %game.firstHalf = false;
      %oldOffenseTeam = %game.offenseTeam;
      if (%game.offenseTeam == 1)
         %game.offenseTeam = 2;
      else
         %game.offenseTeam = 1;

      //send the message
      messageAll('MsgSiegeRolesSwitched', '\c2Team %1 is now on offense.', $teamName[%game.offenseTeam], %game.offenseTeam);

      //reset stations and vehicles that players were using
      %game.resetPlayers();
      // zero out the counts for deployable items (found in defaultGame.cs)
      %game.clearDeployableMaxes();

      // z0dd - ZOD, 5/17/02. Clean up deployables triggers, function in supportClassic.cs
      cleanTriggers(nameToID("MissionCleanup/Deployables"));

      // clean up the MissionCleanup group - note, this includes deleting all the player objects
      %clean = nameToID("MissionCleanup");
      %clean.housekeeping();

      // Non static objects placed in original position
      resetNonStaticObjPositions();

      // switch the teams for objects belonging to the teams
      %group = nameToID("MissionGroup/Teams");
      %group.swapTeams();
      // search for vehicle pads also
      %mcg = nameToID("MissionCleanup");
      %mcg.swapVehiclePads();

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
            if ( %client.team == %oldOffenseTeam )
            {
               if ( $teamScore[%oldOffenseTeam] > 0 )
                  messageClient( %client, 'MsgSiegeResult', "", '%1 captured the %2 base in %3!', %game.capPlayer[%oldOffenseTeam], $teamName[%game.offenseTeam], %game.formatTime( $teamScore[%oldOffenseTeam], true ) );
               else
                  messageClient( %client, 'MsgSiegeResult', "", 'Your team failed to capture the %1 base.', $teamName[%game.offenseTeam] );   
            }
            else if ( $teamScore[%oldOffenseTeam] > 0 )
               messageClient( %client, 'MsgSiegeResult', "", '%1 captured your base in %3!', %game.capPlayer[%oldOffenseTeam], %game.formatTime( $teamScore[%oldOffenseTeam], true ) );
            else
               messageClient( %client, 'MsgSiegeResult', "", 'Your team successfully held off team %1!', $teamName[%oldOffenseTeam] );   
            
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

            commandToClient( %client, 'SetHalftimeClock', $Host::Siege::Halftime / 60000 );

            // Get the HUDs right:
            commandToClient( %client, 'setHudMode', 'SiegeHalftime' );
            commandToClient( %client, 'ControlObjectReset' );

            clientResetTargets(%client, true);
            %client.notReady = true;
         }
      }
      %game.schedule( $Host::Siege::Halftime, halftimeOver );
   }
   else
   {
      // let's wrap it all up
      %game.gameOver();
      cycleMissions();
   }
}

function SiegeGame::halftimeOver( %game )
{
   // drop all players into mission
   %game.dropPlayers();

   //setup the AI for the second half
   %game.aiHalfTime();

   //redo the objective waypoints
   %game.findObjectiveWaypoints();

   // Players are now spawned, now we want to check to start the countdown
   %game.checkMatchStart();
}

function SiegeGame::dropPlayers( %game )
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
            %client.observerMode = "pregame";
            %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
            %client.setControlObject( %client.camera );
            // z0dd - ZOD, 5/12/04. Was false, we need this true for spawn position changing.
            if( $Host::TournamentMode )
               %client.notReady = true;
            else
               %client.notReady = flase;
         }
      }
   }      
}

function SiegeGame::checkMatchStart(%game)
{
   // Ripped from server functions
   if( $Host::TournamentMode )
   {
      if( $CountdownStarted || $matchStarted )
         return;

      // loop through all the clients and see if any are still notready
      %playerCount = 0;
      %notReadyCount = 0;
      %count = ClientGroup.getCount();
      for( %i = 0; %i < %count; %i++ )
      {
         %cl = ClientGroup.getObject(%i);
         if(%cl.camera.mode $= "pickingTeam")
         {
            %notReady[%notReadyCount] = %cl;
            %notReadyCount++;
         }   
         else if(%cl.camera.Mode $= "pre-game")
         {
            if(%cl.notready)
            {
               centerprint( %cl, "\nPress FIRE when ready.", 0, 3 );
               %notReady[%notReadyCount] = %cl;
               %notReadyCount++;
            }
            else
            {   
               %playerCount++;
            }
         }
         else if(%cl.camera.Mode $= "observer")
         {
            // this guy is watching
         }
      }
   
      if(%notReadyCount)
      {
         if(%notReadyCount == 1)
            MessageAll( 'msgHoldingUp', '\c1%1 is holding things up!', %notReady[0].name);
         else if(%notReadyCount < 4)
         {
            for(%i = 0; %i < %notReadyCount - 2; %i++)
               %str = getTaggedString(%notReady[%i].name) @ ", " @ %str;

            %str = "\c2" @ %str @ getTaggedString(%notReady[%i].name) @ " and " @ getTaggedString(%notReady[%i+1].name) 
                     @ " are holding things up!";

            MessageAll( 'msgHoldingUp', %str );
         }
         return;
      }

      if(%playerCount != 0)
      {
         %count = ClientGroup.getCount();
         for( %i = 0; %i < %count; %i++ )
         {
            %cl = ClientGroup.getObject(%i);
            %cl.notready = "";
            %cl.notReadyCount = "";
            ClearCenterPrint(%cl);
            ClearBottomPrint(%cl);
         }
      
         if ( %game.scheduleVote !$= "" && %game.voteType $= "VoteMatchStart") 
         {
            messageAll('closeVoteHud', "");
            cancel(%game.scheduleVote);
            %game.scheduleVote = "";
         }
         %game.halfTimeCountDown( $Host::warmupTime );
      }
   }
   else
   {
      %readyToStart = false;
      for(%clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++)
      {   
         %client = ClientGroup.getObject(%clientIndex);
         if(%client.isReady)
         {   
            %readyToStart = true;
            break;
         }
      }

      if(%readyToStart || ClientGroup.getCount() < 1)  
         %game.halfTimeCountDown( $Host::warmupTime );
      else  
         %game.schedule(1500, "checkMatchStart");     
   }
}

function SiegeGame::halfTimeCountDown(%game, %time)
{
   %game.secondHalfCountDown = true;
   $MatchStarted = false;
   $countdownStarted = true; // z0dd - ZOD, 5/12/04. Allow players to switch spawn points.
   %timeMS = %time * 1000;
   %game.schedule(%timeMS, "startSecondHalf");
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

function SiegeGame::startSecondHalf(%game)
{
   $MatchStarted = true;
   %game.secondHalfCountDown = false;
   $countdownStarted = false; // z0dd - ZOD, 5/12/04. Allow players to switch spawn points.

   MessageAll('MsgMissionStart', "\c2Match started");

   // set the start time.  
   //the new %game.timeLimitMS would have been set by timeLimitReached() or allObjectivesCompleted()
   %game.startTimeMS = getSimTime();

   %game.timeThread = %game.schedule(%game.timeLimitMS, "timeLimitReached");
   if (%game.timeLimitMS > 20000)
      %game.timeSync = %game.schedule(20000, "checkTimeLimit");
   else
      %game.timeSync = %game.schedule(%game.timeLimitMS, "checkTimeLimit");

   logEcho("start second half");

   EndCountdown(%game.timeLimitMS);

   // set all clients control to their player
   %count = ClientGroup.getCount();
   for( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject(%i);
      if (!isObject(%cl.player))
         commandToClient(%cl, 'setHudMode', 'Observer');
      else
      {
         %cl.observerMode = "";
         %cl.setControlObject( %cl.player );
         commandToClient(%cl, 'setHudMode', 'Standard');
//          if ( %client.team == %game.offenseTeam )
//             centerPrint( %cl, "\nTouch the enemy control switch to capture their base!", 5, 3 );
//          else
//             centerPrint( %cl, "\nPrevent the enemy from touching your control switch!", 5, 3 );   
      }
   }
   
   //now synchronize everyone's clock
   updateClientTimes(%game.timeLimitMS);

   //start the bots up again...
   AISystemEnabled(true);
}

function SiegeGame::resetPlayers(%game)
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

function SimGroup::housekeeping(%this)
{
   // delete selectively in the MissionCleanup group
   %count = %this.getCount();
   // have to do this backwards or only half the objects will be deleted
   for(%i = (%count - 1); %i > -1; %i--)
   {
      %detritus = %this.getObject(%i);
      %class = %detritus.getClassName();
      if(%class $= "SimSet")
      {
         // I don't think there are any simsets we want to delete
      }
      else if(%detritus.getName() $= "PZones")
      {
         // simgroup of physical zones for force fields
         // don't delete them
      }
      //else if (%class $= "ScriptObject")
      //{
      // // this will be the game type object.
      // // DEFINITELY don't want to delete this.
      //}
      else if(%detritus.getName() $= PosMarker)
      {
         //Needed to reset non static objects...
      }
      else if((%detritus.getName() $= "TeamDrops1") || (%detritus.getName() $= "TeamDrops2"))
      {
         // this will actually be a SimSet named TeamDropsN (1 or 2)
         // don't want to delete the spawn sphere groups, so do nothing
      }
      else if (%detritus.getName() $= "PlayerListGroup")
      {
         // we don't want to delete PlayerListGroup (SimGroup)
      }
      else if (%detritus.getDatablock().getName() $= "stationTrigger")
      {
         //we don't want to delete triggers attached to stations except the MPB station (important)
         if(%detritus.station.getDataBlock().getName() $= "MobileInvStation")
            %detritus.delete();
      }
      else if (%detritus.getDatablock().getName() $= "markerTrigger")
      {
         //ZOD: Stop deleting @$@#%^$%#& You fargin @#!%@$#^%@!!
      }
      else if (%detritus.getDatablock().getName() $= "StationVehicle")
      {
         // vehicle stations automatically get placed in MissionCleanup in a
         // position near the vehicle pad. Don't delete it.
      }
      else if (%detritus.getDatablock().getName() $= "MPBTeleporter") // z0dd - ZOD: Don't delete teleporters either!
      {
         // teleporter stations automatically get placed in MissionCleanup in a
         // position near the vehicle pad. Don't delete it.
      }
      else if (%class $= "Camera")
      {
         // Cameras should NOT be deleted
      }
      //else if((%class $= "FlyingVehicle" || %class $= "WheeledVehicle" || %class $= "HoverVehicle") && (%detritus.respawn == 1))
      //{
         // We must delete vehciles, respawning or otherwise.
      //}
      else
      {
         // this group of stuff to be deleted should include:
         // mines, deployed objects, projectiles, explosions, corpses,
         // players, and the like.
         %detritus.delete();
      }
   }
}

function SiegeGame::groupSwapTeams(%game, %this)
{
   for(%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).swapTeams();
}

function SiegeGame::objectSwapTeams(%game, %this)
{
   %defTeam = %game.offenseTeam == 1 ? 2 : 1;

   if(%this.getDatablock().getName() $= "Flipflop") 
   {
      if(getTargetSensorGroup(%this.getTarget()) != %defTeam)
      {
         setTargetSensorGroup(%this.getTarget(), %defTeam);
         %this.team = %defTeam;
      }
   }
   else
   {
      if(%this.getTarget() != -1)
      {
         if(getTargetSensorGroup(%this.getTarget()) == %game.offenseTeam)
         {
            setTargetSensorGroup(%this.getTarget(), %defTeam);
            %this.team = %defTeam;
         }
         else if(getTargetSensorGroup(%this.getTarget()) == %defTeam)
         {
            setTargetSensorGroup(%this.getTarget(), %game.offenseTeam);
            %this.team = %game.offenseTeam;
         }
      }
      if(%this.getClassName() $= "Waypoint")
      {
         if(%this.team == %defTeam)
            %this.team = %game.offenseTeam;
         else if(%this.team == %game.offenseTeam)
            %this.team = %defTeam;
      }
   }
}

function SiegeGame::groupSwapVehiclePads(%game, %this)
{
   for(%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).swapVehiclePads();
}

function SiegeGame::objectSwapVehiclePads(%game, %this)
{
   %defTeam = %game.offenseTeam == 1 ? 2 : 1;

   if(%this.getDatablock().getName() $= "StationVehicle")
   {
      if(%this.getTarget() != -1)
      {
         // swap the teams of both the vehicle pad and the vehicle station
         if(getTargetSensorGroup(%this.getTarget()) == %game.offenseTeam)
         {
            setTargetSensorGroup(%this.getTarget(), %defTeam);
            %this.team = %defTeam;
            setTargetSensorGroup(%this.pad.getTarget(), %defTeam);
            %this.pad.team = %defTeam;
            // z0dd - ZOD, 4/20/02. Switch the teleporter too.
            setTargetSensorGroup(%this.teleporter.getTarget(), %defTeam);
            %this.teleporter.team = %defTeam;
         }
         else if(getTargetSensorGroup(%this.getTarget()) == %defTeam)
         {
            setTargetSensorGroup(%this.getTarget(), %game.offenseTeam);
            %this.team = %game.offenseTeam;
            setTargetSensorGroup(%this.pad.getTarget(), %game.offenseTeam);
            %this.pad.team = %game.offenseTeam;
            // z0dd - ZOD, 4/20/02. Switch the teleporter too.
            setTargetSensorGroup(%this.teleporter.getTarget(), %game.offenseTeam);
            %this.teleporter.team = %game.offenseTeam;
         }
      }
   }
}

function SiegeGame::restoreObjects(%game)
{
   // restore all the "permanent" mission objects to undamaged state
   %group = nameToID("MissionGroup/Teams");
   // SimGroup::objectRestore is defined in DefaultGame.cs -- it simply calls
   // %game.groupObjectRestore
   %group.objectRestore();
}

function SiegeGame::groupObjectRestore(%game, %this)
{
   for(%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).objectRestore();
}

function SiegeGame::shapeObjectRestore(%game, %object)
{
   //if(%object.getDatablock().getName() $= FlipFlop)
   //{
   // messageAll('MsgSiegeObjRestore', "", %object.number, true);
   //}
   //else if(%object.getDamageLevel())
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

function InteriorInstance::objectRestore(%this)
{
   // avoid console error spam
}

function Trigger::objectRestore(%this)
{
   // avoid console error spam
}

function TSStatic::objectRestore(%this)
{
   // avoid console error spam
}

function ForceFieldBare::objectRestore(%this)
{
   // avoid console error spam
}

/////////////////////////////////////////////////////////////////////////////////////////
// Waypoint managing ////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

function siegeGame::findObjectiveWaypoints(%game, %group)
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

function siegeGame::initializeWaypointAtObjective(%game, %object)
{
   // out with the old...jic
   if ( %object.waypoint )
   {
      if ( isObject( %object.waypoint ) )
         %object.waypoint.delete();
      else
         %object.waypoint = "";   
   }

   if(%object.team == %game.offenseTeam)
      %team = %game.offenseTeam;
   else
      %team = (%game.offenseTeam == 1 ? 2 : 1);
	
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
      team = %team;
      name = getTaggedString(%object.nameTag) SPC %append;
   };
   MissionCleanup.add(%object.waypoint);
}

function siegeGame::switchWaypoint(%game, %waypoint)
{
   %team = %waypoint.team;
   %newTeam = (%team == 1 ? 2 : 1);
   %waypoint.team = %newTeam; 
}

function SiegeGame::gameOver(%game)
{
   //call the default
   DefaultGame::gameOver(%game);

   cancel(%game.timeThread);

   messageAll('MsgClearObjHud', "");
}

function SiegeGame::clientMissionDropReady(%game, %client)
{
   messageClient(%client, 'MsgClientReady', "", %game.class);

   for(%i = 1; %i <= %game.numTeams; %i++) 
   {
      %isOffense = (%i == %game.offenseTeam);
      messageClient(%client, 'MsgSiegeAddTeam', "", %i, $teamName[%i], %isOffense);
   }

   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 
   DefaultGame::clientMissionDropReady(%game, %client);
}

function SiegeGame::assignClientTeam(%game, %client, %respawn)
{
   DefaultGame::assignClientTeam(%game, %client, %respawn);
   
   // if player's team is not on top of objective hud, switch lines
   messageClient(%client, 'MsgCheckTeamLines', "", %client.team);
}

/////////////////////////////////////////////////////////////////////////////////////////
// Scoring functions ////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
function SiegeGame::resetScore(%game, %client)
{
   %client.score = 0;
   %client.kills = 0;
   %client.deaths = 0;
   %client.suicides = 0;
   %client.objScore = 0;
   %client.teamKills = 0;
   %client.turretKills = 0;
   %client.offenseScore = 0;
   %client.defenseScore = 0;

   %client.tkDestroys = 0;
   %client.genDestroys = 0;
   %client.solarDestroys = 0;
   %client.flipFlopDefends = 0;
   %client.genDefends = 0;
   %client.genRepairs = 0;
   %client.solarRepairs = 0;
   %client.outOfBounds = "";
}

function SiegeGame::recalcScore(%game, %cl)
{
   %killValue = %cl.kills * %game.SCORE_PER_KILL;
   %deathValue = %cl.deaths * %game.SCORE_PER_DEATH;

   if (%killValue - %deathValue == 0)
      %killPoints = 0;
   else
      %killPoints = (%killValue * %killValue) / (%killValue - %deathValue);

   %cl.offenseScore = %killPoints +
                      %cl.teamKills     * %game.SCORE_PER_TEAMKILL +
                      %cl.genDestroys   * %game.SCORE_PER_DESTROY_GEN +
                      %cl.solarDestroys * %game.SCORE_PER_DESTROY_SOLAR +
                      %cl.objScore;

   %cl.defenseScore = %cl.turretKills     * %game.SCORE_PER_TURRET_KILL +
                      %cl.flipFlopDefends * %game.SCORE_PER_FLIPFLOP_DEFEND +
                      %cl.genDefends      * %game.SCORE_PER_GEN_DEFEND +
                      %cl.genRepairs      * %game.SCORE_PER_REPAIR_GEN +
                      %cl.solarRepairs    * %game.SCORE_PER_REPAIR_SOLAR;

   %cl.score = mFloor(%cl.offenseScore + %cl.defenseScore);
   %game.recalcTeamRanks(%cl);
}

function SiegeGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement)
{
   if(isObject(%implement))
   {
      if(%implement.getDataBlock().getName() $= "AssaultPlasmaTurret" || %implement.getDataBlock().getName() $= "BomberTurret")
         %clKiller = %implement.vehicleMounted.getMountNodeObject(1).client;
      else if(%implement.getDataBlock().catagory $= "Vehicles")
         %clKiller = %implement.getMountNodeObject(0).client;             
   }
   if(%game.testTurretKill(%implement))
   {        
      %game.awardScoreTurretKill(%clVictim, %implement);  
   }
   else if (%game.testKill(%clVictim, %clKiller))
   {
      %game.awardScoreKill(%clKiller);
      %game.awardScoreDeath(%clVictim);

      if (%game.testGenDefend(%clVictim, %clKiller))
         %game.awardScoreGenDefend(%clKiller);

      %flipflop = %game.testPlayerFFDefend(%clVictim, %clKiller);
      if (isObject(%flipflop))
         %game.awardScorePlayerFFDefend(%clKiller, %flipflop);	  		
   }
   else
   {        
     if (%game.testSuicide(%clVictim, %clKiller, %damageType))  //otherwise test for suicide
     {
       %game.awardScoreSuicide(%clVictim);
     }
     else
     {
        if (%game.testTeamKill(%clVictim, %clKiller)) //otherwise test for a teamkill
              %game.awardScoreTeamKill(%clVictim, %clKiller);
     }
   }        
}

function SiegeGame::testGenDefend(%game, %victimID, %killerID)
{
   InitContainerRadiusSearch(%victimID.plyrPointOfDeath, %game.RADIUS_GEN_DEFENSE, $TypeMasks::StaticShapeObjectType);
   %objID = containerSearchNext();
   while(%objID != 0)
   {
     %objType = %objID.getDataBlock().className;
     if ((%objType $= "generator") && (%objID.team == %killerID.team)) 
        return true;
     else
        %objID = containerSearchNext();
   }
   return false;
}

function SiegeGame::awardScoreGenDefend(%game, %killerID)
{
   %killerID.genDefends++;
   messageClient(%killerID, 'msgGenDef', '\c0You received a %1 point bonus for defending a generator.', %game.SCORE_PER_GEN_DEFEND);
   messageTeamExcept(%killerID, 'msgGenDef', '\c0Teammate %1 received a %2 point bonus for defending a generator.', %killerID.name, %game.SCORE_PER_GEN_DEFEND);
   %game.recalcScore(%cl);
   return %game.SCORE_PER_GEN_DEFEND;
}

function SiegeGame::testPlayerFFDefend(%game, %victimID, %killerID)
{
   if (!isObject(%victimId) || !isObject(%killerId) || %killerId.team <= 0)
      return -1;

   InitContainerRadiusSearch(%victimID.plyrPointOfDeath, %game.RADIUS_FLIPFLOP_DEFENSE, $TypeMasks::StaticShapeObjectType);
   %objID = containerSearchNext();   
   while(%objID != 0) 
   {
     if((%objID.getDataBlock().getName() $= "FlipFlop") && (%objID.team == %killerID.team)) 
        return %objID;
     else
        %objID = containerSearchNext();     
   }
   return -1;
}

function SiegeGame::awardScorePlayerFFDefend(%game, %cl, %flipflop)
{
   %cl.flipFlopDefends++;
   messageClient(%cl, 'msgFFDef', '\c0You received a %1 point bonus for defending %2.', %game.SCORE_PER_FLIPFLOP_DEFEND, %game.cleanWord(%flipflop.name));
   messageTeamExcept(%cl, 'msgFFDef', '\c0Teammate %1 received a %2 point bonus for defending %3', %cl.name, %game.SCORE_PER_FLIPFLOP_DEFEND, %game.cleanWord(%flipflop.name));
   %game.recalcScore(%cl);
   return %game.SCORE_PER_FLIPFLOP_DEFEND;
}

function SiegeGame::awardScoreTkDestroy(%game, %cl, %obj)
{
   %cl.tkDestroys++;
   messageTeamExcept(%cl, 'msgTkDes', '\c5Teammate %1 destroyed your team\'s %3 objective!', %cl.name, %game.cleanWord(%obj.getDataBlock().targetTypeTag));
   messageClient(%cl, 'msgTkDes', '\c0You have been penalized %1 points for destroying your teams equiptment.', %game.SCORE_PER_TK_DESTROY);
   %game.recalcScore(%cl);
   %game.shareScore(%cl, %game.SCORE_PER_TK_DESTROY);
}

function SiegeGame::awardScoreStaticShapeDestroy(%game, %cl, %obj)
{
   %dataName = %obj.getDataBlock().getName();
   switch$ ( %dataName )
   {
      case "GeneratorLarge":
         %cl.genDestroys++;
         %value = %game.SCORE_PER_DESTROY_GEN;
         %msgType = 'msgGenDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Generator!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy generator.';

      case "SolarPanel":
         %cl.solarDestroys++;
         %value = %game.SCORE_PER_DESTROY_SOLAR;
         %msgType = 'msgSolarDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Solar Panel!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy solar panel.';
   }
   teamDestroyMessage(%cl, 'MsgDestroyed', %tMsg, %cl.name, %obj.nameTag);
   messageClient(%cl, %msgType, %clMsg, %value, %dataName);
   %game.recalcScore(%cl);
}

function SiegeGame::testValidRepair(%game, %obj)
{
   if(!%obj.wasDisabled)
      return false;
   else if(%obj.lastDamagedByTeam == %obj.team)
      return false;
   else if(%obj.team != %obj.repairedBy.team)
      return false;
   else 
   {
      if(%obj.soiledByEnemyRepair)
         %obj.soiledByEnemyRepair = false;

      return true;
   }
}

function SiegeGame::objectRepaired(%game, %obj, %objName)
{
   %game.staticShapeOnRepaired(%obj, %objName);
   %obj.wasDisabled = false;
}

function SiegeGame::staticShapeOnRepaired(%game, %obj, %objName)
{
   if (%game.testValidRepair(%obj))
   {
      %repairman = %obj.repairedBy;
      %dataName = %obj.getDataBlock().getName();
      switch$ (%dataName)
      {
         case "GeneratorLarge":
            %repairman.genRepairs++;
            %score = %game.SCORE_PER_REPAIR_GEN;
            %tMsgType = 'msgGenRepaired';
            %msgType = 'msgGenRep';
            %tMsg = '\c0%1 repaired the %2 Generator!';
            %clMsg = '\c0You received a %1 point bonus for repairing a generator.';

         case "SolarPanel":
            %repairman.solarRepairs++;
            %score = %game.SCORE_PER_REPAIR_SOLAR;
            %tMsgType = 'msgsolarRepaired';
            %msgType = 'msgsolarRep';
            %tMsg = '\c0%1 repaired the %2 Solar Panel!';
            %clMsg = '\c0You received a %1 point bonus for repairing a solar panel.';

         case "StationInventory" or "StationAmmo":
            %tMsgType = 'msgStationRepaired';
            %tMsg = '\c0%1 repaired the %2 Station!';

         case "SensorLargePulse" or "SensorMediumPulse":
            %tMsgType = 'msgSensorRepaired';
            %tMsg = '\c0%1 repaired the %2 Pulse Sensor!';

         case "TurretBaseLarge":
            %tMsgType = 'msgTurretRepaired';
            %tMsg = '\c0%1 repaired the %2 Turret!';

         case "StationVehicle":
            %tMsgType = 'msgvstationRepaired';
            %tMsg = '\c0%1 repaired the Vehicle Station!';
      }
      teamRepairMessage(%repairman, %tMsgType, %tMsg, %repairman.name, %obj.nameTag);
      if(clMsg !$= "")
         messageClient(%repairman, %msgType, %clMsg, %score, %dataName);

      %game.recalcScore(%repairman);
   }
}

function SiegeGame::updateScoreHud(%game, %client, %tag)
{
   %timeElapsedMS = getSimTime() - %game.startTimeMS;
   %curTimeLeftMS = %game.timeLimitMS - %timeElapsedMS;

   if (!$MatchStarted)
      %curTimeLeftStr = %game.formatTime(%game.timelimitMS, false);
   else
      %curTimeLeftStr = %game.formatTime(%curTimeLeftMS, false);

   // Send header:
   if (%game.firstHalf)
      messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Team %1 has %2 to capture the base.', 
            $teamName[%game.offenseTeam], %curTimeLeftStr ); 
   else
      messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Team %1 must capture the base within %2 to win.', 
            $teamName[%game.offenseTeam], %curTimeLeftStr );

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

function SiegeGame::sendDebriefing( %game, %client )
{
   //if neither team captured
   %winnerName = "";
   if ( $teamScore[1] == 0 && $teamScore[2] == 0 )
      %winner = -1;

   //else see if team1 won
   else if ( $teamScore[1] > 0 && ( $teamScore[2] == 0 || $teamScore[1] < $teamScore[2] ) )
   {
      %winner = 1;
      %winnerName = $teamName[1];
   }

   //else see if team2 won
   else if ($teamScore[2] > 0 && ($teamScore[1] == 0 || $teamScore[2] < $teamScore[1]))
   {
      %winner = 2;
      %winnerName = $teamName[2];
   }

   //else see if it was a tie (right down to the millisecond - doubtful)
   else if ($teamScore[1] == $teamScore[2])
      %winner = 0;

   //send the winner message
   if (%winnerName $= 'Storm')
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.stowins.wav" );
   else if (%winnerName $= 'Inferno')
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.infwins.wav" );
   else
      messageClient( %client, 'MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav" );

   // Mission result:
   if (%winner > 0)
   {
      if (%winner == 1)
      {  
         if ($teamScore[2] == 0)
            messageClient(%client, 'MsgDebriefResult', "", '<just:center>Team %1 wins!', $TeamName[1]);
         else
         {
            %timeDiffMS = $teamScore[2] - $teamScore[1];
            messageClient(%client, 'MsgDebriefResult', "", '<just:center>Team %1 won by capturing the base %2 faster!', $TeamName[1], %game.formatTime(%timeDiffMS, true));
         }
      }
      else
      {
         if ($teamScore[1] == 0)
            messageClient(%client, 'MsgDebriefResult', "", '<just:center>Team %1 wins!', $TeamName[2]);
         else
         {
            %timeDiffMS = $teamScore[1] - $teamScore[2];
            messageClient(%client, 'MsgDebriefResult', "", '<just:center>Team %1 won by capturing the base %2 faster!', $TeamName[2], %game.formatTime(%timeDiffMS, true));
         }
      }
   }
   else
      messageClient( %client, 'MsgDebriefResult', "", '<just:center>The mission ended in a tie.' );

   // Game summary:
   messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>SUMMARY:<spop>' );
   %team1 = %game.offenseTeam == 1 ? 2 : 1;
   %team2 = %game.offenseTeam;
   if ( $teamScore[%team1] > 0 )
   {  
      %timeStr = %game.formatTime($teamScore[%team1], true);
      messageClient( %client, 'MsgDebriefAddLine', "", '<bitmap:bullet_2><lmargin:24>%1 captured the %2 base for Team %3 in %4.<lmargin:0>', %game.capPlayer[%team1], $TeamName[%team2], $TeamName[%team1], %timeStr);
   }
   else
      messageClient( %client, 'MsgDebriefAddLine', "", '<bitmap:bullet_2><lmargin:24>Team %1 failed to capture the base.<lmargin:0>', $TeamName[%team1]);

   if ( $teamScore[%team2] > 0 )
   {  
      %timeStr = %game.formatTime($teamScore[%team2], true);
      messageClient( %client, 'MsgDebriefAddLine', "", '<bitmap:bullet_2><lmargin:24>%1 captured the %2 base for Team %3 in %4.<lmargin:0>', %game.capPlayer[%team2], $TeamName[%team1], $TeamName[%team2], %timeStr);
   }
   else
      messageClient( %client, 'MsgDebriefAddLine', "", '<bitmap:bullet_2><lmargin:24>Team %1 failed to capture the base.<lmargin:0>', $TeamName[%team2]);

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

/////////////////////////////////////////////////////////////////////////////////////////

function SiegeGame::applyConcussion(%game, %player)
{
}

function SiegeGame::vehicleDestroyed(%game, %vehicle, %destroyer)
{
   // Console suppression
}

function SiegeGame::voteMatchStart( %game, %admin)
{
   %cause = "";
   %ready = forceTourneyMatchStart();
   if(%admin)
   {
      if(!%ready)
      {   
         messageClient( %client, 'msgClient', '\c2No players are ready yet.');
         return;
      }
      else
      {
         messageAll('msgMissionStart', '\c2The admin has forced the match to start.');
         %cause = "(admin)";
         if(%game.firstHalf)
            startTourneyCountdown();
         else
            %game.halfTimeCountDown( $Host::warmupTime );
      }
   }
   else
   {
      if(!%ready)
      {
         messageAll( 'msgClient', '\c2Vote passed to start match, but no players are ready yet.');
         return;
      }
      else
      {  
         %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
         if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100)) 
         {
            messageAll('MsgVotePassed', '\c2The match has been started by vote: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));  
            if(%game.firstHalf)
               startTourneyCountdown();
            else
               %game.halfTimeCountDown( $Host::warmupTime );
         }
         else
            messageAll('MsgVoteFailed', '\c2Start Match vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100)); 
      }
   }
   
   if(%cause !$= "")
      logEcho("start match "@%cause);
}
