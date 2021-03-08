// DisplayName = SkiFree

//--- GAME RULES BEGIN ---
//Ski through the gates in order
//Go as far as you can in one minute
//Happy 20th Anniversary to Tribes 2
//--- GAME RULES END ---

// created by Red Shifter
// thanks to:
// - DarkTiger for the phase through players code

// mapping instructions:
// - your terrainblock should be called Terrain. this is normally done in a T2 mission already.
//   (one would be created if you didn't have one, but the SkiFree map already does that - no reason to make another)
//
// - spawn platform will be auto-generated somewhere from -1024/-1024 to 1024/1024, unless you've included one
//   it you include a spawn platform, it should be named "SpawnPlatform", and be bwall4.dif at scale 3 3 3
//   everything a spawn platform needs (triggers, spawnpoints, observer cameras, etc) will be auto-generated. no need to include any of this
//
// - the game will be ignoring buildings when it generates gates, so don't put too many of the building
//
// - if you want to define gates, too bad. i didn't get that far.
//   TODO make it possible to define gates
//   gates will continue being generated even after your final gate (in SkiFree). so if you're making an indoor map, be sure to have an exit to the outdoors and a good terrain for skiing
//
// - bots have no idea how to play indoor games so you probably shouldn't bother with a navgraph
// - though it's usually a good idea to include a two-team spawngraph even if it's not being used

// TODO organize the shit out of the methods and put them into extra CS files, i mean holy shit this is super disorganized
// there should be files for:
// - datablocks
// - map generation
// - ai
// - SkiFreeGame package code

// TODO put waypoints on the next two relevant gates (wasn't working when i tried it)

// TODO vaporware racing mode (will probably be SkiRace instead of SkiFree, which means i need to unify this code when i get there)
// starts out as SkiFree without scoring, but game turns into a race when 2+ players join the game. should allow player joins up until 5 seconds after race starts
// normal races will have minimum of 4 gates, and increase by 1 for every 2 extra players, up to a maximum of 8 gates at 8+ players
// elimination races (minimum 4 players) will have 1 gate per player up to the maximum of 8 gates. the last person to not have crossed each gate dies (if someone dies between gates, count that as the gate kill)
// - race timer will be gate x 20 seconds (with a 30 second timer to explode a player that fails to cross a gate every 30 seconds)
// - exploding deadstop does not kill you
// - gates should give you extra repair so you can discjump more
// - enemies you MA have their momentum cut in half
//
// if making a map with predefined gates, the missiongroup will have some variables for this
// SkiRace_EndAtFinal - the race will finish at your final gate (if not defined, all gates must be crossed and then it will generate additional gates if needed to get up to 8)
// SkiRace_TimeLimit - lets you define a time limit, in seconds (if not defined, will be 20 x gate count)
//
// ambient crowd noise:
// - gets louder if the first 2/3 players are close to each other
// - collective gasp if first place loses more than 40% of speed inside a second (from a deadstop, flubbing the route, or getting MA'ed)
// - cheering at the end

if( $Host::SkiRacePhaseThroughPlayers $= "" ) {
	$Host::SkiRacePhaseThroughPlayers = 0;
}

exec("scripts/SkiFreeDatablock.cs");
exec("scripts/SkiFreeOverrides.cs");
exec("scripts/SkiFreeAI.cs");

function SkiFreeGame::sendGameVoteMenu( %game, %client, %key ) {
	DefaultGame::sendGameVoteMenu(%game, %client, %key);
	
	if(%client.isAdmin) {
		if( $Host::SkiRacePhaseThroughPlayers ) {
			messageClient( %client, 'MsgVoteItem', "", %key, 'VotePhaseThroughPlayers', "", 'SkiFree: Turn Player Phasing OFF');
		}
		else {
			messageClient( %client, 'MsgVoteItem', "", %key, 'VotePhaseThroughPlayers', "", 'SkiFree: Turn Player Phasing ON');
		}
	}
}

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

		if( mAbs(getWord(%targetObject.position, 2) - %h) > 0.01 ) {
			// deadstops happen on the ground
			return;
		}
		
		for( %x = -1; %x <= 1; %x++ ) {
			for( %y = -1; %y <= 1; %y++ ) {
				%pos = (getWord(%targetObject.position, 0) + %x) SPC (getWord(%targetObject.position, 1) + %y);
				%nh = %game.findHeight(%pos);
				if( %minh > %h ) %minh = %h;
				if( %maxh < %h ) %maxh = %h;
			}
		}
		
		if( %minh != %maxh ) {
			//error("deadstop detected at" SPC %targetObject.position SPC "but variance is" SPC mAbs(%minh - %maxh) SPC "- no explosion");
			return;
		}
		// check the terrain height - should be close to 0

		error("deadstop detected at" SPC %targetObject.position SPC "- kill the run");
		
		// what if, instead of exploding, we just go back to playing? just kidding! unless...
		// it is possible, though unlikely, to cut out like 50% of deadstops (the ones that do damage)
		// you would need to remove ground damage and schedule it to after the deadstop check
		// so it is possible, but getting to 100% is definitely not going to be easy
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
		%charge.schedule(200, setDamageState, "Destroyed");
		%charge.schedule(200, blowup);
		%charge.schedule(2000, delete);
	}
}


function SkiFreeGame::initGameVars(%game) {
	// running tallies
	%game.gate = 0;
	
	// player variables
	%game.lifeTime        = 60 * 1000; // length of run
	%game.warningTime     = 10 * 1000; // amount of time remaining until warning
	%game.followTime      =  5 * 1000; // if someone follows you off the spawn platform in this amount of time, give a message

	//%game.heartbeatTime         = 1 * 1000; // would be used for giving waypoints if it worked

	// auto-generation variables
	%game.firstGateMin   =  800;
	%game.firstGateMax   = 1100;
	%game.extraGateMin   =  600;
	%game.extraGateMax   =  900;
	%game.minGateAngle   = 45 * (3.1415927 / 180); // should be up to 22.5 degrees offset from last gate
	%game.angleIncrement = 15 * (3.1415927 / 180);
	%game.maxGateAngle   = 90 * (3.1415927 / 180); // should be up to 45.0 degrees offset from last gate
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

//function SkiFreeGame::heartbeat(%game, %player) {
//	if( !isObject(%player.client) ) return;
//	if( %player.getState() $= "Dead" ) return;
//
//	%client = %player.client;
//	
//	%gate = %player.gate $= "" ? 1 : %player.gate;
//	
	// couldn't get this to work - dunno why. does it not accept waypoint as a target?
	//%client.setTargetId(nameToID("GatePoint" @ %gate).target);
	//commandToClient(%client, 'TaskInfo', %client, -1, false, "> > > > > > [Gate " @ %gate @ "] < < < < < <");
	//%client.sendTargetTo(%client, false);
	
//	%game.schedule(%game.heartbeatTime, heartbeat, %player);
//}

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
	%client.maxGates = 0;
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
	
	if( %damageType == $DamageType::Ground && %player.modGlass ) {
		// explode tha player into tiny pieces and make them watch
		%player.blowup();
		%player.schedule(0, setVelocity, "0 0 0");

		// make the shattering louder by doing it multiple times, lol
		messageClient(%clVictim, 0, '~wfx/Bonuses/horz_perppass3_glasssmash.wav');
		messageClient(%clVictim, 0, '~wfx/Bonuses/horz_perppass3_glasssmash.wav');
	}
	
	// make sure we started and that this is a valid run
	if( %player.launchTime !$= "" && %player.gate > 1 ) {
		// if we skipped a gate, don't bother
		if( %damageType != $DamageType::ForceFieldPowerup ) {
			// calculate score for the run v2
			%score = 0;
			for( %i = 1; %i < %player.gate; %i++ ) {
				%score += nameToID("GatePoint" @ %i).gateDistance;
				//echo("dist after gate " SPC %i SPC %distance);
			}
			
			// measure distance between the current point and the next point, then compare to player/next point. maximum %game.scorePerGate - 1
			%nextGate = nameToID("GatePoint" @ %player.gate);
			%nextPos = getWords(%nextGate.position, 0, 1) SPC "0";
			%playerPos = getWords(%player.position, 0, 1) SPC "0";
			
			%progress = %nextGate.gateDistance - vectorDist(%nextPos, %playerPos);
			if( %progress < 0 ) %progress = 0;
			
			%score += %progress;
			%score = mFloor(%score * 10) / 10;

			%playerName = stripChars( getTaggedString( %clVictim.name ), "\cp\co\c6\c7\c8\c9" );
			
			if( %player.handicap $= "NONE" ) {
				%handicap = "";
			}
			else {
				%handicap = %player.handicap @ " ";
			}

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
			if( %clVictim.score < %score ) {
				%clVictim.score = %score;
				%clVictim.maxGates = (%player.gate - 1);
				
				for( %i = 1; %i < 420; %i++ ) {
					if( %player.curGateTime[%i] $= "" ) break;
					%clVictim.gateTime[%i] = %player.curGateTime[%i];
				}

				%game.recalcScore(%clVictim);
				
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
			}
			
			messageAllExcept(%clVictim, -1, 0, '%1 did a %2m %5run (%3 gates).%4', %playerName, %score, %player.gate - 1, %rankOthers, %handicap);
			messageClient(%clVictim, 0, '\c2That %5run went %2m (%3 gates).%4', %playerName, %score, %player.gate - 1, %rankPersonal, %handicap);
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
	//	cancel(%client.waypointSchedule); // no need to cancel something that doesn't exist
	}
	
	// turn off phasing in case we're moving to a new gametype
	%game.phaseThroughPlayers(false);
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
	messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,247,430>\tPLAYER\tRUNS (FULL)\tBEST');

	for (%index = 0; %index < $TeamRank[0, count]; %index++)
	{
		//get the client info
		%cl = $TeamRank[0, %index];

		%clStyle = %cl == %client ? "<color:dcdcdc>" : "";
		
		%score = mFloor(%cl.score) == %cl.score
			? %cl.score @ ".0"
			: %cl.score;
			
		if( %cl.AI_skiFreeBotLevel !$= "" ) {
			%botLevel = "<spush><color:ff8080>Lv" @ %cl.AI_skiFreeBotLevel @ "<spop>";
		}
		else {
			%botLevel = "";
		}


		//if the client is not an observer, send the message
		if (%client.team != 0)
		{
			messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20,450,500>\t<clip:200>%1</clip><rmargin:200><just:right>%8<rmargin:304><just:right>%2 (%7)<rmargin:461><just:right>%3<rmargin:580><just:left> (%4 gates)', 
				%cl.name, %cl.runs, %score, %cl.maxGates, %clStyle, %cl, %cl.fullRuns, %botLevel
			);
		}
		//else for observers, create an anchor around the player name so they can be observed
		else
		{
			messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20,450,500>\t<clip:200><a:gamelink\t%6>%1</a></clip><rmargin:200><just:right>%8<rmargin:304><just:right>%2 (%7)<rmargin:461><just:right>%3<rmargin:580><just:left> (%4 gates)', 
				%cl.name, %cl.runs, %score, %cl.maxGates, %clStyle, %cl, %cl.fullRuns, %botLevel
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
	
	// all players are team1 - let them watch each other
	setTargetAlwaysVisMask(1, 1 << 1);
	setTargetFriendlyMask(1, 1 << 1);

	// change gate colors
	%game.setSensorWaypointColors();
	
	%game.phaseThroughPlayers($Host::SkiRacePhaseThroughPlayers);
}

function SkiFreeGame::setSensorWaypointColors(%game) {
	setSensorGroupColor(1, 1 << 2, "255 0 0 255");
	setSensorGroupColor(1, 1 << 3, "255 128 0 255");
	setSensorGroupColor(1, 1 << 4, "255 255 0 255");
	setSensorGroupColor(1, 1 << 5, "0 255 0 255");
	setSensorGroupColor(1, 1 << 6, "0 0 255 255");
	setSensorGroupColor(1, 1 << 7, "128 0 255 255");
	setSensorGroupColor(1, 1 << 8, "255 0 255 255");
	setSensorGroupColor(1, 1 << 9, "255 255 255 255");
	// getting to these gates is really, really hard so there isn't much reason to worry about how they look
	setSensorGroupColor(1, 1 << 10, "204 204 204 255");
	setSensorGroupColor(1, 1 << 11, "153 153 153 255");
	setSensorGroupColor(1, 1 << 12, "102 102 102 255");
	setSensorGroupColor(1, 1 << 13, "51 51 0 255");
	setSensorGroupColor(1, 1 << 14, "0 0 0 255");
	setSensorGroupColor(1, 1 << 15, "0 0 0 128");
	setSensorGroupColor(1, 1 << 16, "0 0 0 64");
}

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

	%gateColor = %gate + 1;
	if( %gateColor > 16 ) %gateColor = 15;

	%waypointObj = new WayPoint("GatePoint" @ %gate) {
		position = %waypointPosition;
		rotation = "1 0 0 0";
		scale = "1 1 1";
		dataBlock = "WayPointMarker";
		team = %gateColor;
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
	%gateType = "SkiFreeGateField" @
		(%gate <= 7 ? %gate : "")
	;
	%gateObj = new ForceFieldBare("GateFF" @ %gate) {
		position = %gatePosition;
		rotation = "1 0 0 0";
		scale = "25 25 500";
		dataBlock = %gateType;
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

function SkiFreeGame::leaveSpawnTrigger(%game, %player) {
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;
	
	// make sure we didn't jump the gun already
	if( %player.launchTime !$= "" ) return;
	
	// check if we're ai controlling ourselves and if we meant to launch
	if( %player.client.isAIControlled() && !%player.AI_meantToLaunch && %player.getControllingClient() == %player.client ) {
		// just set the player back on the platform
		if( !isObject(%player.resetThread) ) {
			%player.resetThread = Game.schedule(2000, AI_resetPosition, %player.client, %player);
		}
		return;
	}
	
	// TODO check if time is about to expire and inform the player that they can't actually finish another run

	// start the run for this player
	%mod = "";
	
	if( %player.getDamageLevel() >= 0.649 ) {
		%player.modGlass = 1;
		%mod = %mod @ "glass ";

		%player.setInventory(RepairKit, 0);
		Game.schedule(1000, GlassModeAnticheat, %player); // in case we ate a repair kit at exactly the right time
	}
	else if( %player.getInventory("Disc") == 0 ) {
		// check velocity
		if( !%player.hasDiscjumped ) {
			%mod = %mod @ "discless ";
		}
		else {
			%mod = %mod @ "discless discjump launch ";
		}
	}
	
	if( %player.getInventory("EnergyPack") == 0 ) {
		%player.modHard = 1;
		%mod = %mod @ "hard mode ";
	}
	
	if( %mod !$= "" ) {
		%player.handicap = getSubStr(%mod, 0, strlen(%mod) - 1);
	}
	else {
		%player.handicap = "NONE";
	}
	
 	%client = %player.client;
	messageClient(%client, 0, '\c2Your %1run has begun.~wfx/misc/target_waypoint.wav', %mod);
	%player.schedule(Game.lifeTime, scriptKill, $DamageType::NexusCamping);
	Game.schedule(Game.lifeTime - Game.warningTime, warningMessage, %player);
	%player.launchTime = getSimTime();
	Game.lastLaunchTime = getSimTime();
	%player.gate = 1;
	
	// remove invincibility
	%player.setInvincibleMode(0 ,0.00);
	%player.setInvincible( false );
	
	Game.checkFollowingPlayers(%player);
	Game.schedule(Game.followTime, listFollowingPlayers, %player);
}

function SkiFreeGame::enterSpawnTrigger(%game, %player) {
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;
	
	// we jumped the gun
	if( %player.launchTime !$= "" && !%player.noMulligans ) {
		%player.noMulligans = true;
		%client = %player.client;
		messageClient(%client, 0, '\c2Hey, no mulligans! Your timer is still running!~wfx/packs/shield_hit.wav');
	}
}

function SkiFreeGame::checkFollowingPlayers(%game, %player) {
	%client = %player.client;
	
	for( %i = 0; %i < ClientGroup.getCount(); %i++ ) {
		%clTarget = ClientGroup.getObject(%i);
		if( %clTarget == %client ) continue;
		%plTarget = %clTarget.player;
		if( !isObject( %plTarget ) || %plTarget.launchTime $= "" ) continue;

		// get everyone that started a run within 5 seconds of you
		if( %player.launchTime - %game.followTime <= %plTarget.launchTime ) {
			// three different variables based on handicap
			if( %plTarget.handicap $= "NONE" ) {
				%plTarget.following++;
				%plTarget.followName = %client.name;
			}
			else if( %plTarget.handicap $= %player.handicap ) {
				%plTarget.followingMatch++;
				%plTarget.followMatchName = %client.name;
			}
			else if( %plTarget.handicap !$= %player.handicap ) {
				%plTarget.followingNoMatch++;
				%plTarget.followNoMatchName = %client.name;
			}
			
			if( %player.handicap $= "NONE" ) {
				%player.following++;
				%player.followName = %clTarget.name;
			}
			else if( %player.handicap $= %plTarget.handicap ) {
				%player.followingMatch++;
				%player.followMatchName = %clTarget.name;
			}
			else if( %player.handicap !$= %plTarget.handicap ) {
				%player.followingNoMatch++;
				%player.followNoMatchName = %clTarget.name;
			}
		}
	}
}

function SkiFreeGame::listFollowingPlayers(%game, %player) {
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;
	
	%client = %player.client;
	
	if( %player.handicap $= "NONE" ) {
		if( %player.following > 1 )
			messageClient(%client, 0, 'You are in a run with %1 other players.', %player.following);
		else if( %player.following == 1 )
			messageClient(%client, 0, 'You are in a run with %1.', %player.followName);
	}
	else {
		if( %player.followingMatch > 1 )
			messageClient(%client, 0, 'You are in a %2 run with %1 other players.', %player.followingMatch, %player.handicap);
		else if( %player.followingMatch == 1 )
			messageClient(%client, 0, 'You are in a %2 run with %1.', %player.followMatchName, %player.handicap);
			
		if( %player.followingNoMatch > 1 )
			messageClient(%client, 0, '%1 players did not choose the same handicap.', %player.followingNoMatch);
		else if( %player.followingNoMatch == 1 )
			messageClient(%client, 0, '%1 did not choose the same handicap as you.', %player.followNoMatchName);
			
	}
}

function SkiFreeGame::GlassModeAntiCheat(%game, %player) {
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;

	if( %player.getDamageLevel() < 0.649 ) {
		%player.setDamageLevel(0.65);
		Game.schedule(100, GlassModeAnticheat, %player);
	}
}


function SkiFreeGame::enterGateTrigger(%game, %trigger, %player) {
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;
	
	if( %trigger.gate == %player.gate ) {
		// ready the next gate
		%player.gate++;
		if( !%player.modGlass ) %player.applyRepair(0.125);
		%player.setInventory(DiscAmmo, 15);

		// TODO waypoint
		
		// get other variables
		%timeMS = (getSimTime() - %player.launchTime) / 1000;
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
		
		// deal with ai
		if( %player.client.isAIControlled() ) {
			%game.AI_crossedGate(%player.client, %player);
		}
	}
	else if( %trigger.gate > %player.gate ) {
		messageClient(%player.client, 0, '\c2GATE SKIP DETECTED!~wfx/misc/red_alert_short.wav');
		%player.scriptKill($DamageType::ForceFieldPowerup);
	}
}

function SkiFreeGame::warningMessage(%game, %player) {
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
	// clear this shit out just in case
	deleteVariables("$SkiFreeTerrainList*");

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

	else if( %damageType == $DamageType::Ground && %clVictim.player.modGlass ) {
	      messageAll('msgSelfKill', '%1 shattered into a million pieces.', %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
      logEcho(%clVictim.nameBase@" (pl "@%clVictim.player@"/cl "@%clVictim@") killed self ("@getTaggedString($DamageTypeText[%damageType])@")");
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
	messageClient( %client, 'MsgDebriefAddLine', "", '<spush><color:00dc00><font:univers condensed:18>PLAYER<lmargin%%:60>BEST<lmargin%%:80>GATES<spop>' );
	for ( %i = 0; %i < %count; %i++ ) {
		%cl = $TeamRank[0, %i];
		if ( %cl.score $= "" )
			%score = "0.0";
		else if( mFloor(%cl.score) == %cl.score )
			%score = %cl.score @ ".0";
		else
			%score = %cl.score;

		if( %cl.maxGates $= "" )
			%gates = 0;
		else
			%gates = %cl.maxGates;

		messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:60><clip%%:20> %2</clip><lmargin%%:80><clip%%:20> %3', %cl.name, %score, %gates );
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
         if ( %cl.score $= "" )
            %score = "0.0";
         else if( mFloor(%cl.score) == %cl.score )
            %score = %cl.score @ ".0";
         else
            %score = %cl.score;

         messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2</clip>', %cl.name, %score);
      }
   }
}

function SkiFreeGame::phaseThroughPlayers(%game, %active) {
	// don't run unneeded mempatches!
	if( !%active && !%game.phaseActive ) return;
	%game.phaseActive = %active;
	
	// this code adds/removes the player mask from player collisions
	// so players can phase right through each other if this is active
	// thanks to DarkTiger for this code
	%patch1 = %active ? "3CA1" : "3CE1";
	%patch2 = %active ? "1C21" : "1C61";
	%patch3 = %active ? "1C21" : "1C61";
	
	memPatch("83FBF4", %patch1);
	memPatch("79B40C", %patch2);
	memPatch("83FBF8", %patch3);
}
