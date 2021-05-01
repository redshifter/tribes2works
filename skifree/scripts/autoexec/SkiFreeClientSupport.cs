// this method is for server
function serverCmdSkiFreePassport(%client, %version) {
	%client.SkiFreePassport = %version;
}

// rest of this shit is only for client
if( $LaunchMode $= "DedicatedServer" ) return;

package RegisterSkiFreeClient {
	// register with the server
	function clientCmdMissionStartPhase2(%seq) {
		commandToServer('SkiFreePassport', 1);
		Parent::clientCmdMissionStartPhase2(%seq);
	}

	function lobbyDisconnect() {
		// export blueprints
		if( $SkiFreeDirty ) {
			saveSkiFreePrefs();
		}
		allowSkiFreePhasing(0, 0, false);
		$SkiFreeClientTimer = "";
		Parent::lobbyDisconnect();
	}
	
	function setupObjHud(%gameType)
	{
		if (%gameType $= SkiFreeGame) {
			setupSkiFreeObjHud();
		}
		Parent::setupObjHud(%gameType);
	}

};
if( !isActivePackage(RegisterSkiFreeClient) ) activatePackage(RegisterSkiFreeClient);

function clientCmdSendSkiFreeBlueprint(%fields) {
	for( %i = 0; %i < $SkiFreeBlueprintCount; %i++ ) {
		if( $SkiFreeBlueprint[%i,Map] $= %fields ) return;
	}
	%i = $SkiFreeBlueprintCount;
	if( %i $= "" ) %i = 0;
	$SkiFreeBlueprintCount++;
	$SkiFreeBlueprint[%i,Map] = %fields;
	$SkiFreeBlueprint[%i,Date] = formatTimeString("yy-mm-dd"); // note: time zone constricted
	$SkiFreeBlueprint[%i,Epoch] = currentEpochTime(); // tribesnext method that uses rubyEval - should be fine?
	$SkiFreeBlueprint[%i,Attempts] = 0;
	$SkiFreeBlueprint[%i,Completions] = 0;

	$SkiFreeCurrentBlueprintMap = %i;
	
	saveSkiFreePrefs();
}

function clientCmdSendSkiFreeStart(%attempts) {
	if( $SkiFreeCurrentBlueprintMap !$= "" ) {
		//$SkiFreeBlueprint[$SkiFreeCurrentBlueprintMap,Attempts] = %attempts;
		$SkiFreeBlueprint[$SkiFreeCurrentBlueprintMap,Attempts]++;

		// don't save every single time you come off the line - that's way too many writes
		$SkiFreeDirty = true;
	}
	
	// start the timer
	$SkiFreeClientTimer = getSimTime();
	updateSkiFreeClientTimer($SkiFreeClientTimer, 0);
}

function clientCmdCorrectSkiFreeDrift(%drift) {
	$SkiFreeClientTimer = getSimTime();
	updateSkiFreeClientTimer($SkiFreeClientTimer, %drift);
}	

function updateSkiFreeClientTimer(%start, %drift) {
	if( %start != $SkiFreeClientTimer ) return;
	if( !isObject(objectiveHud.activeTimer) ) return;
	
	%timeMS = (getSimTime() - %start) + %drift;
	%timeMS = mFloor(%timeMS / 100) / 10;

	if( strpos(%timeMS, ".") == -1 ) %timeMS = %timeMS @ ".0";
	
	objectiveHud.activeTimer.setText(%timeMS);
	
	schedule(100, 0, updateSkiFreeClientTimer, %start, %drift);
}

function clientCmdSendSkiFreeStop() {
	$SkiFreeClientTimer = "";
}

function clientCmdSendSkiFreeEnd(%completions, %isBestTime, %fields) {
	$SkiFreeClientTimer = "";

	if( $SkiFreeCurrentBlueprintMap !$= "" ) {
		//$SkiFreeBlueprint[$SkiFreeCurrentBlueprintMap,Completions] = %completions;
		$SkiFreeBlueprint[$SkiFreeCurrentBlueprintMap,Completions]++;
		
		if( %isBestTime ) {
			$SkiFreeBlueprint[$SkiFreeCurrentBlueprintMap,Fields] = %fields;
		}
		
		saveSkiFreePrefs();
	}
}

addMessageCallback('MsgSkiFreeLeaderScoreIs', "updateSkiFreeLeader");
addMessageCallback('MsgSkiFreePhaseThroughPlayers', "allowSkiFreePhasing");

function updateSkiFreeLeader(%msgType, %msgString, %bestTime) {
	if( !isObject(objectiveHud.bestScore) ) return;
	if( %bestTime $= "" || %bestTime == 300 ) {
		objectiveHud.bestScore.setText("<none>");
	}
	else {
		objectiveHud.bestScore.setText(%bestTime);
	}
}

// the most dangerous of skifree commands. tell players they can phase through each other (note: if this doesn't match the server setting, you're gonna have a bad time)
function allowSkiFreePhasing(%msgType, %msgString, %active) {
	// only do phasing comamnds we should
	if( $SkiFreeLocalPhasingActive $= "" && !%active) return;
	if( $SkiFreeLocalPhasingActive == %active ) return;
	$SkiFreeLocalPhasingActive = %active;

	%patch1 = %active ? "3CA1" : "3CE1";
	%patch2 = %active ? "1C21" : "1C61";
	%patch3 = %active ? "1C21" : "1C61";
	
	echo("SkiFree is injecting patches to turn "
		@ (%active ? "ON" : "OFF")
		@ " the ability to phase through players..."
	);
	
	memPatch("83FBF4", %patch1);
	memPatch("79B40C", %patch2);
	memPatch("83FBF8", %patch3);
}

// epoch time must be defined before prefs loading!
// STOLEN FROM T2CSRI - WE NEED THEM DEFINED HERE IN CASE THEY'RE NOT ALREADY IN PLACE
// gets the current Unix Epoch time from Ruby -- in seconds
function currentEpochTime()
{
	$temp = "";
	rubyEval("tsEval '$temp=' + Time.now.to_i.to_s + ';'");
	return $temp;
}

// compute the addition in Ruby, due to the Torque script precision problems for >1e6 values
function getEpochOffset(%seconds)
{
	$temp = "";
	rubyEval("tsEval '$temp=' + (Time.now.to_i + " @ %seconds @ ").to_s + ';'");
	return $temp;
}

// save with base qualifier
function saveSkiFreePrefs() {
	export("$SkiFreeBlueprint*", "../base/prefs/SkiFreeBlueprint.cs", false);
	$SkiFreeDirty = false;
	echo("Exported to base/prefs/SkiFreeBlueprint.cs");
}

// load without base qualifier
function loadSkiFreePrefs() {
	if( isFile("prefs/SkiFreeBlueprint.cs") )
		exec("prefs/SkiFreeBlueprint.cs");
	
	// garbage collection
	%modified = skifreeGarbageCollect();
	if( %modified ) {
		saveSkiFreePrefs();
	}
}

function skifreeGarbageCollect() {
	%modified = false;
	
	%killWithRun =   "2678400";
	%killWithoutRun = "604800";
	for( %i = 0; %i < $SkiFreeBlueprintCount; %i++ ) {
		// skip all favorites
		if( $SkiFreeBlueprint[%i,Favorite] ) continue;
		
		// the "-" needs to be concatenated or you are going to have conversion problems
		%offset = getEpochOffset("-" @ $SkiFreeBlueprint[%i,Epoch]);
		if( %offset $= "" ) return false; // don't delete everything if we don't have ruby active!
		%killPoint = ($SkiFreeBlueprint[%i,Fields] $= "" ? %killWithoutRun : %killWithRun);
		//error(%i SPC %offset SPC %killPoint);
		
		if( %offset < %killPoint ) {
			continue;
		}
		
		killSkiFreeEntry(%i);
		%i--; // reset
		%modified = true;
	}
	
	return %modified;
}

function killSkiFreeEntry(%i) {
	//error("killing " @ %i @ " (" @ $SkiFreeBlueprint[%i,Map] @ ")");
	for( %j = %i; %j < $SkiFreeBlueprintCount; %j++ ) {
		$SkiFreeBlueprint[%j,Attempts]    = $SkiFreeBlueprint[%j+1,Attempts];
		$SkiFreeBlueprint[%j,Completions] = $SkiFreeBlueprint[%j+1,Completions];
		$SkiFreeBlueprint[%j,Date]        = $SkiFreeBlueprint[%j+1,Date];
		$SkiFreeBlueprint[%j,Epoch]       = $SkiFreeBlueprint[%j+1,Epoch];
		$SkiFreeBlueprint[%j,Favorite]    = $SkiFreeBlueprint[%j+1,Favorite];
		$SkiFreeBlueprint[%j,Fields]      = $SkiFreeBlueprint[%j+1,Fields];
		$SkiFreeBlueprint[%j,Map]         = $SkiFreeBlueprint[%j+1,Map];
	}
	$SkiFreeBlueprint[$SkiFreeBlueprintCount - 1,Attempts]    = "";
	$SkiFreeBlueprint[$SkiFreeBlueprintCount - 1,Completions] = "";
	$SkiFreeBlueprint[$SkiFreeBlueprintCount - 1,Date]        = "";
	$SkiFreeBlueprint[$SkiFreeBlueprintCount - 1,Epoch]       = "";
	$SkiFreeBlueprint[$SkiFreeBlueprintCount - 1,Favorite]    = "";
	$SkiFreeBlueprint[$SkiFreeBlueprintCount - 1,Fields]      = "";
	$SkiFreeBlueprint[$SkiFreeBlueprintCount - 1,Map]         = "";
	
	$SkiFreeBlueprintCount--;
}

loadSkiFreePrefs();

function setupSkiFreeObjHud() {
	objectiveHud.setSeparators("49 150 194");

	// row 1
	objectiveHud.timerLabel = new GuiTextCtrl() {
		profile = "GuiTextObjGreenLeftProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "4 3";
		extent = "41 16";
		visible = "1";
		text = "TIMER";
	};
	objectiveHud.activeTimer = new GuiTextCtrl() {
		profile = "GuiTextObjGreenCenterProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "51 3";
		extent = "98 16";
		visible = "1";
		text = "0.0";
	};
	objectiveHud.scoreLabel = new GuiTextCtrl() {
		profile = "GuiTextObjGreenLeftProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "153 3";
		extent = "41 16";
		visible = "1";
		text = "BEST";
	};
	objectiveHud.yourScore = new GuiTextCtrl() {
		profile = "GuiTextObjGreenCenterProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "195 3";
		extent = "48 16";
		visible = "1";
		text = "<none>";
	};

	// row 2
	objectiveHud.terrainLabel = new GuiTextCtrl() {
		profile = "GuiTextObjGreenLeftProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "4 19";
		extent = "43 16";
		visible = "1";
		text = "TERRAIN";
	};
	objectiveHud.yourTarget = new GuiTextCtrl() {
		profile = "GuiTextObjGreenCenterProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "51 19";
		extent = "98 16";
		visible = "1";
		text = "DangerousCrossing_nef";
	};
	objectiveHud.bestLabel = new GuiTextCtrl() {
		profile = "GuiTextObjGoldLeftProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "153 19";
		extent = "41 16";
		visible = "1";
		text = "LEADER";
	};
	objectiveHud.bestScore = new GuiTextCtrl() {
		profile = "GuiTextObjGoldCenterProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "195 19";
		extent = "48 16";
		visible = "1";
		text = "<none>";
	};
	
	objectiveHud.add(objectiveHud.timerLabel);
	objectiveHud.add(objectiveHud.activeTimer);
	objectiveHud.add(objectiveHud.scoreLabel);
	objectiveHud.add(objectiveHud.yourScore);
	
	objectiveHud.add(objectiveHud.terrainLabel);
	objectiveHud.add(objectiveHud.yourTarget);
	objectiveHud.add(objectiveHud.bestLabel);
	objectiveHud.add(objectiveHud.bestScore);
}