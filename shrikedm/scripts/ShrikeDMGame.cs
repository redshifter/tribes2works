// --------------------------------------------------------
// Shrike Deathmatch v1.0
//
// Concepts and coding by Red Shifter
// --------------------------------------------------------

// DisplayName = Shrike Deathmatch

//--- GAME RULES BEGIN ---
//Shoot down enemy Shrikes
//Don't crash your own Shrike
//Points awarded for hits as well as kills
//--- GAME RULES END ---

$ShrikeDM::ServerVersion = 1.0;
// $ShrikeDM::ClientVersion = 0.00;  // It's not time to show up yet

// mappers need to use the GenerateShrikeSpawns() command
// there's no spawngraphs so don't bother building one

// if you want a more advanced auto-spawner, use 
// ShrikeDM_GenerateShrikeSpawns(%height, %spread, %space);
// it's harder to remember for a reason.  It is dangerous.

function ShrikeDMGame::initGameVars(%game)
{
	%game.SCORE_PER_KILL    =  8;
	%game.SCORE_PER_DEATH   = -2;
	%game.SCORE_PER_SUICIDE = -5;
	%game.SCORE_PER_HIT     =  1; // kill-stealers beware; you're not getting the full points!
}

//exec("scripts/aiShrikeDM.cs");
function ShrikeDMGame::onAIRespawn(%game, %client) {
   %client.spawnUseInv = false;
}

package ShrikeDMGame {
	
function AIVehicleMounted() {
	// remove console spam
}

function AICorpseAdded()
{
	// stop giving me this shit
}

function AIProcessEngagement()
{
	// come the fuck on, AI can't fly shrikes
}

function ScoutFlyer::onAdd(%this, %obj)
{
   VehicleData::onAdd(%this, %obj);
   %obj.mountImage(ScoutChaingunParam, 0);
   %obj.mountImage(ScoutChaingunImage, 2);
   %obj.mountImage(ScoutChaingunPairImage, 3);
   %obj.nextWeaponFire = 2;
   
   // do the activate thread immediately
   %obj.schedule(0, "playThread", $ActivateThread, "activate");
}

// kill the player if the vehicle dies
function VehicleData::onDestroyed(%data, %obj, %prevState) {
   // calls ShrikeDMGame::vehicleDestroyed(%data, %obj, %destroyer)
   // if we don't have a "lastDamagedBy" then call it yourself
   if( !%obj.lastDamagedBy ) {
      Game.vehicleDestroyed(%obj);
   }
   Parent::onDestroyed(%data, %obj, %prevState);
   
   // send the shrike to the moon
   //%data.onAvoidCollisions(%obj);

   // kill the player (if he's still linked to the shrike)
   if( isObject(%obj.player) )
      %obj.player.scriptKill(0);
}

function Armor::doDismount(%this, %obj, %forced) {
   if( !%forced ) {
      if( %obj.MakingBreakForItInShrikeDM $= "" ) {
         %obj.MakingBreakForItInShrikeDM = "vgx";
         Game.schedule(1000, repeatMessageAboutJumping, %obj);
         messageClient( %obj.client, 0, '\c2You can\'t leave your vehicle.~wfx/misc/misc.error.wav' );
      }
      return;
   }
}

function VehicleData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %theClient, %proj) {
   //error("damaged by obj-" @ %sourceObject @ " cl-" @ %sourceObject.player.client @ " dt-" @ %damageType);
   if( %targetObject.getDamageState() $= "Destroyed" )
      return;

   if( %damageType == $DamageType::ShrikeBlaster && isObject(%sourceObject.player.client) ) {
      %sourceObject.player.client.hits++;
      Game.recalcScore(%sourceObject.player.client);
   }
   
   Parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %theClient, %proj);
}

function FlyingVehicle::liquidDamage(%obj, %data, %damageAmount, %damageType)
{
   // lava is a little harder in this gametype...
   if(%obj.getDamageState() !$= "Destroyed") {
      %obj.lastDamagedBy = 0;
      %obj.applyDamage(10000);
      %obj.lDamageSchedule = "";
   }
}

function GenerateShrikeSpawns(%height, %doYouUnderstandMeYet) {
   %spread   = 120;
   %edgeSize = 20;

   if( %height $= "" || %doYouUnderstandMeYet !$= "" ) {
      error("Error: Wrong number of arguments.  Please fill in one variable for the desired height.");
      echo("Example Usage: GenerateShrikeSpawns(300);");
      return;
   }
   if( %height == 0 ) {
      error("Error: Height should not be set to zero.  Please change your height variable.");
      echo("Example Usage: GenerateShrikeSpawns(300);");
      return;
   }

   ShrikeDM_GenerateShrikeSpawns(%height, %spread, %edgeSize);
}

function ShrikeDM_GenerateShrikeSpawns(%height, %spread, %edgeSize, %doYouUnderstandMeYet) {
   if( %height $= "" || %spread $= "" || %edgeSize $= "" || %doYouUnderstandMeYet !$= "" ) {
      error("Error: Wrong number of arguments.  Use GenerateShrikeSpawns(); if you want a basic generator.");
      echo("ShrikeDM_GenerateSpawnSpheres(%height, %spread, %space);");
      echo("%height - The height that the spawnspheres will be placed.");
      echo("%spread - The amount of space between each spawn (must be 50 or higher).");
      echo("%space  - The amount of space from spawns to the mission bounds (must be 10 or higher).");
      return;
   }
   if( %spread < 50 ) {
      error("Your spread (argument 2) is too low.  Please use at least 50.");
      echo("ShrikeDM_GenerateSpawnSpheres(%height, %spread, %edgeSize);");
      return;
   }
   if( %edgeSize < 10 ) {
      error("Your edge space (argument 3) is too low.  Please use at least 10.");
      echo("ShrikeDM_GenerateSpawnSpheres(%height, %spread, %edgeSize);");
      return;
   }

   echo("Generating Spawn Spheres...");
   // create a ShrikeSpawns group if it does not exist
   %spawnList = nameToId("MissionGroup/ShrikeSpawns");

   if(%spawnList == -1) {
      %group = nameToID("MissionGroup");
      if(%group == -1) {
         error("- No MissionGroup?  What are you smoking?");
         error("Canceling job...");
         return;
      }
      
      echo("- ShrikeSpawns group added to the map");

      %spawnList = new SimGroup("ShrikeSpawns");
      %group.add(%spawnList);
   }
   
   echo("- Searching for mission area...");  
   if( !isObject(MissionArea) ) {
      error("No MissionArea?  What are you smoking?");
      error("Canceling job...");
      return;
   }
   %bounds = MissionArea.area;
   %boundsWest = firstWord(%bounds);
   %boundsNorth = getWord(%bounds, 1);
   %boundsEast = %boundsWest + getWord(%bounds, 2);
   %boundsSouth = %boundsNorth + getWord(%bounds, 3);
   %nodeCount = 0;
   %CenterX = %boundsWest + (getWord(%bounds, 2) / 2);
   %CenterY = %boundsNorth + (getWord(%bounds, 3) / 2);
   
   for( %i = %CenterX; %i > %boundsWest; %i -= %spread ) {
      ShrikeDM_addSpawnPoint(%spawnList, %i SPC (%boundsNorth - %edgeSize) SPC %height, %nodeCount); %nodeCount++;
      ShrikeDM_addSpawnPoint(%spawnList, %i SPC (%boundsSouth + %edgeSize) SPC %height, %nodeCount); %nodeCount++;
   }
   for( %i = %CenterX + %spread; %i < %boundsEast; %i += %spread ) {
      ShrikeDM_addSpawnPoint(%spawnList, %i SPC (%boundsNorth - %edgeSize) SPC %height, %nodeCount); %nodeCount++;
      ShrikeDM_addSpawnPoint(%spawnList, %i SPC (%boundsSouth + %edgeSize) SPC %height, %nodeCount); %nodeCount++;
   }
   for( %i = %CenterY; %i > %boundsNorth; %i -= %spread ) {
      ShrikeDM_addSpawnPoint(%spawnList, (%boundsWest - %edgeSize) SPC %i SPC %height, %nodeCount); %nodeCount++;
      ShrikeDM_addSpawnPoint(%spawnList, (%boundsEast + %edgeSize) SPC %i SPC %height, %nodeCount); %nodeCount++;
   }
   for( %i = %CenterY + %spread; %i < %boundsSouth; %i += %spread ) {
      ShrikeDM_addSpawnPoint(%spawnList, (%boundsWest - %edgeSize) SPC %i SPC %height, %nodeCount); %nodeCount++;
      ShrikeDM_addSpawnPoint(%spawnList, (%boundsEast + %edgeSize) SPC %i SPC %height, %nodeCount); %nodeCount++;
   }
   
   echo("Spawnsphere generation has been completed.");
   echo("Spawnpoints: " @ %nodeCount);

   Game.buildSpawns();
}

function ShrikeDM_addSpawnPoint(%spawnList, %position, %nodeCount) {
   echo("- Adding Spawnpoint " @ %nodeCount @ " at " @ %position);
   %spawnPoint = new SpawnSphere() {
      position = %position;
      dataBlock = "SpawnSphereMarker";
      radius = "8";
   };

   %spawnList.add(%spawnPoint);
}

};

function ShrikeDMGame::repeatMessageAboutJumping(%game, %obj) {
   if( !isObject(%obj) )
      return;

   %obj.MakingBreakForItInShrikeDM = "";
}

function ShrikeDMGame::setUpTeams(%game)
{  
   %group = nameToID("MissionGroup/Teams");
   if(%group == -1)
      return;
   
   // create a team0 if it does not exist
   %team = nameToID("MissionGroup/Teams/team0");
   if(%team == -1)
   {
      %team = new SimGroup("team0");
      %group.add(%team);
   }

   // 'team0' is not counted as a team here
   %game.numTeams = 0;
   while(%team != -1)
   {
      // create drop set and add all spawnsphere objects into it
      //%dropSet = new SimSet("TeamDrops" @ %game.numTeams);
      //MissionCleanup.add(%dropSet);

      //%spawns = nameToID("MissionGroup/Teams/team" @ %game.numTeams @ "/SpawnSpheres");
      //if(%spawns != -1)
      //{
      //   %count = %spawns.getCount();
      //   for(%i = 0; %i < %count; %i++)
      //      %dropSet.add(%spawns.getObject(%i));
      //}

      // set the 'team' field for all the objects in this team
      %team.setTeam(0);

      clearVehicleCount(%team+1);
      // get next group
      %team = nameToID("MissionGroup/Teams/team" @ %game.numTeams + 1);
      if (%team != -1)
         %game.numTeams++;
   }

   // set the number of sensor groups (including team0) that are processed
   setSensorGroupCount(%game.numTeams + 1);

   %game.numTeams = 1;

   // allow teams 1->31 to listen to each other (team 0 can only listen to self)
   for(%i = 1; %i < 32; %i++)
      setSensorGroupListenMask(%i, 0xfffffffe);
}

function ShrikeDMGame::allowsProtectedStatics(%game)
{
   return true;
}

function ShrikeDMGame::equip(%game, %player)
{
   for(%i =0; %i<$InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

   // there's no equipment in Shrike Deathmatch
   %player.weaponCount = 0;
}

function ShrikeDMGame::clientJoinTeam( %game, %client, %team, %respawn )
{
   %game.assignClientTeam( %client );
   
   // Spawn the player:
   %game.spawnPlayer( %client, %respawn );
}

function ShrikeDMGame::assignClientTeam(%game, %client)
{
   for(%i = 1; %i < 32; %i++)
      $DMTeamArray[%i] = false;

   %maxSensorGroup = 0;
   %count = ClientGroup.getCount();
   for(%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl != %client)
      {
         $DMTeamArray[%cl.team] = true;
         if (%cl.team > %maxSensorGroup)
            %maxSensorGroup = %cl.team;
      }
   }

   //now loop through the team array, looking for an empty team
   for(%i = 1; %i < 32; %i++)
   {
      if (! $DMTeamArray[%i])
      {
         %client.team = %i;
         if (%client.team > %maxSensorGroup)
            %maxSensorGroup = %client.team;
         break;
      }
   }

   // set player's skin pref here
   setTargetSkin(%client.target, %client.skin);

   // Let everybody know you are no longer an observer:
   messageAll( 'MsgClientJoinTeam', '\c1%1 has joined the skies.', %client.name, "", %client, 1 );
   updateCanListenState( %client );

   //now set the max number of sensor groups...
   setSensorGroupCount(%maxSensorGroup + 1);
}

function ShrikeDMGame::clientMissionDropReady(%game, %client)
{
   messageClient(%client, 'MsgClientReady',"", DMGame);
   messageClient(%client, 'MsgYourScoreIs', "", 0);
   messageClient(%client, 'MsgDMPlayerDies', "", 0);
   messageClient(%client, 'MsgDMKill', "", 0);
   %game.resetScore(%client);

   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName );
   
   DefaultGame::clientMissionDropReady(%game, %client);
}

function ShrikeDMGame::AIHasJoined(%game, %client)
{
   // Lose that null and get yourself a zero!
   %game.resetScore(%client);
}

function ShrikeDMGame::checkScoreLimit(%game, %client)
{
   //there's no score limit in DM
}

function ShrikeDMGame::resetScore(%game, %client)
{
   %client.deaths = 0;
   %client.suicides = 0;
   %client.kills = 0;
   %client.hits = 0;
   %client.score = 0;
}

function ShrikeDMGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement)
{
   // console spam
}

function ShrikeDMGame::recalcScore(%game, %client)
{
   %killValue = %client.kills * %game.SCORE_PER_KILL;
   %deathValue = %client.deaths * %game.SCORE_PER_DEATH;
   %hitValue = %client.hits * %game.SCORE_PER_HIT;
   %suicideValue = %client.suicides * %game.SCORE_PER_SUICIDE;

   %client.score = %killValue + %deathValue + %hitValue + %suicideValue;
   messageClient(%client, 'MsgYourScoreIs', "", %client.score);
   %game.recalcTeamRanks(%client);
}

function ShrikeDMGame::timeLimitReached(%game)
{
   logEcho("game over (timelimit)");
   %game.gameOver();
   cycleMissions();
}

function ShrikeDMGame::scoreLimitReached(%game)
{
   // Score Limit?  There's no friggin score limit!
}

function ShrikeDMGame::gameOver(%game)
{
   //call the default
   DefaultGame::gameOver(%game);

   messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav" );

   cancel(%game.timeThread);
   messageAll('MsgClearObjHud', "");
   for(%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      %game.resetScore(%client);
   }
}

function ShrikeDMGame::enterMissionArea(%game, %playerData, %player)
{
   %player.client.outOfBounds = false; 

   // don't show message if he's accelerating into bounds
   if( %player.RunningIntoBounds )
      %player.RunningIntoBounds = false;
   else
      messageClient(%player.client, 'EnterMissionArea', '\c1You are back in the mission area.');

   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") entered mission area");
   cancel(%player.alertThread);
}

function ShrikeDMGame::leaveMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;

   if( !$MatchStarted ) {
      // we idle oob until the mission begins
      %game.schedule(500, "leaveMissionArea", %playerData, %player);
      return;
   }
   
   if( %player.RunningIntoBounds && !%player.client.outOfBounds && !%player.ShouldBeInBounds ) {
      %player.ShouldBeInBounds = true;
      %player.client.outOfBounds = true;
      %game.schedule(1000, "leaveMissionArea", %playerData, %player);
      return;
   }
   if( !%player.client.outOfBounds && %player.ShouldBeInBounds ) {
      %player.ShouldBeInBounds = false;
      return;
   }
   
   %player.RunningIntoBounds = false;
   %player.client.outOfBounds = true;

   messageClient(%player.client, 'LeaveMissionArea', '\c1You have left the mission area.  Return or take damage.~wfx/misc/warning_beep.wav');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") left mission area");
   %player.alertThread = %game.schedule(1000, "DMAlertPlayer", 4, %player);
}

function ShrikeDMGame::DMAlertPlayer(%game, %count, %player)
{
   // MES - I commented below line out because it prints a blank line to chat window
   //messageClient(%player.client, 'MsgDMLeftMisAreaWarn', '~wfx/misc/red_alert.wav');
   if(%count > 1)
      %player.alertThread = %game.schedule(1000, "DMAlertPlayer", %count - 1, %player);
   else 
      %player.alertThread = %game.schedule(1000, "MissionAreaDamage", %player);
}

function ShrikeDMGame::MissionAreaDamage(%game, %player)
{
   // apply damage to the shrike
   %shrike = %player.mVehicle;

   if( !isObject(%shrike) )
      return;

   %shrike.lastDamagedBy = -1;
   %shrike.applyDamage(0.234);
   
   if( %shrike.getDamageState() !$= "Destroyed" )
      %player.alertThread = %game.schedule(1000, "MissionAreaDamage", %player);
}

function ShrikeDMGame::updateScoreHud(%game, %client, %tag)
{
   // Clear the header:
   messageClient( %client, 'SetScoreHudHeader', "", "" );

   // Send the subheader:
   messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,235,340,415>\tPLAYER\tKILLS\tDEATHS\tSCORE');

   for (%index = 0; %index < $TeamRank[0, count]; %index++)
   {
      //get the client info
      %cl = $TeamRank[0, %index];

      //get the score
      %clScore = %cl.score;
      %clKills = %cl.kills;
      %clDeath = %cl.deaths + %cl.suicides;

      %clStyle = %cl == %client ? "<color:dcdcdc>" : "";

      //if the client is not an observer, send the message
      if (%client.team != 0)
      {
         messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:370><just:right>%3<rmargin:460><just:right>%4', 
               %cl.name, %clComp, %clAcc, %clScore, %clStyle );
      }
      //else for observers, create an anchor around the player name so they can be observed
      else
      {
         messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:200><a:gamelink\t%6>%1</a></clip><rmargin:280><just:right>%2<rmargin:370><just:right>%3<rmargin:460><just:right>%4', 
               %cl.name, %clComp, %clAcc, %clScore, %clStyle, %cl );
      }
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

function ShrikeDMGame::applyConcussion(%game, %player)
{
}

function ShrikeDMGame::sendDebriefing( %game, %client )
{
      // Mission result:
      %winner = $TeamRank[0, 0];
      if ( %winner.score > 0 )
         messageClient( %client, 'MsgDebriefResult', "", '<just:center>%1 wins!', $TeamRank[0, 0].name );
      else
         messageClient( %client, 'MsgDebriefResult', "", '<just:center>Nobody wins.' );

      // Player scores:
      %count = $TeamRank[0, count];
      messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:70>SCORE<spop>' );
      for ( %i = 0; %i < %count; %i++ )
      {
         %cl = $TeamRank[0, %i];
         if ( %cl.score $= "" )
            %score = 0;
         else
            %score = %cl.score;

         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:70> %1</clip><lmargin%%:70><clip%%:20> %2', %cl.name, %score );
      }

   //now go through an list all the observers:
   %count = ClientGroup.getCount();
   %printedHeader = false;
   for (%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.team <= 0)
      {
         //print the header only if we actually find an observer
         if (!%printedHeader)
         {
            %printedHeader = true;
            messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>OBSERVERS<lmargin%%:70>SCORE<spop>');
         }

         //print out the client
         %score = %cl.score $= "" ? 0 : %cl.score;
         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:70> %1</clip><lmargin%%:70><clip%%:20> %2</clip>', %cl.name, %score);
      }
   }
}

function ShrikeDMGame::spawnPlayer( %game, %client, %respawn )
{
   %client.lastSpawnPoint = %game.pickPlayerSpawn( %client );
   //error(%client @" spawned at "@ %client.lastSpawnPoint);
   %game.createPlayer( %client, %client.lastSpawnPoint, %respawn );
}

function ShrikeDMGame::missionLoadDone(%game) {
   %game.buildSpawns();
   Parent::missionLoadDone(%game);
}

function ShrikeDMGame::buildSpawns( %game ) {
   if( isObject(nameToId("MissionGroup/ShrikeSpawns")) ) {
      %spawnList = nameToId("MissionGroup/ShrikeSpawns");
      %x = 0;
      for( %i = 0; %i < %spawnList.getCount(); %i++ ) {
         %spawnPoint = %spawnList.getObject(%i);
         
         if( %spawnPoint.getClassName() $= SpawnSphere ) {
            $ShrikeDM_SpawnPoint[%x] = %spawnPoint.position;
            $ShrikeDM_SpawnObj[%x] = %spawnPoint;
            %x++;
         }
      }

      $ShrikeDM_SpawnCount = %x;
   }
}

function ShrikeDMGame::pickPlayerSpawn( %game, %client ) {
   // TODO - finish this function (2021 - WHAT THE FUCK DID THAT EVEN MEAN)
   if( $ShrikeDM_SpawnCount == 0 ) {
      error("No spawn points are present.  Make them by typing into the console:");
      error("GenerateShrikeSpawns(%height);");
      return -1;
   }
   else {
      %avoidThese = $TypeMasks::VehicleObjectType  | $TypeMasks::MoveableObjectType |
                    $TypeMasks::PlayerObjectType   | $TypeMasks::TurretObjectType;

      for (%i = 0; %i < $ShrikeDM_SpawnCount; %i++ )
         %triedThis[%i] = false;

      for (%attempt = 0; %attempt < $ShrikeDM_SpawnCount; %attempt++) {
         %x = mFloor(getRandom() * ($ShrikeDM_SpawnCount - %attempt));

         for( %i = 0; %x > 0 && %i < $ShrikeDM_SpawnCount; %i++ ) {
            if( !%triedThis[%i] )
               %x--;
         }
         
         if( %x != 0 ) {
            return -1;
         }

         %x = %i;
         %triedThis[%i] = true;

         %loc = $ShrikeDM_SpawnPoint[%x];
         
         if( %loc $= "" )
            continue;
         
         %adjUp = VectorAdd(%loc, "0 0 1.0");   // don't go much below

         if ( ContainerBoxEmpty(%avoidThese,%adjUp,5.0) )
               break;

         %x = -1;
      }
      
      if( %x == -1 ) {
         error("Cannot find a suitable spawnpoint.  Make more spawnpoints or use less players.");
         return -1;
      }
      else {
         if( $TestCheats ) error("Spawn Point "@ %x SPC "(" @ $ShrikeDM_SpawnObj[%x] @ ")");
         return $ShrikeDM_SpawnPoint[%x];
      }
   }
}

function ShrikeDMGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
   // do not allow a new player if there is one (not destroyed) on this client
   if(isObject(%client.player) && (%client.player.getState() !$= "Dead"))
      return;

   // clients and cameras can exist in team 0, but players should not
   if(%client.team == 0)
      error("Players should not be added to team0!");

   // defaultplayerarmor is in 'players.cs'
   if(%spawnLoc == -1)      
      %spawnLoc = "0 0 300 1 0 0 0";
   else {
      %bounds = MissionArea.area;
      %CenterX = firstWord(%bounds) + (getWord(%bounds, 2) / 2);
      %CenterY = getWord(%bounds, 1) + (getWord(%bounds, 3) / 2);
      
      %CenterPos = %CenterX SPC %CenterY SPC getWord(%spawnLoc, 2);
      
      %angle = %game.selectSpawnFacing(%spawnLoc, %CenterPos, 0);
      
      %spawnLoc = %spawnLoc @ %angle;
   }

   // copied from player.cs
   if (%client.race $= "Bioderm")
      // Only have male bioderms.
      %armor = $DefaultPlayerArmor @ "Male" @ %client.race @ Armor;
   else
      %armor = $DefaultPlayerArmor @ %client.sex @ %client.race @ Armor;
   %client.armor = $DefaultPlayerArmor;

   %player = new Player() {
      //dataBlock = $DefaultPlayerArmor;
      dataBlock = %armor;
   };
   
   %shrike = new FlyingVehicle() {
      dataBlock = "ScoutFlyer";
   };

   %shrike.setTransform( %spawnLoc );
      
   MissionCleanup.add(%player);
   MissionCleanup.add(%shrike);
   
   // setup some info
   %player.setOwnerClient(%client);
   %player.team = %client.team;
   %shrike.team = %player.team;
   %client.outOfBounds = false;
   %player.setEnergyLevel(60);
   %client.player = %player;
   %player.RunningIntoBounds = true;

   // updates client's target info for this player
   %player.setTarget(%client.target);
   setTargetDataBlock(%client.target, %player.getDatablock());
   setTargetSensorData(%client.target, PlayerSensor);
   setTargetSensorGroup(%client.target, %client.team);
   %client.setSensorGroup(%client.team);

   //make sure the player has been added to the team rank array...
   %game.populateTeamRankArray(%client);

   // now mount him up
   %node = 0;
   commandToClient(%client,'SetDefaultVehicleKeys', true);
   if(%node == 0)
      commandToClient(%client,'SetPilotVehicleKeys', true);
   else
      commandToClient(%client,'SetPassengerVehicleKeys', true);
   %shrike.mountObject(%player,%node);
   %shrike.playAudio(0, MountVehicleSound);
   %player.mVehicle = %shrike;
   %shrike.player = %player;
   
   %shrike.getDataBlock().playerMounted(%shrike, %player, %node);

   %game.playerSpawned(%client.player);
   
   if( $MatchStarted ) {
      // accelerate the shrike towards the mission bounds
      %game.schedule(0, "accelerateShrike", %shrike);
   }
   else {
      // keeps the shrike (mostly) still until the match begins
      %shrike.lockPosition = %shrike.position;
      %game.schedule(200, "vehicleStasis", %shrike);
   }
}

function ShrikeDMGame::vehicleStasis(%game, %shrike) {
   if( !isObject(%shrike) )
      return;
   
   if( $MatchStarted ) {
      %game.accelerateShrike(%shrike);
      return;
   }
   else {
      %shrike.position = %shrike.lockPosition;
      %shrike.setVelocity("0 0 0");
      %shrike.setTransform(%shrike.getTransform());
      
      %game.schedule(250, "vehicleStasis", %shrike);
   }
}

function ShrikeDMGame::accelerateShrike(%game, %shrike) {
   // TODO - Accelerate the Shrike forward based on its rotation
   // probably want it going 200-300 kph
   %nosdir = %shrike.player.getMuzzleVector(0);
   %force = "25000";
   %shrike.applyImpulse(%shrike.getTransform(), VectorScale(%nosdir, %force));
}

function ShrikeDMGame::startMatch(%game) {
   Parent::startMatch(%game);
   
   // display default vehicle keys
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      %pl = %cl.player;
      %sl = %pl.mVehicle;
      
      if( isObject(%sl) ) {
         %node = 0;

         commandToClient(%cl,'SetDefaultVehicleKeys', true);
         if(%node == 0)
            commandToClient(%cl,'SetPilotVehicleKeys', true);
         else
            commandToClient(%cl,'SetPassengerVehicleKeys', true);
   
         commandToClient(%cl, 'setHudMode', 'Pilot', "Shrike", %node);
      }
   }
}

function ShrikeDMGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation) {
   cancel(%clVictim.player.alertThread);
   %clVictim.player.schedule(505, setActionThread, "Death11");

   // if it was damageType suicide, give it to a suicide
   if( %damageType == $DamageType::Suicide ) {
      %game.awardScoreSuicide(%clVictim);
   }
   // maybe he was killed with a shrike blaster (for real?)
   else if ( %damageType != 0 ) {
      if( isObject(%clKiller) ) {
         %game.awardScoreKill(%clVictim, %clKiller);
      }
      else {
         // how can we get here?  is there any way?
         // does it matter?
      }
   }

   // kill off the shrike
   %shrike = %clVictim.player.mVehicle;
   if( isObject(%shrike) ) {
      %shrike.player = "";
      %shrike.applyDamage(10000);
   }

   DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
}

function ShrikeDMGame::vehicleDestroyed(%game, %shrike, %destroyer) {
   if(%shrike.player $= "")
      return;
      
   if(%shrike.lastDamagedBy == 0) {
      %clVictim = %shrike.player.client;
      %game.awardScoreSuicide(%clVictim);
      %game.displayDeathMessages(%clVictim, 0, $DamageType::Crash);
   }
   else if( %shrike.lastDamagedBy == -1 ) {
      %clVictim = %shrike.player.client;
      %game.awardScoreSuicide(%clVictim);
      %game.displayDeathMessages(%clVictim, 0, $DamageType::OutOfBounds);
   }
   else {
      %clVictim = %shrike.player.client;
      %clKiller = %destroyer.player.client;
      
      if( %clKiller == 0 )
         return;
      
      %game.awardScoreKill(%clVictim, %clKiller);
      %game.displayWittyQuote(%clVictim, %clKiller, %shrike.lastDamageType);
   }
}

function ShrikeDMGame::awardScoreSuicide(%game, %clVictim) {
   %clVictim.suicides++;
   %game.recalcScore(%clVictim);
   messageClient(%clVictim, 'MsgDMPlayerDies', "", %clVictim.deaths + %clVictim.suicides);
}

function ShrikeDMGame::awardScoreKill(%game, %clVictim, %clKiller) {
   %clVictim.deaths++; %game.recalcScore(%clVictim);
   %clKiller.kills++; %game.recalcScore(%clKiller);
   messageClient(%clKiller, 'MsgDMKill', "", %clKiller.kills);
   messageClient(%clVictim, 'MsgDMPlayerDies', "", %clVictim.deaths + %clVictim.suicides);
}

function ShrikeDMGame::displayWittyQuote(%game, %clVictim, %clKiller, %damageType) {
   // my own death function.  sue me, *****es
   %victimGender = (%clVictim.sex $= "Male" ? 'him' : 'her');
   %victimPoss = (%clVictim.sex $= "Male" ? 'his' : 'her');
   %killerGender = (%clKiller.sex $= "Male" ? 'him' : 'her');
   %killerPoss = (%clKiller.sex $= "Male" ? 'his' : 'her');
   %victimName = %clVictim.name;
   %killerName = %clKiller.name;
   
   %randomMessage = mFloor(getRandom() * 7);
   //error(%randomMessage);
   switch( %randomMessage ) {
      case 0:
         %wittyQuote = '\c0%1 dines on a Shrike blaster sandwich, courtesy of %4.';
      case 1:
         %wittyQuote = '\c0The blaster of %4\'s Shrike turns %1 into finely shredded meat.';
      case 2:
         %wittyQuote = '\c0%1 gets drilled by the blaster of %4\'s Shrike.';
      case 3:
         %wittyQuote = '\c0%1 was shot out of the sky by %4.';
      case 4:
         %wittyQuote = '\c0%4 shatters %1\'s hopes and dreams with %6 Shrike blaster.';
      case 5:
         %wittyQuote = '\c0Whoops! %1 + %4\'s Shrike blaster = Dead %1.';
      case 6:
         %wittyQuote = '\c0%4 shows %1 %5 own special art form: Shrike a\'la pwned.';
   }

   messageAll('msgCTurretKill', %wittyQuote, %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
}