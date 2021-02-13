// Capture and Hold v1.1 - Credits
//
// Red Shifter:   Lead Scripter
// Celios:        l33t Hax0r
// a tiny fishie: Server Admin
// Auza:          Pickup Coordinator, Scoring System, and VSTO Map Pack?! (like that'll ever exist, lol)

$InvBanList[CnH, "ElfBarrelPack"]     = 1;
$InvBanList[CnH, "MortarBarrelPack"]  = 1;
$InvBanList[CnH, "PlasmaBarrelPack"]  = 1;
$InvBanList[CnH, "AABarrelPack"]      = 1;
$InvBanList[CnH, "MissileBarrelPack"] = 1;

// create new $Host settings
if($Host::CnH_QuickScoring $= "")  $Host::CnH_QuickScoring  = 1;
if($Host::CnH_SpawnEnergy $= "")   $Host::CnH_SpawnEnergy   = 1;
if($Host::CnH_SecureCapping $= "") $Host::CnH_SecureCapping = 0;
if($Host::CnH_SlowRespawn $= "")   $Host::CnH_SlowRespawn   = 0;

package CnHModGame {

function CnHGame::setUpTeams(%game) {  
   Parent::setUpTeams(%game);
}

function CnHGame::missionLoadDone(%game) {
   Parent::missionLoadDone(%game);

   // two effects come together to make a single total kickass effect
   %t3 = (1 << 3); %t4 = (1 << 4);
   setSensorGroupColor(1, %t3, "0 0 255 255");
   setSensorGroupColor(1, %t4, "255 96 0 255");
   setSensorGroupColor(2, %t3, "255 96 0 255");
   setSensorGroupColor(2, %t4, "0 0 255 255");
}

function CnHGame::initGameVars(%game)
{
	%game.SCORE_PER_SUICIDE = 0;
	%game.SCORE_PER_TEAMKILL = -1;
	%game.SCORE_PER_DEATH = 0;  

	%game.SCORE_PER_KILL = 1; 
	%game.SCORE_PER_PLYR_FLIPFLOP_CAP = 5;
	%game.SCORE_PER_TEAM_FLIPFLOP_CAP = 1;
	%game.SCORE_PER_TEAM_FLIPFLOP_HOLD = 1;	

	%game.SCORE_PER_TURRET_KILL = 1; 
	%game.SCORE_PER_FLIPFLOP_DEFEND = 2;

	%game.TIME_REQ_PLYR_CAP_BONUS =	12 * 1000;  //player must hold a switch 12 seconds to get points for it.

	%game.RADIUS_FLIPFLOP_DEFENSE = 40; //meters

	// for BASIC SCORE MODE
	%game.SCORE_LIMIT_PER_TOWER = 1200; //default of 1200 points per tower required to win @ 1 pt per %game.TIME_REQ_TEAM_HOLD_BONUS milliseconds
	%game.TIME_REQ_TEAM_CAP_BONUS = 12 * 1000;      //time after touching it takes for team to get a point
	%game.TIME_REQ_TEAM_HOLD_BONUS = 0.5 * 1000;	 //recurring time it takes team to earn a point when flipflop held
	
	// for QUICK SCORE MODE
	%game.SCORE_LIMIT_QUICK = 225;
	%game.TIME_CALC_SCORE = 1 * 1000;
	%game.QUICK_GRACE_PERIOD = 120 * 1000;
	
	// for SECURE MODE
	%game.RADIUS_FLIPFLOP_SECURE = 5; //meters
	%game.SECURE_TIME_SLICE = 0.25; // in seconds; do not use a multiple of 1000!
	
	// for SLOW RESPAWN mode
	%game.SLOW_RESPAWN_DELAY = 6 * 1000;
}

function CnHGame::equip(%game, %player)
{
	for(%i =0; %i<$InventoryHudCount; %i++)
		%player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

	//%player.setArmor("Light");

	%player.setInventory(Blaster, 1);
	%player.setInventory(Chaingun, 1);
	%player.setInventory(ChaingunAmmo, 100);
	%player.setInventory(Disc,1);
	%player.setInventory(DiscAmmo, 20);
	%player.setInventory(TargetingLaser, 1);
	%player.setInventory(Grenade,6);
	%player.setInventory(Beacon, 3);

	if( $Host::CnH_SpawnEnergy )
		%player.setInventory(EnergyPack, 1);

	%player.setInventory(RepairKit,1);
	%player.weaponCount = 3;

	%player.use("Blaster");
}

// activate the special scoring system
function CnHGame::startMatch(%game)
{
   for(%i = 0; %i <= %game.numTeams; %i++)
      $TeamScore[%i] = 0;

   if($Host::CnH_QuickScoring) {
      %game.quickScoreThread = %game.schedule(0, "awardQuickScore");
      %game.quickScoreActive = %game.schedule(%game.QUICK_GRACE_PERIOD, "startScoringTime");
      $CnH_QuickTimeActive = 0;
      $CnH_QuickScoring = 1;
   }
   else {
      $CnH_QuickScoring = 0;
   }

   %game.secureSwitchThread = %game.schedule(0, "checkSecureSwitches");
   DefaultGame::startMatch(%game);

   %sLimit = %game.getScoreLimit();
   messageAll('MsgCnHTeamCap', "", $TeamName[1], %game.SCORE_PER_TEAM_FLIPFLOP_HOLD, %game.cleanWord(%this.name), 1, $teamScore[1], %sLimit);
   messageAll('MsgCnHTeamCap', "", $TeamName[2], %game.SCORE_PER_TEAM_FLIPFLOP_HOLD, %game.cleanWord(%this.name), 2, $teamScore[2], %sLimit);
}

function CnHGame::doSecureAnim(%game, %player) {
   %player.setInvincibleMode(%game.SECURE_TIME_SLICE, 0.01);
}

function CnHGame::checkSecureSwitches(%game) {
   if( $Host::CnH_SecureCapping ) {
      %ffGroup = nameToID("MissionCleanup/FlipFlops");
      for (%i = 0; %i < %ffGroup.getCount(); %i++) {
         %ffObj = %ffGroup.getObject(%i);
         
         if( %ffObj.team != 0 ) {
            %found = false;
            for (%i2 = 0; %i2 < ClientGroup.getCount(); %i2++) {
               %cl = ClientGroup.getObject(%i2);
               if( %cl.team == %ffObj.team ) {
                  if (VectorDist(%ffObj.position, %cl.player.position) < %game.RADIUS_FLIPFLOP_SECURE) {
                     %game.doSecureAnim(%cl.player);
                     %found = true;
                  }
               }
            }
            
            // this should be interesting to say the least...
            if( %found )
               %ffObj.waypoint.team = %ffObj.team + 2;
            else
               %ffObj.waypoint.team = %ffObj.team;

         }
      }
   }

   %game.secureSwitchThread = %game.schedule(%game.SECURE_TIME_SLICE * 1000, "checkSecureSwitches");
}

function CnHGame::checkCaptureValidity(%game, %flipflop) {
   if ($Host::CnH_SecureCapping) {
      %count = ClientGroup.getCount();
      for (%i = 0; %i < %count; %i++) {
         %cl = ClientGroup.getObject(%i);
         if( %cl.team == %flipflop.team ) {
            if (VectorDist(%flipflop.position, %cl.player.position) < %game.RADIUS_FLIPFLOP_SECURE) {
               return false;
            }
         }
      }
   }
   
   return true;
}

function CnHGame::startScoringTime(%game) {
   cancel(%game.quickScoreActive);
   $CnH_QuickTimeActive = 1;
}

function CnHGame::awardQuickScore(%game) {
   cancel(%game.quickScoreThread);

   %team1_control = %game.countFlipsHeld(1);
   %team2_control = %game.countFlipsHeld(2);

   // don't score until all flip-flops are under control or grace period (2 minutes) has passed
   if( %team1_control + %team2_control == %game.getNumFlipFlops() || $CnH_QuickTimeActive ) {
      %oldScore1 = $TeamScore[1];
      %oldScore2 = $TeamScore[2];
      $TeamScore[1] += mPow(%team1_control, 2);
      $TeamScore[2] += mPow(%team2_control, 2);
      %sLimit = %game.getScoreLimit();
      
      if( %team1_control > 0 ) {
         messageAll('MsgCnHTeamCap', "", $TeamName[1], %game.SCORE_PER_TEAM_FLIPFLOP_HOLD, %game.cleanWord(%this.name), 1, $teamScore[1], %sLimit);
         %game.checkScoreLimit(1);
      }
      if( %team2_control > 0 ) {
         messageAll('MsgCnHTeamCap', "", $TeamName[2], %game.SCORE_PER_TEAM_FLIPFLOP_HOLD, %game.cleanWord(%this.name), 2, $teamScore[2], %sLimit);
         %game.checkScoreLimit(2);
      }

      // check milestones
      for(%i = 1; %i <= 10; %i++) {
         %compare = mFloor((%sLimit / 10) * %i);

         if( %oldScore1 < %compare && $TeamScore[1] >= %compare && %team1_control > 0)
            messageAll(0, "\c2" @ getTaggedString($TeamName[1]) @ " hold time completed: " @ (%i * 10) @ " percent~wfx/misc/target_waypoint.wav");

         if( %oldScore2 < %compare && $TeamScore[2] >= %compare && %team2_control > 0)
            messageAll(0, "\c2" @ getTaggedString($TeamName[2]) @ " hold time completed: " @ (%i * 10) @ " percent~wfx/misc/target_waypoint.wav");
      }

   }

   %game.quickScoreThread = %game.schedule(%game.TIME_CALC_SCORE, "awardQuickScore");
}

function CnHGame::getScoreLimit(%game)
{
	if(!$CnH_QuickScoring) {
		%scoreLimit = MissionGroup.CnH_scoreLimit;
		if(%scoreLimit $= "")
			%scoreLimit = %game.getNumFlipFlops() * %game.SCORE_LIMIT_PER_TOWER;
	}
	else {
		%scoreLimit = %game.SCORE_LIMIT_QUICK * mPow(%game.getNumFlipFlops(), 2);
	}

	return %scoreLimit;
}

function CnHGame::awardScoreTeamFFCap(%game, %team, %this)
{
	cancel(%this.tCapThread);

	if(!($missionRunning))
		return;

	if($CnH_QuickScoring)
		return;

	$TeamScore[%team] +=%game.SCORE_PER_TEAM_FLIPFLOP_CAP;
	%sLimit = %game.getScoreLimit();
	if (%game.SCORE_PER_TEAM_FLIPFLOP_CAP)
		messageAll('MsgCnHTeamCap', "", -1, -1, -1, %team, $teamScore[%team], %sLimit);
   if (%game.SCORE_PER_TEAM_FLIPFLOP_HOLD != 0)
		%this.tHoldThread = %game.schedule(%game.TIME_REQ_TEAM_HOLD_BONUS, "awardScoreTeamFFHold", %team, %this);
	
	%game.checkScoreLimit(%team);
}

function CnHGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc) {
   DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc);

   if ($Host::CnH_SlowRespawn) {
      %clVictim.waitRespawn = false;
      %clVictim.suicideRespawnTime = getSimTime() + %game.SLOW_RESPAWN_DELAY;
   }
}

// Function created to kick players out of turrets when it switches hands
// Thanks to Celios for doing it to me "accidentally" so I knew there was a problem.
function CnHGame::claimFlipflopResources(%game, %flipflop, %team) {
   // Copied from DefaultGame.cs
   %group = %flipflop.getGroup();
   %group.setTeam(%team);

   // Kick players out of turrets
   %group.kickTurretControl();

   // make this always visible in the commander map (gets reset when sensor group gets changed)
   setTargetAlwaysVisMask(%flipflop.getTarget(), 0xffffffff);
}

// begin toggle script
function CnHGame::sendGameVoteMenu( %game, %client, %key )
{
	DefaultGame::sendGameVoteMenu( %game, %client, %key );

	if( %client.isAdmin ) {
		if( $Host::CnH_QuickScoring )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteQuickScoring', 'Disable Advanced Scoring (next map)', 'Disable Advanced Scoring (next map)' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteQuickScoring', 'Enable Advanced Scoring (next map)', 'Enable Advanced Scoring (next map)' );

		if( $Host::CnH_SpawnEnergy )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSpawnEnergy', 'Disable Spawn w/Energy', 'Disable Spawn w/Energy' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSpawnEnergy', 'Enable Spawn w/Energy', 'Enable Spawn w/Energy' );

		if( $Host::CnH_SecureCapping )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSecureCapping', 'Disable Secured Switches', 'Disable Secured Switches' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSecureCapping', 'Enable Secured Switches', 'Enable Secured Switches' );

		if( $Host::CnH_SlowRespawn )
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSlowRespawn', 'Disable Slow Respawn', 'Disable Slow Respawn' );
		else
			messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSlowRespawn', 'Enable Slow Respawn', 'Enable Slow Respawn' );
	}
	else {
		// players don't know the meaning of balance
	}
}

function SimGroup::kickTurretControl(%this)
{
   // Scroll through the list of objects that this team controls
   for (%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).kickTurretControl();
}

function InteriorInstance::kickTurretControl(%this)
{
   // prevent console error spam
}

function AIObjective::kickTurretControl(%this)
{
   // prevent console error spam
}

function TSStatic::kickTurretControl(%this)
{
   // prevent console error spam
}

function GameBase::kickTurretControl(%this)
{
   if(%this.getDatablock().getName() $= "TurretBaseLarge") {
      //echo("Turret changes hands");

      // See if anybody is controlling the turret.
      %client = %this.getControllingClient();
      if(%client != 0) {
         //echo("Kick his arse out");
         serverCmdResetControlObject(%client);
      }
   }
}
// end turret control kick script

function serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %playerVote) {
	parent::serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %playerVote);

	if( %typeName $= "VoteQuickScoring" && %client.isAdmin )
		ToggleCnHQuickScoring(%client);
	if( %typeName $= "VoteSpawnEnergy" && %client.isAdmin )
		ToggleCnHSpawnEnergy(%client);
	if( %typeName $= "VoteSecureCapping" && %client.isAdmin )
		ToggleCnHSecureCapping(%client);
	if( %typeName $= "VoteSlowRespawn" && %client.isAdmin )
		ToggleCnHSlowRespawn(%client);
}

function ToggleCnHQuickScoring( %client ) {
	$Host::CnH_QuickScoring = !$Host::CnH_QuickScoring;
	%status = $Host::CnH_QuickScoring ? "enabled Advanced Scoring mode for the next map." : "disabled Advanced Scoring mode for the next map.";
	messageAll('MsgAdminForce', '\c2%1 %2', %client.name, %status);
}

function ToggleCnHSpawnEnergy( %client ) {
	$Host::CnH_SpawnEnergy = !$Host::CnH_SpawnEnergy;
	%status = $Host::CnH_SpawnEnergy ? "enabled spawning with an Energy Pack." : "disabled spawning with an Energy Pack.";
	messageAll('MsgAdminForce', '\c2%1 %2', %client.name, %status);
}

function ToggleCnHSecureCapping( %client ) {
	$Host::CnH_SecureCapping = !$Host::CnH_SecureCapping;
	%status = $Host::CnH_SecureCapping ? "enabled Secured Switches." : "disabled Secured Switches.";
	messageAll('MsgAdminForce', '\c2%1 %2', %client.name, %status);
}

function ToggleCnHSlowRespawn( %client ) {
	$Host::CnH_SlowRespawn = !$Host::CnH_SlowRespawn;
	%status = $Host::CnH_SlowRespawn ? "enabled Slow Respawn." : "disabled Slow Respawn.";
	messageAll('MsgAdminForce', '\c2%1 %2', %client.name, %status);
}


// gets around problems with clashing packages in CnHGame.cs
function DefaultGame::countFlips(%game) { return false; } // avoid console spam
function FlipFlop::onCollision(%data,%obj,%col)
{
   if (%col.getDataBlock().className $= Armor && %col.getState() !$= "Dead") {
      if( Game.countFlips() ) {
         Game.tryToCapture(%data, %obj, %col);
      }
      else {
         // skip the formalities
         %data.playerTouch(%obj, %col);
      }
   }
}
function CnHGame::tryToCapture(%game, %data, %obj, %col) {
   if(%obj.team != %col.client.team)
   {
      if(Game.checkCaptureValidity(%obj))
         %data.playerTouch(%obj, %col);
      else {
         if (!%col.failCap) {
            messageClient(%col.client, 0, '\c2Access Denied -- Get your enemies away from the switch first!~wfx/powered/station_denied.wav');
            %col.failCap = 1;
            %game.schedule(1000, "stopSpammingMe", %col);
         }
      }
   }
}
function CnHGame::stopSpammingMe(%game, %col) {
   %col.failCap = 0;
}

// all part of the plan
function SimGroup::setTeam(%this, %team)
{
   for (%i = 0; %i < %this.getCount(); %i++)
   {
      %obj = %this.getObject(%i);
      if( %obj.dataBlock $= "FlipFlop" ) %flipflop = %obj;
      if( %obj.dataBlock $= "WayPointMarker" ) %waypoint = %obj;
   }
   
   if( isObject(%flipflop) && isObject(%waypoint) )
      %flipflop.waypoint = %waypoint;

   Parent::setTeam(%this, %team);
}

};

activatePackage(CnHModGame);