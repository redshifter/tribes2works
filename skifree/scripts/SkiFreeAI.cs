// SkiFreeAI.cs
// no, not aiSkiFree.cs - that naming convention is stupid

// level 1 - the bot will just be handled by the game
// level 2 - ski down hills and jet up/over hills

function SkiFreeGame::AIHasJoined(%game, %client) {
	%skill = %client.getSkillLevel();
	if( %skill < 0.5 ) {
		%client.AI_skiFreeBotLevel = 1;
	}
	else {
		%client.AI_skiFreeBotLevel = 2;
	}
}
function SkiFreeGame::AIinit(%game) {
	// keeps ai from spamming the console - no idea what else it does
	AIInit();
}

function SkiFreeGame::onAIRespawn(%game, %client) {
	// start a heartbeat
	%player = %client.player;
	%game.schedule(1000, AI_heartbeat, %client, %player);

	// give a vgcg (but don't make every bot do it, to avoid spam)
	if( getSimTime() - 500 >= %game.AI_lastGoodGame ) {
		schedule(100, 0, AIPlay3DSound, %client, "gbl.goodgame");
		%game.AI_lastGoodGame = getSimTime();
	}
	
	// have you ever heard 16 bots all with disc launchers out? not a fun experience
	%player.schedule(0, use, TargetingLaser);
}

function SkiFreeGame::AI_heartbeat(%game, %client, %player) {
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;
	
	// check what our current task is
	if( %player.getControllingClient() != %player.client ) {
		// we're possessed (probably for debugging purposes) - don't do anything weird
		%heartbeat = 1000;
	}
	if( !%player.AI_meantToLaunch ) {
		%game.AI_mingle(%client, %player);
		%heartbeat = 1000;
	}
	else if( %client.AI_skiFreeBotLevel <= 1 ) {
		// level 1 is just "turn off the heartbeat and use the standard bot behavior"
		// it already knows where it needs to go
		return;
	}
	else if( %client.AI_skiFreeBotLevel == 2 ) {
		// 20ms resolution is needed for more direct control of a bot
		%game.AI_playGameLevel2(%client, %player);
		%heartbeat = 20;
	}
	
	// continue calling heartbeat to see what we should be doing
	%game.schedule(%heartbeat, AI_heartbeat, %client, %player);
}

function SkiFreeGame::AI_mingle(%game, %client, %player) {
	// PRIORITY 1. if alone in the game for 5 beats (no non-AI players on team1) and none of the AI are in the field, start a run
	%alone = true;
	for( %i = 0; %i < ClientGroup.getCount(); %i++ ) {
		%cl = ClientGroup.getObject(%i);
		if( !%cl.isAIControlled() && %cl.team == 1 ) {
			// player in game - bots only go if someone else goes
			%alone = false;
			break;
		}
		else if( %cl.isAIControlled() ) {
			if( !isObject(%cl.player) ) {
				%alone = false;
				break;
			}
			else if( %cl.player.launchTime !$= "" ) {
				%alone = false;
				break;
			}
		}
	}
	if( %alone ) {
		%player.AI_alone++;
		if( %player.AI_alone >= 5 ) {
			%game.AI_startRun(%client, %player);
			return;
		}
	}
	else {
		%player.AI_alone = 0;
	}
	
	// PRIORITY 2. if someone just jumped off the platform, start a run behind them
	if( getSimTime() - 2000 < %game.lastLaunchTime ) {
		%game.AI_startRun(%client, %player);
		return;
	}
	
	// PRIORITY 3. if a new (non-AI) player spawned, say hello
	for( %i = 0; %i < ClientGroup.getCount(); %i++ ) {
		%cl = ClientGroup.getObject(%i);
		if( !%cl.isAIControlled()
			&& %cl.team == 1
			&& isObject(%cl.player)
			&& !%cl.player.AI_sentGreeting
			&& isObject(%cl.player.client)
			&& %cl.player.getState() !$= "Dead"
		) {
			%cl.player.AI_sentGreeting = true; // only one bot should greet the player
			schedule(250, %client, "AIPlayAnimSound", %client, %cl.player.position, "gbl.hi", $AIAnimWave, $AIAnimWave, 0);
			return;
		}
	}
}

function SkiFreeGame::AI_startRun(%game, %client, %player) {
	// start the run
	%player.AI_meantToLaunch = 1;
	%client.stepMove(GatePoint1.position, 5);
	%player.use(Disc);
}

function SkiFreeGame::AI_crossedGate(%game, %client, %player) {
	// is there anything else to do here
	%client.stepMove(nameToID("GatePoint" @ %player.gate).position, 5);
}

// in case some asshole knocks a bot off the platform
function SkiFreeGame::AI_resetPosition(%game, %client, %player) {
	// what the fuck did you do to him?!
	if( !isObject(%player.client) ) return;
	if( %player.getState() $= "Dead" ) return;
	
	%player.setTransform(Game.pickPlayerSpawn(%client));
	%player.setVelocity("0 0 0");
	%player.setDamageLevel(0);
	%player.resetThread = "";
	
	// watch where you're shooting
	schedule(100, 0, AIPlay3DSound, %client, "wrn.watchit");
}

function SkiFreeGame::AI_playGameLevel2(%game, %client, %player) {
	// return if we haven't launched yet - just do standard bot things up to that point
	if( %player.launchTime $= "" ) return;
	
	// disable the bot's jets when i don't tell you to use them
	if( %player.storedEnergy > 0 ) {
		%player.setEnergyLevel(%player.getEnergyLevel() + %player.storedEnergy);
		%player.storedEnergy = 0;
	}
	%fireJets = false;
	
	%aiHandled = false;
	
	// PRIORITY 1. check distance to gate - if we're very close, return from method (use default bot behavior)
	// TODO check distance to gate - if we're going fast, check trajectory. if we're off, return from method (use default bot behavior)
	%dist = VectorDist(%player.position, nameToID("GatePoint" @ %player.gate));
	%vel = %player.getVelocity();
	// TODO remove this shit and make it good
	if( %dist < 100 ) {
		return;
	}
	//%speed = VectorLen(%vel);
	//if( %speed >= 70 ) {
		// i have no idea how to do this shit
	//}

	//%dir = VectorNormalize(getWords(%vel,0,1) SPC "0");
	%dir = VectorSub(%player.position, nameToID("GatePoint" @ %player.gate).position);
	%dir = VectorNormalize(getWords(%dir, 0, 1) SPC "0");
	%dir = VectorScale(%dir, 2);
	%x = getWord(%player.position, 0);
	%y = getWord(%player.position, 1);
	%terrainHeight = %game.findHeight(%x SPC %y);
	
	// PRIORITY 2. check slope
	// going down slope - hold jump
	// going up slope - hit the jets, unless too high (if there's a mountain in front of you, there's no such thing as too high)
	// TODO this is too naive - need slope to left and right to know if we're getting pushed off-course
	// TODO also use parabolas
	
	if( !%aiHandled ) {
		%xslope = %x + getWord(%dir, 0); %yslope = %y + getWord(%dir, 1);
		%hslope = %game.findHeight(%xslope SPC %yslope);
		%slope = %terrainHeight - %hslope;
		
		if( %slope < 0.02 ) {
			// going down
			%client.pressjump();
			%aiHandled = true;
		}
		else { 
			// going up
			// make sure we aren't too high off the ground - 10m is plenty
			// TODO we need to do a distance check
			if( getWord(%player.position,2) - %terrainHeight < 10 ) {
				%fireJets = true;
				%aiHandled = true;
			}
			else {
				%hit = ContainerRayCast(%player.position, VectorScale(%dir, 50) SPC getWord(%player.position,2) - 25, $TypeMasks::TerrainObjectType);
				if( VectorDist(%player.position, getWords(%hit, 1, 3)) < 25 ) {
					%fireJets = true;
					%aiHandled = true;
				}
			}
		}
	}
	
	if( %fireJets ) {
		%client.pressjump();
		%client.pressjet();
	}
	else {
		%player.storedEnergy = %client.player.getEnergyLevel();
		%player.setEnergyLevel(0);
	}
}