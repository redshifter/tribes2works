// --------------------------------------------------------
// Blaster Deathmatch v1.5
//
// Concepts and coding by Red Shifter
// --------------------------------------------------------

// DisplayName = Blaster Deathmatch

//--- GAME RULES BEGIN ---
//3 points for each hit, +1 without Energy Pack
//2 points for each kill, +3 without Energy Pack
//1 point lost for each death or self-hit
//Self-healing occurs when you kill an enemy
//--- GAME RULES END ---

$BlasterDM::ServerVersion = 1.5;
$BlasterDM::ClientVersion = 1.0;

function BlasterDMGame::initGameVars(%game)
{
   %game.SCORE_PER_HIT        = 3;
   %game.SCORE_PER_HIT_BONUS  = 1;

   %game.SCORE_PER_KILL       = 2;
   %game.SCORE_PER_KILL_BONUS = 3;

   %game.SCORE_PER_DEATH      = -1;
   %game.SCORE_PER_SCREWUP    = -1;
}

$InvBanList[BlasterDM, "TurretOutdoorDeployable"] = 1;
$InvBanList[BlasterDM, "TurretIndoorDeployable"] = 1;
$InvBanList[BlasterDM, "ElfBarrelPack"] = 1;
$InvBanList[BlasterDM, "MortarBarrelPack"] = 1;
$InvBanList[BlasterDM, "PlasmaBarrelPack"] = 1;
$InvBanList[BlasterDM, "AABarrelPack"] = 1;
$InvBanList[BlasterDM, "MissileBarrelPack"] = 1;
$InvBanList[BlasterDM, "ShieldPack"] = 1;
$InvBanList[BlasterDM, "AmmoPack"] = 1;
$InvBanList[BlasterDM, "CloakingPack"] = 1;
$InvBanList[BlasterDM, "RepairPack"] = 1;
$InvBanList[BlasterDM, "Mine"] = 1;
$InvBanList[BlasterDM, "Shocklance"] = 1;
$InvBanList[BlasterDM, "Mortar"] = 1;
$InvBanList[BlasterDM, "Disc"] = 1;
$InvBanList[BlasterDM, "Chaingun"] = 1;
$InvBanList[BlasterDM, "MissileLauncher"] = 1;
$InvBanList[BlasterDM, "GrenadeLauncher"] = 1;
$InvBanList[BlasterDM, "SniperRifle"] = 1;
$InvBanList[BlasterDM, "ELFGun"] = 1;
$InvBanList[BlasterDM, "Plasma"] = 1;
$InvBanList[BlasterDM, "CameraGrenade"] = 1;
$InvBanList[BlasterDM, "MotionSensorDeployable"] = 1;
$InvBanList[BlasterDM, "PulseSensorDeployable"] = 1;
$InvBanList[BlasterDM, "SatchelCharge"] = 1;
$InvBanList[BlasterDM, "SensorJammerPack"] = 1;
$InvBanList[BlasterDM, "InventoryDeployable"] = 1;
$InvBanList[BlasterDM, "Grenade"] = 1;
$InvBanList[BlasterDM, "ConcussionGrenade"] = 1;
$InvBanList[BlasterDM, "FlashGrenade"] = 1;

exec("scripts/aiBDM.cs");

package BlasterDMGame {

function BlasterImage::onFire(%data, %obj, %slot)
{
	if( Parent::onFire(%data,%obj,%slot) ) {
	    %obj.client.total++;
		Game.recalcScore(%obj.client);
    }
}

function ShapeBase::throwWeapon(%this) {
    // yeah...
    if( %this.getState() !$= "Dead" )
        return;

    Parent::throwWeapon(%this);
}

function ShapeBase::throw(%this,%data) {
    // you don't throw your blaster until you're dead
    // gets around "blaster throw" scripts... which are the devil indoors, btw
    if( %data.image $= "BlasterImage" && %this.getState() !$= "Dead" )
        return;

    Parent::throw(%this,%data);
}

function Pack::onCollision(%data, %obj, %col)
{
    // Once your pack is gone, it's gone for good!
}

};

function BlasterDMGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %sourceObject)
{
   if (%damageType != $DamageType::Blaster)
      return;

   if (%clVictim == %clAttacker)
   {
      %clVictim.selfhit++;
      %game.recalcScore(%clVictim);
   }
   if (%clVictim != %clAttacker)
   {
      %clAttacker.hit++;

      if(isObject(%clAttacker.player))
         if(!%clAttacker.player.hasEnergyPack)
            %clAttacker.bonus += %game.SCORE_PER_HIT_BONUS;

      %game.recalcScore(%clAttacker);
      
      // give a damage click if we're not playing classic
      // we don't call Parent so we need to reinstate it for classic anyway
      if( $classicVersion $= "" )
         messageClient(%clAttacker, 'MsgClientHit', '~wfx/weapons/cg_hard4.wav');
      else
         messageClient(%clAttacker, 'MsgClientHit', %clAttacker.playerHitWav);

      // the lastDamagedBy gets defined right after onClientDamaged (for some ungodly reason)
      // so a schedule is absolutely necessary
      if( %clVictim.isAIControlled() )
         %game.schedule(0, RSAI_heHitMe, %clVictim);
   }

   %game.bleedAnimation(%clVictim.player);
}

function BlasterDMGame::missionLoadDone(%game)
{
   $CorpseTimeoutValue = 2 * 1000;
   DefaultGame::missionLoadDone(%game);
}

function BlasterDMGame::setUpTeams(%game)
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
      %dropSet = new SimSet("TeamDrops" @ %game.numTeams);
      MissionCleanup.add(%dropSet);

      %spawns = nameToID("MissionGroup/Teams/team" @ %game.numTeams @ "/SpawnSpheres");
      if(%spawns != -1)
      {
         %count = %spawns.getCount();
         for(%i = 0; %i < %count; %i++)
            %dropSet.add(%spawns.getObject(%i));
      }

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

function BlasterDMGame::allowsProtectedStatics(%game)
{
   return true;
}

function BlasterDMGame::equip(%game, %player)
{
   for(%i =0; %i<$InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

   //%player.setArmor("Light");
   %player.setInventory(RepairKit, 1);
   %player.setInventory("Blaster", 1);
   %player.setInventory("EnergyPack", 1);
   %player.setInventory("TargetingLaser", 1);
   %player.setInventory("FlareGrenade", 5);
   %player.weaponCount = 1;

   %player.use("Blaster");
}

function BlasterDMGame::pickPlayerSpawn(%game, %client, %respawn)
{
   // all spawns come from team 1
   return %game.pickTeamSpawn(1);
}

function BlasterDMGame::clientJoinTeam( %game, %client, %team, %respawn )
{
   %game.assignClientTeam( %client );
   
   // Spawn the player:
   %game.spawnPlayer( %client, %respawn );
}

function BlasterDMGame::assignClientTeam(%game, %client)
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
   messageAll( 'MsgClientJoinTeam', '\c1%1 has joined the fray.', %client.name, "", %client, 1 );
   updateCanListenState( %client );

   //now set the max number of sensor groups...
   setSensorGroupCount(%maxSensorGroup + 1);
}

function BlasterDMGame::clientMissionDropReady(%game, %client)
{
   if( %client.hasBlasterDMClient ) {
      messageClient(%client, 'MsgClientReady',"", BlasterDMGame);

      %game.messageClientInfo(%client);
      %game.messageLeaderInfo(%client);
   }
   else {
      messageClient(%client, 'MsgClientReady',"", DMGame);
      messageClient(%client, 'MsgDMPlayerDies', "", 0);
      messageClient(%client, 'MsgDMKill', "", 0);
      messageClient(%client, 'MsgYourScoreIs', "", 0);
   }

   %game.resetScore(%client);

   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName );
   
   DefaultGame::clientMissionDropReady(%game, %client);
}

function BlasterDMGame::AIHasJoined(%game, %client)
{
   // Lose that null and get yourself a zero!
   %game.resetScore(%client);
}

function BlasterDMGame::checkScoreLimit(%game, %client)
{
   //there's no score limit in DM
}

function BlasterDMGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
   DefaultGame::createPlayer(%game, %client, %spawnLoc, %respawn);
   %client.setSensorGroup(%client.team);
}

function BlasterDMGame::resetScore(%game, %client)
{
   %client.deaths = 0;
   %client.kills = 0;
   %client.score = 0;

// Native to Blaster Deathmatch
   %client.total = 0;
   %client.bonus = 0;
   %client.hit = 0;
   %client.selfhit = 0;
   %client.acc = 0;
   
   %game.recalcScore(%client);
}

function BlasterDMGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc)
{
   cancel(%clVictim.player.alertThread);
   %clVictim.player.setInventory(RepairKit, 0);
   DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc);
}

function BlasterDMGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement)
{
   if (%game.testKill(%clVictim, %clKiller) && (%damageType == $DamageType::Blaster)) //verify victim was an enemy
   {
      %game.awardScoreKill(%clKiller);
      if(isObject(%clKiller.player)) {

         if(!%clKiller.player.hasEnergyPack)
            %clKiller.bonus += %game.SCORE_PER_KILL_BONUS;

         if(%clKiller.player.getState() !$= "Dead") {
            %clKiller.player.applyRepair(0.2);

            if (%clKiller.player.getDamageLevel() == 0 && %clKiller.player.getInventory(RepairKit) == 0) {
               messageClient(%clKiller, 'MsgItemPickup', '\c0You picked up a repair kit.', "a repair kit");
               serverPlay3D(ItemPickupSound, %clKiller.player.getTransform());
               %clKiller.player.incInventory(RepairKit, 1);
            }
         }
      }

	  if( %clKiller.hasBlasterDMClient ) {
	  
	  }
	  else {
         messageClient(%clKiller, 'MsgDMKill', "", %clKiller.kills);
      }
      %game.awardScoreDeath(%clVictim);
   }       
   else if (%game.testSuicide(%clVictim, %clKiller, %damageType))  //otherwise test for suicide
      %game.awardScoreSuicide(%clVictim);
   else if (%damageType != 0)
      %game.awardScoreDeath(%clVictim);

   if( %clVictim.hasBlasterDMClient ) {
      
   }
   else {
      messageClient(%clVictim, 'MsgDMPlayerDies', "", %clVictim.deaths + %clVictim.suicides);
   }
}

function BlasterDMGame::recalcScore(%game, %client)
{
   %killValue = %client.kills * %game.SCORE_PER_KILL;
   %deathValue = (%client.deaths + %client.suicides) * %game.SCORE_PER_DEATH;
   %hitValue = %client.hit * %game.SCORE_PER_HIT;
   %suicideValue = %client.selfhit * %game.SCORE_PER_SCREWUP;
   %bonusValue = %client.bonus;

   if (%client.total != 0)
      %client.acc = %client.hit / %client.total;

   %client.score = %killValue + %deathValue + %hitValue + %suicideValue + %bonusValue;

   if( %client.hasBlasterDMClient ) {
      %game.messageClientInfo(%client);
   }
   else
      messageClient(%client, 'MsgYourScoreIs', "", %client.score);

   %leader = $TeamRank[0, 0];
   %game.recalcTeamRanks(%client);
   
   if( %leader != $TeamRank[0, 0] || %client == %leader )
      %game.messageLeaderInfo();
   
   %game.checkScoreLimit(%client);
}

function BlasterDMGame::timeLimitReached(%game)
{
   logEcho("game over (timelimit)");
   %game.gameOver();
   cycleMissions();
}

function BlasterDMGame::scoreLimitReached(%game)
{
   // Score Limit?  There's no friggin score limit!
}

function BlasterDMGame::gameOver(%game)
{
   $CorpseTimeoutValue = 22 * 1000;

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

function BlasterDMGame::enterMissionArea(%game, %playerData, %player)
{
   %player.client.outOfBounds = false; 
   messageClient(%player.client, 'EnterMissionArea', '\c1You are back in the mission area.');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") entered mission area");
   cancel(%player.alertThread);
}

function BlasterDMGame::leaveMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;
                                         
   %player.client.outOfBounds = true;
   messageClient(%player.client, 'LeaveMissionArea', '\c1You have left the mission area. Return or DIE!~wfx/misc/warning_beep.wav');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") left mission area");
   %player.alertThread = %game.schedule(1000, "DMAlertPlayer", 3, %player);
}

function BlasterDMGame::DMAlertPlayer(%game, %count, %player)
{
   // MES - I commented below line out because it prints a blank line to chat window
   //messageClient(%player.client, 'MsgDMLeftMisAreaWarn', '~wfx/misc/red_alert.wav');
   if(%count > 1)
      %player.alertThread = %game.schedule(1000, "DMAlertPlayer", %count - 1, %player);
   else 
      %player.alertThread = %game.schedule(1000, "MissionAreaDamage", %player);
}

function BlasterDMGame::MissionAreaDamage(%game, %player)
{
   %player.scriptKill($DamageType::OutOfBounds);
}

function BlasterDMGame::updateScoreHud(%game, %client, %tag)
{
   // Clear the header:
   messageClient( %client, 'SetScoreHudHeader', "", "" );

   // Send the subheader:
   messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,235,340,415>\tPLAYER\tHITS\tACC\tSCORE');

   for (%index = 0; %index < $TeamRank[0, count]; %index++)
   {
      //get the client info
      %cl = $TeamRank[0, %index];

      //get the score
      %clScore = %cl.score;

      %clHits = %cl.hit;
      %clTotal = %cl.total;
      %clComp = %clHits @ "/" @ %clTotal;

      %clAcc = %cl.acc * 10000;

      if( %clAcc % 100 == 0 )
          %clApp = ".00%";
      else if( %clAcc % 10 == 0 )
          %clApp = "0%";
      else
          %clApp = "%";

      %clAcc = (mFloor(%clAcc) / 100) @ %clApp;

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

function BlasterDMGame::applyConcussion(%game, %player)
{
}

function BlasterDMGame::sendDebriefing( %game, %client )
{
      // Mission result:
      %winner = $TeamRank[0, 0];
      if ( %winner.score > 0 )
         messageClient( %client, 'MsgDebriefResult', "", '<just:center>%1 wins!', $TeamRank[0, 0].name );
      else
         messageClient( %client, 'MsgDebriefResult', "", '<just:center>Nobody wins.' );

      // Player scores:
      %count = $TeamRank[0, count];
      messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:60>SCORE<lmargin%%:80>ACC<spop>' );
      for ( %i = 0; %i < %count; %i++ )
      {
         %cl = $TeamRank[0, %i];
         if ( %cl.score $= "" )
            %score = 0;
         else
            %score = %cl.score;

         if ( %cl.acc $= "")
            %acc = "0.00%";
         else {
            %acc = %cl.acc * 10000;
 
            if( %acc % 100 == 0 )
                %clApp = ".00%";
            else if( %acc % 10 == 0 )
                %clApp = "0%";
            else
                %clApp = "%";

            %acc = (mFloor(%acc) / 100) @ %clApp;
         }
         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:60><clip%%:20> %2</clip><lmargin%%:80><clip%%:20> %3', %cl.name, %score, %acc );
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
            messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>OBSERVERS<lmargin%%:60>SCORE<spop>');
         }

         //print out the client
         %score = %cl.score $= "" ? 0 : %cl.score;
         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2</clip>', %cl.name, %score);
      }
   }
}

function BlasterDMGame::bleedAnimation(%game, %obj)
{
   if (MissionGroup.NoFlare)
      return;

   %p = new FlareProjectile() {
      dataBlock        = FlareGrenadeProj;
      initialDirection = %obj.getEyeVector();
      initialPosition  = getBoxCenter(%obj.getWorldBox());
      sourceObject     = %obj;
      sourceSlot       = 0;
   };
   MissionCleanup.add(%p);
   %p.schedule(6000, "delete");
}

function BlasterDMGame::messageClientInfo(%game, %client) {
   %clAcc = %client.acc * 10000;
   if( %clAcc % 100 == 0 )
      %clApp = ".00%";
   else if( %clAcc % 10 == 0 )
      %clApp = "0%";
   else
      %clApp = "%";
   %clAcc = (mFloor(%clAcc) / 100) @ %clApp;

   messageClient(%client, 'MsgBlasterDMScore', "", %client.score, %client.hit, %client.total, %clAcc);
}

function BlasterDMGame::messageLeaderInfo(%game, %client) {
   if( %client $= "" ) {
      for( %i = 0; %i < ClientGroup.getCount(); %i++ )
         if( !ClientGroup.getObject(%i).isAIControlled() )
            %game.messageLeaderInfo(ClientGroup.getObject(%i));
   }
   else {
      %leader = $TeamRank[0, 0];
       
      %clAcc = %leader.acc * 10000;
      if( %clAcc % 100 == 0 )
          %clApp = ".00%";
      else if( %clAcc % 10 == 0 )
          %clApp = "0%";
      else
          %clApp = "%";
      %clAcc = (mFloor(%clAcc) / 100) @ %clApp;

      messageClient(%client, 'MsgBlasterDMLeader', "", %leader.score, %leader.hit, %leader.total, %clAcc);
   }
}

function serverCmdBlasterDMRegisterClient(%client, %version) {
	%client.hasBlasterDMClient = %version;

	//if (%version >= $BlasterDM::ClientVersion)
	//	messageClient(%client, '', '\c2Blaster Deathmatch Client successfully registered.');
	//else
	//	messageClient(%client, '', '\c2A newer SLDM Client (v%1) is available.', $ShocklanceDM::latestClientVersion);
}