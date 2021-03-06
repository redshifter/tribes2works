// DisplayName = SkiFree

//--- GAME RULES BEGIN ---
//SkiFree by Red Shifter
//Ski through the gates in order
//Go as far as you can in one minute
//Happy 20th Anniversary to Tribes 2
//--- GAME RULES END ---

// mission load:
// - load a new terrain from the list and give it a number - 0-128 - seed number
// - create a platform in a random location, create spawn points, trigger, observer drop point looking at it
// - move the mission boundaries around the platform - leaving that area will start your timer
// - make first gate (gate 1)
// - make extended gate (gate 2)
// - drop players into the mission
// - set timer to 10 minutes instead of whatever it normally is

// script kill the player 60 seconds after leaving mission area

// when a player crosses a gate trigger, make sure gate trigger +2 is created (gate 1 creates gate 3, gate 2 creates gate 4)

// when the player dies, calculate their score

// [DONE] make players invulnerable to all weapons but their own

// display information to player?

// TODO put waypoints on the next two relevant gates
// TODO enforce 10 minute games

// TODO racing mode (will probably be SkiRace instead of SkiFree)
// starts out as SkiFree without scoring, but game turns into a race when 2+ players are in the race
// normal races will have minimum of 4 gates, and increase by 1 for every 2 extra players, up to a maximum of 8 gates
// elimination races (minimum 4 players) will have 1 gate per player up to the maximum of 8 gates. the last person to not have crossed each gate dies (if someone dies between gates, count that as the gate kill)
// - race timer will be gate x 20 seconds (afk players will be exploded if they take more than 30 seconds to cross any gate)
// - exploding deadstop does not kill you
// - gates should refill discs and give you extra repair so you can discjump more
// - enemies you MA have their momentum cut in half
// - ambient crowd noise that gets louder if the first 2/3 players are close to each other, collective gasp if first place gets MA'd or flubs a route

// special gate - gives a nice yellow complexion
datablock ForceFieldBareData(SkiFreeGateField) : defaultAllSlowFieldBare
{
	fadeMS           = 1000;
	baseTranslucency = 0.69; // nice (not actually nice, looks awful)
	powerOffTranslucency = 0.10;
	powerOffColor    = "1.0 1.0 0.0";

	scrollSpeed = -1;
};

datablock TriggerData(SkiFreeTriggerSpawn) {
	tickPeriodMS = 50;
};

datablock TriggerData(SkiFreeTriggerGate) {
	tickPeriodMS = 50;
};

// you can't just summon an explosion - you need to blow something up
datablock ItemData(SatchelChargeDeadstop) : SatchelChargeThrown
{
   explosion = VehicleBombExplosion;
   underwaterExplosion = VehicleBombExplosion;
   maxDamage = 0.1;
   kickBackStrength = 0;
   armDelay = 1;
   computeCRC = false;
};

package SkiFreeGame {
	
// players can only damage themselves. however they can apply impulse to others
function Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC) {
	//error("Armor::damageObject( "@%data@", "@%targetObject@", "@%sourceObject@", "@%position@", "@%amount@", "@%damageType@", "@%momVec@" )");
	%oldVector = %targetObject.getVelocity();
	%sourceClient = isObject(%sourceObject) ? %sourceObject.getOwnerClient() : 0; // don't hurt others
	
	// why did i even write this shite
	if( %damageType == $DamageType::Ground ) {
		Game.schedule(0, checkDeadstop, %targetObject, %oldVector);
	}
	
	if( !%sourceClient || %targetObject == %sourceObject ) {
		// copy/pasta from classix 1.5.2, with modifications
				
		   if(%targetObject.invincible || %targetObject.getState() $= "Dead")
			  return;

		   //----------------------------------------------------------------
		   // z0dd - ZOD, 6/09/02. Check to see if this vehicle is destroyed, 
		   // if it is do no damage. Fixes vehicle ghosting bug. We do not
		   // check for isObject here, destroyed objects fail it even though
		   // they exist as objects, go figure.
		   if(%damageType == $DamageType::Impact)
			  if(%sourceObject.getDamageState() $= "Destroyed")
				 return;

		   if (%targetObject.isMounted() && %targetObject.scriptKilled $= "")
		   {
			  %mount = %targetObject.getObjectMount();
			  if(%mount.team == %targetObject.team)
			  {
				 %found = -1;
				 for (%i = 0; %i < %mount.getDataBlock().numMountPoints; %i++)
				 {
					if (%mount.getMountNodeObject(%i) == %targetObject)
					{
					   %found = %i;
					   break;
					}
				 }

				 if (%found != -1)
				 {
					if (%mount.getDataBlock().isProtectedMountPoint[%found])
					{
					   // z0dd - ZOD, 5/07/04. Let players be damaged if gameplay changes in affect.
					   if(!$Host::ClassicLoadPlayerChanges)
					   {
						  %mount.getDataBlock().damageObject(%mount, %sourceObject, %position, %amount, %damageType);
						  return;
					   }
					   else
					   {
						  if(%damageType != $DamageType::Laser && %damageType != $DamageType::Bullet && %damageType != $DamageType::Blaster) 
							 return;
					   }
					}
				 }
			  }
		   }

		   %targetClient = %targetObject.getOwnerClient();
		   if(isObject(%mineSC))
			  %sourceClient = %mineSC;   
		   else
			  %sourceClient = isObject(%sourceObject) ? %sourceObject.getOwnerClient() : 0;

		   %targetTeam = %targetClient.team;

		   //if the source object is a player object, player's don't have sensor groups
		   // if it's a turret, get the sensor group of the target
		   // if its a vehicle (of any type) use the sensor group
		   if (%sourceClient)
			  %sourceTeam = %sourceClient.getSensorGroup();
		   else if(%damageType == $DamageType::Suicide)
			  %sourceTeam = 0;
		   //--------------------------------------------------------------------------------------------------------------------
		   // z0dd - ZOD, 4/8/02. Check to see if this turret has a valid owner, if not clear the variable. 
		   else if(isObject(%sourceObject) && %sourceObject.getClassName() $= "Turret")
		   {
			  %sourceTeam = getTargetSensorGroup(%sourceObject.getTarget());
			  if(%sourceObject.owner !$="" && (%sourceObject.owner.team != %sourceObject.team || !isObject(%sourceObject.owner)))
			  {
				 %sourceObject.owner = "";
			  }
		   }
		   //--------------------------------------------------------------------------------------------------------------------
		   else if( isObject(%sourceObject) &&
			( %sourceObject.getClassName() $= "FlyingVehicle" || %sourceObject.getClassName() $= "WheeledVehicle" || %sourceObject.getClassName() $= "HoverVehicle"))
			  %sourceTeam = getTargetSensorGroup(%sourceObject.getTarget());
		   else
		   {
			  if (isObject(%sourceObject) && %sourceObject.getTarget() >= 0 )
			  {
				 %sourceTeam = getTargetSensorGroup(%sourceObject.getTarget());
			  }
			  else
			  {
				 %sourceTeam = -1;
			  }
		   }

		   // if teamdamage is off, and both parties are on the same team
		   // (but are not the same person), apply no damage
		   if(!$teamDamage && (%targetClient != %sourceClient) && (%targetTeam == %sourceTeam))
			  return;

		   if(%targetObject.isShielded && %damageType != $DamageType::Blaster)
			  %amount = %data.checkShields(%targetObject, %position, %amount, %damageType);

		   if(%amount == 0)
			  return;
			  
  		   // if the damage of a discjump would kill, override
		   if( %damageType == $DamageType::Disc && %targetObject.getDamageLevel() + %amount > 0.66 ) {
			   if( %targetObject.safetyFeature $= "" ) {
				   %targetObject.safetyFeature = 1;
				   messageClient(%targetClient, 0, 'NOT ENOUGH HEALTH FOR DISCJUMP~wfx/misc/red_alert_short.wav');
			   }
			   else {
				   messageClient(%targetClient, 0, '~wfx/misc/red_alert_short.wav');
			   }
			   %targetObject.schedule(0, setVelocity, %oldVector);
			   return;
		   }

		   // Set the damage flash
		   %damageScale = %data.damageScale[%damageType];
		   if(%damageScale !$= "")
			  %amount *= %damageScale;
		   
		   %flash = %targetObject.getDamageFlash() + (%amount * 2);
		   if (%flash > 0.75)
			  %flash = 0.75;
		   
		   %previousDamage = %targetObject.getDamagePercent();
		   %targetObject.setDamageFlash(%flash);
		   
		   %targetObject.applyDamage(%amount);
		   Game.onClientDamaged(%targetClient, %sourceClient, %damageType, %sourceObject);

		   %targetClient.lastDamagedBy = %damagingClient;
		   %targetClient.lastDamaged = getSimTime();
		   
		   //now call the "onKilled" function if the client was... you know...  
		   if(%targetObject.getState() $= "Dead")
		   {
			  // where did this guy get it?
			  %damLoc = %targetObject.getDamageLocation(%position);
			  
			  // should this guy be blown apart?
			  if( %damageType == $DamageType::Explosion || 
				  %damageType == $DamageType::TankMortar ||
				  %damageType == $DamageType::Mortar ||
				  %damageType == $DamageType::MortarTurret ||
				  %damageType == $DamageType::BomberBombs ||
				  %damageType == $DamageType::SatchelCharge ||
				  %damageType == $DamageType::Missile )     
			  {
				 if( %previousDamage >= 0.35 ) // only if <= 35 percent damage remaining
				 {
					%targetObject.setMomentumVector(%momVec);
					%targetObject.blowup(); 
				 }
			  }
		   
			  // this should be funny...
			  if( %damageType == $DamageType::VehicleSpawn )
			  {   
				 %targetObject.setMomentumVector("0 0 1");
				 %targetObject.blowup();
			  }
			  
			  // If we were killed, max out the flash
			  %targetObject.setDamageFlash(0.75);
			  
			  %damLoc = %targetObject.getDamageLocation(%position);
			  Game.onClientKilled(%targetClient, %sourceClient, %damageType, %sourceObject, %damLoc);
		   }
		   else if ( %amount > 0.1 )
		   {   
			  if( %targetObject.station $= "" && %targetObject.isCloaked() )
			  {
				 %targetObject.setCloaked( false );
				 %targetObject.reCloak = %targetObject.schedule( 500, "setCloaked", true ); 
			  }
			  
			  playPain( %targetObject );
		   }
	}
}

function SatchelChargeDeadstop::onCollision(%data,%obj,%col) {
   // Do nothing...
}

};

function SkiFreeGame::checkDeadstop(%game, %targetObject, %oldVector) {
	%newVector = %targetObject.getVelocity();
//	error("velocity " SPC %oldVector SPC ">" SPC %newVector);
	%ox = mAbs(getWord(%oldVector,0)); %nx = mAbs(getWord(%newVector,0));
	%oy = mAbs(getWord(%oldVector,1)); %ny = mAbs(getWord(%newVector,1));
	%deadx = %nx <= 0.01 && %ox > 0.01;
	%deady = %ny <= 0.01 && %oy > 0.01;
	
	%deadstop = false;
	if( %deadx && %deady ) {
		%deadstop = true;
	}
	else if( %deadx && %oy == %ny ) {
		%deadstop = true;
	}
	else if( %deady && %ox == %nx ) {
		%deadstop = true;
	}
	
	if( %deadstop ) {
		// violent deadstop? check around the perimeter!
		%minh = 500;
		%maxh = -100;
		%h = %game.findHeight( getWords(%targetObject.position, 0, 1) );
		for( %x = -1; %x <= 1; %x++ ) {
			for( %y = -1; %y <= 1; %y++ ) {
				%pos = (getWord(%targetObject.position, 0) + %x) SPC (getWord(%targetObject.position, 1) + %y);
				%nh = %game.findHeight(%pos);
				if( %minh > %h ) %minh = %h;
				if( %maxh < %h ) %maxh = %h;
			}
		}
		
		if( %minh != %maxh ) {
			error("deadstop detected at" SPC %targetObject.position SPC "but variance is" SPC mAbs(%minh - %maxh) SPC "- no explosion");
			return;
		}
		error("deadstop detected at" SPC %targetObject.position SPC "- kill the run");
		
		// what if, instead of exploding, we just go back to playing? just kidding! unless...
		// it is possible, though unlikely, to cut out like 50% of deadstops (the ones that do damage)
		// you would need to remove ground damage and schedule it to after the deadstop check
		// so it is possible, but getting to 100% 
		//%targetObject.setVelocity(%oldVector);
		//return;
		
		// if we made it out of here, explode the player for deadstopping
		%targetObject.scriptKill($DamageType::Crash);
		
		%charge = new Item("lolDeadstop") {
			dataBlock = SatchelChargeDeadstop;
			rotation = "0 0 1 " @ (getRandom() * 360);
		};
		MissionCleanup.add(%charge);
		%charge.setTransform(%targetObject.getTransform());
		%charge.schedule(100, setDamageState, "Destroyed");
		%charge.schedule(100, blowup);
		%charge.schedule(3000, delete);
		//schedule(0, 0, RadiusExplosion, %charge, %charge.getPosition(), 0, 0, 0, %targetObject, 0);
	}
}


function SkiFreeGame::initGameVars(%game) {
	// running tallies
	%game.gate = 0;
	
	// player variables
	%game.lifeTime              = 60 * 1000;
	%game.warningTime           = 10 * 1000;
	%game.scorePerGate = 100;
	//%game.heartbeatTime         = 1 * 1000;

	// auto-generation variables
	%game.firstGateMin = 800;
	%game.firstGateMax = 1100;
	%game.extraGateMin = 600;
	%game.extraGateMax = 900;
	%game.minGateAngle   = 45 * (3.1415927 / 180);
	%game.angleIncrement = 15 * (3.1415927 / 180);
	%game.maxGateAngle   = 90 * (3.1415927 / 180);
}

function SkiFreeGame::setUpTeams(%game) {  
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
	setSensorGroupCount(16);
	%game.numTeams = 1;

	// allow teams 1->31 to listen to each other (team 0 can only listen to self)
	for(%i = 1; %i < 16; %i++)
		setSensorGroupListenMask(%i, 0xfffffffe);
}


function SkiFreeGame::equip(%game, %player) {
	for(%i =0; %i<$InventoryHudCount; %i++)
		%player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
	%player.client.clearBackpackIcon();

	//%player.setArmor("Light");
	
	%player.clearInventory();

	%player.setInventory(EnergyPack, 1);
	%player.setInventory(TargetingLaser, 1);
	%player.setInventory(Beacon, 3);

	%player.setDamageLevel(0);
	%player.setEnergyLevel(60);
	%player.setInventory(FlareGrenade,30);
	%player.setInventory(Disc,1);
	%player.setInventory(DiscAmmo, 15);
	%player.setInventory(RepairKit,1);
	%player.weaponCount = 1;
	%player.use("Disc");
}

function SkiFreeGame::pickPlayerSpawn(%game, %client, %respawn) {
	if( isObject(SpawnPlatform) ) {
		%z = getWord(SpawnPlatform.position, 2) + 63.02;
		
		for( %i = 0; %i < 420; %i++ ) {
			%loc = %game.spawnPosition[%i];
			if( %loc $= "" ) {
				// game breaks down in other ways if we have this many people, lol
				return %game.spawnPosition[%i] SPC "300";
			}
			else {
				%adjUp = VectorAdd(%game.spawnPosition[%i] SPC %z, "0 0 1.0");
				if( ContainerBoxEmpty( $TypeMasks::PlayerObjectType, %adjUp, 2.0) ) {
					break;
				}
			}
		}
		
		// point the player in the direction of gate 1
		%rot = %game.selectSpawnFacing(%loc, nameToID("GatePoint1").position, 0);
		
		return %loc SPC %z SPC %rot;
	}
	
	return "0 0 300";
}

function SkiFreeGame::playerSpawned(%game, %player) {
	DefaultGame::playerSpawned(%game, %player);
	
	// TODO fix waypoint scheduler
	//%game.schedule(%game.heartbeatTime, heartbeat, %player);
}

function SkiFreeGame::heartbeat(%game, %player) {
	if( !$missionRunning ) return;
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;

	%client = %player.client;
	
	%gate = %player.gate $= "" ? 1 : %player.gate;
	
	// couldn't get this to work - dunno why. does it not accept waypoint as a target?
	//%client.setTargetId(nameToID("GatePoint" @ %gate).target);
	//commandToClient(%client, 'TaskInfo', %client, -1, false, "> > > > > > [Gate " @ %gate @ "] < < < < < <");
	//%client.sendTargetTo(%client, false);
	
	%game.schedule(%game.heartbeatTime, heartbeat, %player);
}

function SkiFreeGame::clientJoinTeam( %game, %client, %team, %respawn )
{
	%game.assignClientTeam( %client );
	%game.spawnPlayer( %client, %respawn );
}

function SkiFreeGame::assignClientTeam(%game, %client)
{
	// let's not even do the stupid DM thing
	// everyone drops to team 1
	// (if i suddenly decide i need that again we'll just do it)
	%client.team = 1;

	// set player's skin pref here
	setTargetSkin(%client.target, %client.skin);

	// Let everybody know you are no longer an observer:
	messageAll( 'MsgClientJoinTeam', '\c1%1 has joined the race.', %client.name, "", %client, 1 );
	updateCanListenState( %client );
}

function SkiFreeGame::clientMissionDropReady(%game, %client) {
	if( %client.hasSkiGameClient ) {
		// TODO are we even going to make this?
	}
	else {
		// invoke bounty to give player info (mostly score and terrain info)
		messageClient(%client, 'MsgClientReady',"", BountyGame);
		messageClient(%client, 'msgBountyTargetIs', "", %game.terrain); // terrain
		messageClient(%client, 'MsgYourScoreIs', "", 0);
	}
	
	%game.resetScore(%client);

	messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 
	
	DefaultGame::clientMissionDropReady(%game, %client);
}

function SkiFreeGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
	DefaultGame::createPlayer(%game, %client, %spawnLoc, %respawn);
	%client.setSensorGroup(%client.team);
}

function SkiFreeGame::resetScore(%game, %client) {
	%client.score = 0;
	%client.distance = 0;
	%client.runs = 0;
	%client.fullRuns = 0;
	
	for( %i = 1; %i < 420; %i++ ) {
		if( %client.gateTime[%i] $= "" ) break;
		%client.gateTime[%i] = "";
	}
}

// calculate how good a run it was
function SkiFreeGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation) {
	%player = %clVictim.player;
	
	// make sure we started and that this is a valid run
	if( %player.pointLaunch !$= "" && %player.gate > 1 ) {
		// if we skipped a gate, don't bother
		if( %damageType != $DamageType::ForceFieldPowerup ) {
			// calculate score for the run
			%gateScore = (%player.gate - 1) * %game.scorePerGate;
			%distance = 0;
			for( %i = 1; %i < %player.gate; %i++ ) {
				%distance += nameToID("GatePoint" @ %i).gateDistance;
				//echo("dist after gate " SPC %i SPC %distance);
			}
			
			// measure distance between the current point and the next point, then compare to player/next point. maximum %game.scorePerGate - 1
			%nextGate = nameToID("GatePoint" @ %player.gate);
			%nextPos = getWords(%nextGate.position, 0, 1) SPC "0";
			%playerPos = getWords(%player.position, 0, 1) SPC "0";
			
			%progress = %nextGate.gateDistance - vectorDist(%nextPos, %playerPos);
			if( %progress < 0 ) %progress = 0;
			
			%distance += %progress;
			%distance = mFloor(%distance * 10) / 10;

			%progScore = mFloor((%progress / %nextGate.gateDistance) * %game.scorePerGate);
			if( %progScore == %game.scorePerGate ) %progScore--;

			%totalScore = %gateScore + %progScore;

			%playerName = stripChars( getTaggedString( %clVictim.name ), "\cp\co\c6\c7\c8\c9" );

			%clVictim.runs++;
			if( %damageType == $DamageType::NexusCamping ) {
				%clVictim.fullRuns++;

				// also need a sound for the client
				if( %player.gate > 6 ) {
					messageClient(%clVictim, 0, '~wfx/misc/MA2.wav');
				}
				else if( %player.gate > 4 ) {
					messageClient(%clVictim, 0, '~wfx/misc/MA1.wav');
				}
				else if( %player.gate > 2 ) {
					messageClient(%clVictim, 0, '~wfx/misc/slapshot.wav');
				}
				else {
					messageClient(%clVictim, 0, '~wfx/bonuses/Nouns/llama.wav');
				}
			}
			
			// recalculate score so we know who the best player is
			%pb = %clVictim.distance < %distance;
			%pbDisplay = %clVictim.score < %totalScore;
			if( %pb ) {
				%clVictim.score = %totalScore;
				%clVictim.distance = %distance;
				
				for( %i = 1; %i < 420; %i++ ) {
					if( %player.curGateTime[%i] $= "" ) break;
					%clVictim.gateTime[%i] = %player.curGateTime[%i];
				}

				%game.recalcScore(%clVictim);
			}
			
			if( %pbDisplay ) {
				if( $TeamRank[0, count] > 1 ) {
					%rankNumber = 0;
					for( %i = 0; %i < $TeamRank[0, count]; %i++ ) {
						if( $TeamRank[0, %i] == %clVictim ) {
							%rankNumber = %i + 1;
							break;
						}
					}
					
					if( %rankNumber > 0 ) {
						%rankPersonal = " You are now in" SPC %game.getWordForRank(%rankNumber) SPC "place.";
						%gender = (%clVictim.sex $= "Male" ? "He" : "She");
						%rankOthers = " " SPC %gender SPC "is now in" SPC %game.getWordForRank(%rankNumber) SPC "place.";
					}
				}

				messageAllExcept(%clVictim, -1, 0, '%1 did a run worth %2 points (%3m).%4', %playerName, %totalScore, %distance, %rankOthers);
			}
			
			messageClient(%clVictim, 0, '\c2That run was worth %2 points (%3m).%4', %playerName, %totalScore, %distance, %rankPersonal);
		}
	}
	
	Parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
}

function SkiFreeGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement) {
	// no killing
}

// need to do this to get the recalculations
function SkiFreeGame::recalcScore(%game, %client){
	messageClient(%client, 'MsgYourScoreIs', "", %client.score);

	%game.recalcTeamRanks(%client);
	%game.checkScoreLimit(%client);
}

function SkiFreeGame::timeLimitReached(%game) {
	logEcho("game over (timelimit)");
	%game.gameOver();
	cycleMissions();
}

function SkiFreeGame::scoreLimitReached(%game) {
	// no score limit
}

function SkiFreeGame::checkScoreLimit(%game) {
	// no score limit
}

// TODO change score display at end of map to display distance travelled in addition to score?
function SkiFreeGame::gameOver(%game) {
	//call the default
	DefaultGame::gameOver(%game);

	messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav" );

	cancel(%game.timeThread);
	messageAll('MsgClearObjHud', "");
	for(%i = 0; %i < ClientGroup.getCount(); %i ++) {
		%client = ClientGroup.getObject(%i);
		%game.resetScore(%client);
		cancel(%client.waypointSchedule);
	}
}

function SkiFreeGame::enterMissionArea(%game, %playerData, %player) {
	// do nothing
}

function SkiFreeGame::leaveMissionArea(%game, %playerData, %player) {
	// do nothing
}

function SkiFreeGame::updateScoreHud(%game, %client, %tag)
{
	// Clear the header:
	messageClient( %client, 'SetScoreHudHeader', "", "" );

	// Send the subheader:
	messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,247,420>\tPLAYER\tRUNS (FULL)\tSCORE');

	for (%index = 0; %index < $TeamRank[0, count]; %index++)
	{
		//get the client info
		%cl = $TeamRank[0, %index];

		%clStyle = %cl == %client ? "<color:dcdcdc>" : "";

		//if the client is not an observer, send the message
		if (%client.team != 0)
		{
			messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20,450,500>\t<clip:200>%1</clip><rmargin:305><just:right>%2 (%7)<rmargin:461><just:right>%3<rmargin:580><just:left> (%4m)', 
				%cl.name, %cl.runs, %cl.score, %cl.distance, %clStyle, %cl, %cl.fullRuns
			);
		}
		//else for observers, create an anchor around the player name so they can be observed
		else
		{
			messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20,450,500>\t<clip:200><a:gamelink\t%6>%1</a></clip><rmargin:305><just:right>%2 (%7)<rmargin:461><just:right>%3<rmargin:580><just:left> (%4m)', 
				%cl.name, %cl.runs, %cl.score, %cl.distance, %clStyle, %cl, %cl.fullRuns
			);
			
			//messageClient( %client, 'SetLineHud', "", %tag, %index, '%7<tab:20, 450>\t<clip:200><a:gamelink\t%8>%1</a><rmargin:280><just:right>%2 (%3)<rmargin:370><just:right>%4 (%5)<rmargin:460><just:right>%6', 
					//%cl.name, %cl.flags, %flagScore, %cl.slaps, %cl.slapBonus, %cl.score, %clStyle, %cl );
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

function SkiFreeGame::missionLoadDone(%game) {
	Parent::missionLoadDone(%game);
	%game.generateLevel();
	
	setTargetAlwaysVisMask(1, 1 << 1);
	setTargetFriendlyMask(1, 1 << 1);

	%game.setSensorWaypointColors();
}

function SkiFreeGame::setSensorWaypointColors(%game) {
	setSensorGroupColor(1, 1 << 2, "255 0 0 255");
	setSensorGroupColor(1, 1 << 3, "255 128 0 255");
	setSensorGroupColor(1, 1 << 4, "255 255 0 255");
	setSensorGroupColor(1, 1 << 5, "0 255 0 255");
	setSensorGroupColor(1, 1 << 6, "0 0 255 255");
	setSensorGroupColor(1, 1 << 7, "128 0 255 255");
	setSensorGroupColor(1, 1 << 8, "255 0 255 255");
	setSensorGroupColor(1, 1 << 9, "255 128 255 255");
	// the next ones won't be seen, why not just make them random
	setSensorGroupColor(1, 1 << 10, "255 255 255 255");
	setSensorGroupColor(1, 1 << 11, "204 204 204 255");
	setSensorGroupColor(1, 1 << 12, "153 153 153 255");
	setSensorGroupColor(1, 1 << 13, "102 102 102 255");
	setSensorGroupColor(1, 1 << 14, "51 51 0 255");
	setSensorGroupColor(1, 1 << 15, "0 0 0 255");
}

function SkiFreeGame::AIHasJoined(%game, %client) {}
function SkiFreeGame::AIinit(%game) { AIInit(); }
function SkiFreeGame::applyConcussion(%game, %player) {}
function SkiFreeGame::dropFlag(%game, %player) {}

// make a spawn platform and calculate where the spawn points should be
function SkiFreeGame::addSpawnPlatform(%game, %position) {
	// position should on the ground
	
	// best interior: bwall4.dif, scaled 3 3 3 (Z-scale will need fine tuning based on experience)
	// it's about 30x30 in game units at the 3x3 scale. this is a perfect square and big enough to fit a lot of people on it. in fact, it might be TOO BIG
	if( !isObject(SpawnPlatform) ) {
		%spawnPlatform = new InteriorInstance("SpawnPlatform") {
			position = %position;
			rotation = "1 0 0 0";
			scale = "3 3 3";
			interiorFile = "bwall4.dif";
			showTerrainInside = "0";
		};
		MissionCleanup.add(%spawnPlatform);
	}
	
	// add other stuff directly
	%waypointPosition = 
		getWord(%position, 0) 
		SPC getWord(%position, 1)
		SPC (getWord(%position, 2) + 63);

	%waypointObj = new WayPoint("GatePoint0") {
		position = %waypointPosition;
		rotation = "1 0 0 0";
		scale = "1 1 1";
		dataBlock = "WayPointMarker";
		team = 1;
		name = "Starting Point";
	};
	MissionCleanup.add(%waypointObj);

	// add an observer camera pointed at the platform because why not
	%cameraPosition = 
		(getWord(%position, 0) - 40)
		SPC (getWord(%position, 1) + 40)
		SPC (getWord(%position, 2) + 81);
	
	%cameraObj = new Camera("ObserverCameraLocation") {
		position = %cameraPosition;
		rotation = "0.0949807 -0.215256 0.971928 133.575";
		scale = "1 1 1";
		dataBlock = "Observer";
		lockCount = "0";
		homingCount = "0";

		team = "0";
	};
	MissionCleanup.add(%cameraObj);
	
	// add a trigger to specify that a player's run has begun
	%triggerPosition = (getWord(%position, 0) - 15)
		SPC (getWord(%position, 1) + 15)
		SPC (getWord(%position, 2) + 63);

	%triggerObj = new Trigger("SpawnTrigger") {
		position = %triggerPosition;
		rotation = "1 0 0 0";
		scale = "30 30 5";
		dataBlock = "SkiFreeTriggerSpawn";
		lockCount = "0";
		homingCount = "0";
		polyhedron = "0 0 0 1 0 0 -0 -1 -0 -0 -0 1";
		
		gate = %gate;
	};
	
	MissionCleanup.add(%triggerObj);
	
	// build spawn platforms
	%x = getWord(%position, 0);
	%y = getWord(%position, 1);
	%game.spawnPosition[0] = %x SPC %y;
	
	// TODO randomize each list somehow?
	%i = 1;
	%i = %game.createMoreSpawns(%x, %y, %i,  7.0);
	%i = %game.createMoreSpawns(%x, %y, %i, 14.0);
	%i = %game.createMoreSpawns(%x, %y, %i,  3.5);
	%i = %game.createMoreSpawns(%x, %y, %i, 10.5);
}

function SkiFreeGame::createMoreSpawns(%game, %ox, %oy, %i, %extent) {
	for( %x = %ox - %extent; %x <= %ox + %extent; %x += 7 ) {
		for( %y = %oy - %extent; %y <= %oy + %extent; %y += 7) {
			//echo(%x SPC %y);
			%game.spawnPosition[%i] = %x SPC %y;
			%i++;
		}
	}
	
	return %i;
}

function SkiFreeGame::addGate(%game, %gate, %position) {
	// debugging
	if( isObject(nameToID("GatePoint" @ %gate)) ) {
		for( %i = %gate; %i < 420; %i++ ) {
			if( !isObject(nameToID("GatePoint" @ %i)) ) break;
			nameToId("GatePoint" @ %i).delete();
			nameToId("GateFF" @ %i).delete();
			nameToId("GateTrigger" @ %i).delete();
		}
	}
	
	// waypoint should have +25 to Z so it's slightly off the ground
	%waypointPosition = 
		getWord(%position, 0) 
		SPC getWord(%position, 1)
		SPC (getWord(%position, 2) + 25);
		
	// get distance from last point
	%distanceFromLastGate =
		vectorDist(
			getWords(%waypointPosition, 0, 1) SPC "0",
			getWords(nameToID("GatePoint" @ (%gate - 1)).position, 0, 1) SPC "0"
		);


	%waypointObj = new WayPoint("GatePoint" @ %gate) {
		position = %waypointPosition;
		rotation = "1 0 0 0";
		scale = "1 1 1";
		dataBlock = "WayPointMarker";
		team = %gate + 1;
		name = "Gate " @ %gate;
		
		gateDistance = %distanceFromLastGate;
	};

	// subtract 12.5 from X and Y so it's centered
	// also subtract 50 from Z
	%gatePosition =
		(getWord(%position, 0) - 12.5)
		SPC (getWord(%position, 1) - 12.5) 
		SPC (getWord(%position, 2) - 50.0);
	
	// create the forcefield as our visual indicator
	%gateObj = new ForceFieldBare("GateFF" @ %gate) {
		position = %gatePosition;
		rotation = "1 0 0 0";
		scale = "25 25 500";
		dataBlock = "SkiFreeGateField";
	};
	
	// trigger spawns with wrong Y coordinate and needs to be +25, don't ask me Y
	%triggerPosition = 
		getWord(%gatePosition, 0) 
		SPC (getWord(%gatePosition, 1) + 25) 
		SPC getWord(%gatePosition, 2);
	
	// create the trigger that will actually process the player touching the gate
	%triggerObj = new Trigger("GateTrigger" @ %gate) {
		position = %triggerPosition;
		rotation = "1 0 0 0";
		scale = "25 25 500";
		dataBlock = "SkiFreeTriggerGate";
		lockCount = "0";
		homingCount = "0";
		polyhedron = "0 0 0 1 0 0 -0 -1 -0 -0 -0 1";
		
		gate = %gate;
	};

	MissionCleanup.add(%waypointObj);
	//%waypointObj.target = allocTarget("", "", "", "", 0, "", ""); // TODO target can't be attached to a waypoint? how else can we set a temp waypoint?
	
	MissionCleanup.add(%gateObj);
	%gateObj.open();
	
	MissionCleanup.add(%triggerObj);
}

function SkiFreeGame::pickObserverSpawn(%game, %client, %next) {
	if( isObject(nameToID("ObserverCameraLocation")) ) {
		return nameToId("ObserverCameraLocation");
	}
	else {
		DefaultGame::pickObserverSpawn(%game, %client, %next);
	}
}

function SkiFreeGateField::onAdd(%data, %obj) {
	// absolutely no physical zone should be created, fuck that shit
	// not sure if this method is actually called the way i add the forcefield, but it doesn't really matter
}

function SkiFreeTriggerSpawn::onEnterTrigger(%this, %trigger, %player) {
	// who dat
}

function SkiFreeTriggerSpawn::onTickTrigger(%this, %trigger) {
	// it's dat boi
}

function SkiFreeTriggerSpawn::onLeaveTrigger(%this, %trigger, %player) {
	if( !$missionRunning ) return;
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;
	
	// make sure we didn't jump the gun already
	if( %player.pointLaunch !$= "" ) return;
	
	// TODO check if time is about to expire and inform the player that they can't actually finish another run

	// start the run for this player
 	%client = %player.client;
	messageClient(%client, 0, '\c2Your run has begun.~wfx/misc/target_waypoint.wav');
	%player.schedule(Game.lifeTime, scriptKill, $DamageType::NexusCamping);
	Game.schedule(Game.lifeTime - Game.warningTime, warningMessage, %player);
	%player.pointLaunch = getSimTime();
	%player.gate = 1;
}

function SkiFreeTriggerGate::onEnterTrigger(%this, %trigger, %player) {
	if( !$missionRunning ) return;
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;

	if( %trigger.gate == %player.gate ) {
		// ready the next gate
		%player.gate++;
		%player.applyRepair(0.125);
		%player.setInventory(DiscAmmo, 15);

		// TODO waypoint
		
		// get other variables
		%timeMS = (getSimTime() - %player.pointLaunch) / 1000;
		%timeMS = mFloor(%timeMS * 1000)/1000;
		%kph = mFloor(VectorLen(setWord(%player.getVelocity(), 2, 0)) * 3.6);

		%bestTimeMS = %player.client.gateTime[%trigger.gate];
		
		if( %bestTimeMS !$= "" ) {
			if( %bestTimeMS < %timeMS ) {
				%formatL = '(\c5+';
				%compare = mFloor((%timeMS - %bestTimeMS) * 1000)/1000 ;
			}
			else if( %bestTimeMS > %timeMS ) {
				%formatL = '(\c3-';
				%compare = mFloor((%bestTimeMS - %timeMS) * 1000)/1000;
			}
			else {
				%formatL = '(+';
				%compare = 0;
			}
			%formatR = '\c0)';
		}
		
		%player.curGateTime[%trigger.gate] = %timeMS;
		
		messageClient(%player.client, 0, '\c0Passed Gate %1 at %2 seconds (Speed: %3kph) %4%5%6~wfx/misc/target_waypoint.wav', %trigger.gate, %timeMS, %kph, %formatL, %compare, %formatR);
		
		// generate gate +2 if it doesn't exist
		if( Game.gate == %player.gate ) {
			Game.generateGate(%player.gate + 1);
		}
	}
	else if( %trigger.gate > %player.gate ) {
		messageClient(%player.client, 0, '\c2GATE SKIP DETECTED!~wfx/misc/red_alert_short.wav');
		%player.scriptKill($DamageType::ForceFieldPowerup);
	}
}

function SkiFreeTriggerGate::onTickTrigger(%this, %trigger) {
	// o shit wut up
}

function SkiFreeTriggerGate::onLeaveTrigger(%this, %trigger, %player) {
	// pants
}

function SkiFreeGame::warningMessage(%game, %player) {
	if( !$missionRunning ) return;
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;
	
	messageClient(%player.client, 0, '\c2Only %1 seconds left!~wfx/misc/bounty_objrem1.wav', %game.warningTime / 1000);
}

function SkiFreeGame::generateLevel(%game) {
	// choose a terrain
	if( !isObject(Terrain) ) {
		%game.generateTerrain();
	}

	// generate a spawn platform
	%game.generateSpawnPlatform();

	// add the first 2 gates
	%game.generateGate(1);
	%game.generateGate(2);
}

function SkiFreeGame::generateSpawnPlatform(%game) {
	// allow mappers to pre-define the spawn platform
	if( isObject(SpawnPlatform) ) {
		%game.addSpawnPlatform(SpawnPlatform.position);
	}
	else {
		// grab 8 random points - the spawn must be at or above this (is this actually something we want? lol)
		%max = -100;
		for( %i = 0; %i < 8; %i++ ) {
			%spawnPoint = getRandom(-1024,1024) SPC getRandom(-1024, 1024);
			%height = %game.findHeight(%spawnPoint);
			if( %height > %max ) {
				%max = %height;
			}
		}
		
		//messageAll(0, "max:" SPC %max);
		
		for( %i = 0; %i < 420; %i++ ) {
			// reduce the max at each iteration
			%max -= 2;

			// grab a random point
			%spawnPoint = getRandom(-1024,1024) SPC getRandom(-1024, 1024);
			%height = %game.findHeight(%spawnPoint);
			if( %height < %max ) {
				//messageAll(0, "not high enough" SPC %height SPC "<" SPC %max);
				continue;
			}
			
			// check all 9 points around the spawn point and keep track of them - use lowest Z point so the tower is embedded in the ground
			%maxZ = -100;
			%z = 500;
			for( %x = -1; %x <= 1; %x++ ) {
				for( %y = -1; %y <= 1; %y++ ) {
					%dayZ = %game.findHeight((getWord(%spawnPoint,0) + (%x * 15)) SPC (getWord(%spawnPoint,1) + (%y * 15)));
					if( %z > %dayZ ) %z = %dayZ;
					if( %maxZ < %dayZ ) %maxZ = %dayZ;
				}
			}
			
			// variance should be less than 50 - if it's more, reroll it. we don't want the terrain to intersect the spawn platform
			if( %maxZ - %z >= 50 ) {
				//messageAll(0, "variance too high" SPC (%maxZ - %z));
				break;
			}
			
			break;
		}
		
		%game.addSpawnPlatform(%spawnPoint SPC %z);
	}
}

function SkiFreeGame::validateSpawnPlatform(%game) {
	// TODO do i actually want to do this shit
}

function SkiFreeGame::generateGate(%game, %gate) {
	// TODO add some mapping mechanism for defining where each gate will be generated
	if( %gate == 1 ) {
		// angle should just be anywhere
		%pivot = nameToID("GatePoint0").position;
		%angle = getRandom(2 * 3.1415927 * 100000) / 100000;
		%dist = getRandom(%game.firstGateMin, %game.firstGateMax);
	}
	else {
		%origin = nameToID("GatePoint" @ (%gate - 2)).position;
		%pivot = nameToID("GatePoint" @ (%gate - 1)).position;
		%angle = mAtan( 
			getWord(%pivot, 1) - getWord(%origin, 1), 
			getWord(%pivot, 0) - getWord(%origin, 0) 
		);
		
		%modAngle = (%game.minGateAngle + (%game.angleIncrement * (%gate - 2))) / 2;
		if( %modAngle > %game.maxGateAngle ) %modAngle = %game.maxGateAngle;
		
		%angle += (getRandom(-%modAngle * 100000, %modAngle * 100000) / 100000);
		%dist = getRandom(%game.extraGateMin, %game.extraGateMax);
	}
	//%viewableAngle = (%angle / (3.1415927/180));
	//while( %viewableAngle < 0 ) %viewableAngle += 360;
	//echo("Gate" SPC %gate SPC "at" SPC %viewableAngle SPC "degrees");
	
	%x = getWord(%pivot, 0) + (mCos(%angle) * %dist);
	%y = getWord(%pivot, 1) + (mSin(%angle) * %dist);
	%z = %game.findHeight(%x SPC %y);
	
	%game.addGate(%gate, %x SPC %y SPC %z);
	%game.gate = %gate;
}

function SkiFreeGame::generateTerrain(%game) {
	// pick a terrain from the list
	exec("scripts/SkiFreeTerrains.cs");
	
	%error = "";
	%valid = false;
	if( $SkiFreeTerrainListMAX !$= "" ) {
		%terrain = $SkiFreeTerrainList[getRandom($SkiFreeTerrainListMAX)];
		if( isFile("terrains/" @ %terrain) ) {
			%valid = true;
		}
		else {
			%error = "Terrain file" SPC %terrain SPC "doesn't exist!";
		}
	}
	else {
		%error = "SkiFreeTerrains.cs does not exist or did not correctly compile!";
	}
	
	if( $TerrainTest !$= "" ) {
		%terrain = $TerrainTest;
		if( strpos(%terrain, ".ter") == -1 ) %terrain = %terrain @ ".ter";

		if( isFile("terrains/" @ %terrain) ) {
			%valid = true;
		}
		else {
			%error = "$TerrainTest file" SPC %terrain SPC "doesn't exist!";
			%valid = false;
		}
	}
	
	if( %valid ) {
		messageAll(0, 'Using Terrain: %1', %terrain);
	}
	else {
		messageAll(0, 'There was an error loading terrain %1 - %2', %terrain, %error);
		%terrain = "SunDried.ter";
	}
	
	// don't leave this shit laying around
	deleteVariables("$SkiFreeTerrainList*");
	
	%terrainObj = new TerrainBlock(Terrain) {
		rotation = "1 0 0 0";
		scale = "1 1 1";
		detailTexture = "details/lushdet1";
		terrainFile = %terrain;
		squareSize = "8";

		position = "-1024 -1024 0";
	};
	MissionCleanup.add(%terrainObj);
	
	%game.terrain = getSubStr(%terrain, 0, strpos(%terrain, "."));
	messageAll('msgBountyTargetIs', "", %game.terrain); // terrain
}

function SkiFreeGame::findHeight(%game, %location) {
	%hit = ContainerRayCast(%location SPC "1000", %location SPC "-100", $TypeMasks::TerrainObjectType);
	return getWord(%hit,3);
}

function SkiFreeGame::displayDeathMessages(%game, %clVictim, %clKiller, %damageType, %implement) {
   // ----------------------------------------------------------------------------------
   // z0dd - ZOD, 6/18/02. From Panama Jack, send the damageTypeText as the last varible
   // in each death message so client knows what weapon it was that killed them.

   %victimGender = (%clVictim.sex $= "Male" ? 'he' : 'she');
   %victimName = %clVictim.name;
   //error("DamageType = " @ %damageType @ ", implement = " @ %implement @ ", implement class = " @ %implement.getClassName() @ ", is controlled = " @ %implement.getControllingClient());
   
	if(%damageType == $DamageType::ForceFieldPowerup)
	{
		messageAll('msgVehicleSpawnKill', '\c0%1 tried to skip a gate.', %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by Gate Skip");
	}
	else if( %damageType == $DamageType::Crash ) {
		messageAll('msgVehicleSpawnKill', '\c0%1 deadstopped.', %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by Deadstop");
	}
	else if( %damageType == $DamageType::NexusCamping ) {
		// no message needed for a full run completed
		logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed by End of Run");
	}
	else {
		Parent::displayDeathMessages(%game, %clVictim, %clKiller, %damageType, %implement);
	}
}

function SkiFreeGame::getWordForRank(%game, %rank) {
	if( %rank == 1 ) return "first";
	else if( %rank == 2 ) return "second";
	else if( %rank == 3 ) return "third";
	else return %rank @ "th"; // just type the number
}

function SkiFreeGame::sendDebriefing( %game, %client )
{
	// Mission result:
	%winner = $TeamRank[0, 0];
	if ( %winner.score > 0 )
		messageClient( %client, 'MsgDebriefResult', "", '<just:center>%1 wins!', $TeamRank[0, 0].name );
	else
		messageClient( %client, 'MsgDebriefResult', "", '<just:center>Nobody wins.' );

	// Player scores:
	%count = $TeamRank[0, count];
	messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:60>SCORE<lmargin%%:80>DISTANCE<spop>' );
	for ( %i = 0; %i < %count; %i++ ) {
		%cl = $TeamRank[0, %i];
		if ( %cl.score $= "" )
			%score = 0;
		else
			%score = %cl.score;

		if ( %cl.distance $= "" )
			%dist = 0;
		else
			%dist = %cl.distance;

		messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:60><clip%%:20> %2</clip><lmargin%%:80><clip%%:20> %3m', %cl.name, %score, %dist );
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

// TODO hook up this code
function SkiFreeGame::phaseThroughPlayers(%game, %active) {
	// this code adds/removes the player mask from player collisions
	// so players can phase right through each other if active
	// thanks to DarkTiger for this code (how did he come up with it so quickly)
	%patch1 = %active ? "3CA1" : "3CE1";
	%patch2 = %active ? "1C21" : "1C61";
	%patch3 = %active ? "1C21" : "1C61";
	
	memPatch("83FBF4", %patch1);
	memPatch("79B40C", %patch2);
	memPatch("83FBF8", %patch3);
}