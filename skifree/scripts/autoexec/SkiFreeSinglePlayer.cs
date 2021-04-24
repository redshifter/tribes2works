// the methods in this CS file will only run if this is a client. it won't run if this is a dedicated server
if( $LaunchMode $= "DedicatedServer" ) return;

exec("scripts/SkiFreeMetadata.cs");

package SkiFreeTabPackage {

function OpenLaunchTabs() {
	Parent::OpenLaunchTabs();
	
	if( !isObject(SkiFreeGui) ) {
		createSkiFreeLoaderGui();
		LaunchTabView.addLaunchTab( "SKIFREE", SkiFreeGui );
	}
}

// remove skifree maps from the training menu
function TrainingGui::onWake( %this ) {
	Parent::onWake(%this);
	
	// remove skifree from the training menu
	for( %i = TrainingMissionList.rowCount() - 1; %i >= 0; %i-- ) {
		if( strpos(TrainingMissionList.getRowText(%i), "SkiFree") > -1 ) {
			TrainingMissionList.removeRow(%i);
		}
	}
	TrainingMissionList.setSelectedRow(0);
}

// change the skifree challenge menu into the standard single player dialog
function escapeFromGame()
{
	if( strpos($CurrentMission, "SkiFreeSP") == 0 ) {
		Canvas.pushDialog( SinglePlayerEscapeDlg );
	}
	else {
		Parent::escapeFromGame();
	}
}

// why would you put the i18n object ON THE FUCKING CLIENT OBJECT
function SinglePlayerEscapeDlg::leaveGame( %this )
{
	if( strpos($CurrentMission, "SkiFreeSP") == 0 ) {
		%line = "Thank you for playing.";
		
		%client = ClientGroup.getObject(0);
		if( %client.bestTime != Game.trialDefaultTime ) {
			%line = %line @ "\n\n" @ Game.terrain;
			%line = %line @ "\nTime: " @ %client.bestTime;
			%line = %line @ "\n" @ %client.bestHandicap;
			%line = %line @ "\nVersion " @ $SkiFreeVersionString;
		}
		
		Canvas.popDialog( SinglePlayerEscapeDlg );
		MessageBoxYesNo( "LEAVE GAME", %line, "forceFinish();", "$timeScale = 1;" );
	}
	else {
		Parent::leaveGame(%this);
	}
}

function GuiMLTextCtrl::onURL(%this, %url) {
	%i = 0;
	while((%fld[%i] = getField(%url, %i)) !$= "") %i++;

	%id = %fld[1];
	switch$(%fld[0]) {
	case "SkiFreeAddToFavorites":
		$SkiFreeBlueprint[%id,Favorite] = 1;
		generateSkiFreeText();
		saveSkiFreePrefs();
		
		SkiFreeGui_MissionList.setRowById( SkiFreeGui_MissionList.getSelectedId(), "\c4" @ SkiFreeGui_MissionList.getValue() );
		
	case "SkiFreeRemoveFromFavorites":
		MessageBoxYesNo( "REMOVE FAVORITE", "Are you sure you want to remove this map from your favorites?", "removeSkiFreeFavorite(" @ %id @ ");");
	default:
		Parent::onURL(%this, %url);
	}
}

};
if( !isActivePackage(SkiFreeTabPackage) ) activatePackage(SkiFreeTabPackage);

// TODO add writer support for this tab bullshit

function createSkiFreeLoaderGui() {
	// make the gui
	buildSkiFreeGui();
	
	// set modes
	populateSkiFreeModes();
	
	// default simulated ping
	SkiFreeGui_PingSimSlider.setValue($pref::Net::simPing);
	setSkiFreeSimPing();
	
	validateSkiFreeHasAllMapPacks();
	getVersionNumberForSkiFree();
	helpForSkiFree();
}

function populateSkiFreeModes() {
	SkiFreeGui_Mode.clear();
	SkiFreeGui_Mode.add( "Previous Games", 0 );
	SkiFreeGui_Mode.add( "Choose Terrain", 1 );
	SkiFreeGui_Mode.add( "Randomizers",    2 );
	SkiFreeGui_Mode.add( "Special Maps",   3 );
	
	// TODO select based on setup
	SkiFreeGui_Mode.setSelected($pref::SkiFree::SelectedMode);
	SkiFreeGui_Mode.onSelect($pref::SkiFree::SelectedMode);
	
	// start at help screen regardless of what is selected
	helpForSkiFree();
}

function SkiFreeGui::onWake(%this) {
	// put the launch menu on the bottom of the skifree gui
	Canvas.pushDialog(LaunchToolbarDlg);
}

function SkiFreeGui_Mode::onSelect( %this, %id, %text ) {
	// don't use text in here, lol
	$pref::SkiFree::SelectedMode = %id;
	
	// set modes
	SkiFreeGui_Set.clear();
	if( %id == 0 ) { //
		SkiFreeGui_Set.setActive(1);
		SkiFreeGui_Set.add("Favorites",       0);
		SkiFreeGui_Set.add("Maps with Times", 1);
		SkiFreeGui_Set.add("All Prior Maps",  2);
		SkiFreeGui_Set.setSelected(0);
		SkiFreeGui_RandomSelectBtn.setActive(0);
	}
	else if( %id == 1 ) { // choose terrain
		SkiFreeGui_Set.setActive(1);
		SkiFreeGui_Set.add("Regulation Terrains", 0);
		SkiFreeGui_Set.add("Superhard Terrains",  1);
		SkiFreeGui_Set.add("Rejected Terrains",   2);
		SkiFreeGui_Set.add("Unknown Terrains",    3);
		SkiFreeGui_Set.add("All Terrains",        4);
		SkiFreeGui_Set.setSelected(0);
		SkiFreeGui_RandomSelectBtn.setActive(1);
	}
	else {
		SkiFreeGui_Set.setActive(0);
		SkiFreeGui_RandomSelectBtn.setActive(0);
	}
	
	// populate list
	populateSkiFreeList();
}

function SkiFreeGui_Set::onSelect(%this, %id, %text) {
	populateSkiFreeList();
}

function populateSkiFreeList() {
	%list = SkiFreeGui_MissionList;
	%mode = SkiFreeGui_Mode.getSelected();
	
	%list.clear();
	if( %mode == 0 ) { // previous games
		generateSkiFreePreviousGames();
	}
	else if( %mode == 1 ) { // choose terrain
		generateSkiFreeStandardTerrainList();
	}
	else if( %mode == 2 ) { // daily challenge
		%list.addRow(90001, "Daily Challenge: Easy");
		%list.addRow(90002, "Daily Challenge: Medium");
		%list.addRow(90003, "Daily Challenge: Hard");
		%list.addRow(90004, "Randomizer: Easy");
		%list.addRow(90005, "Randomizer: Medium");
		%list.addRow(90006, "Randomizer: Hard");
	}
	else if( %mode == 3 ) { // tournaments
		// TODO make this an actual parsed list instead of hooking it up to an ID
		%list.addRow(90007, "Spring 2021 Tourney");
	}

	if( %list.rowCount() > 0 ) {
		%list.setSelectedRow(0);
	}
	
	generateSkiFreeText();
}

function generateSkiFreePreviousGames() {
	%list = SkiFreeGui_MissionList;
	%set = SkiFreeGui_Set.getSelected();
	
	for( %i = $SkiFreeBlueprintCount - 1; %i >= 0; %i-- ) {
		if( %set == 1 && $SkiFreeBlueprint[%i,Completions] == 0 ) continue;
		if( %set == 0 && !$SkiFreeBlueprint[%i,Favorite] ) continue;
		
		// start by putting the time down
		//%diff = currentEpochTime() - $SkiFreeBlueprint[%i,Epoch]; // don't ever do this, currentEpochTime() is too big
		%diff = getEpochOffset("-" @ $SkiFreeBlueprint[%i,Epoch]); // do this instead
		if( %diff < 60 * 60 ) {
			%name = mFloor(%diff / 60) @ " mins ago, ";
		}
		else if( %diff < 60 * 60 * 24 ) {
			%name = mFloor(%diff / 60 / 60) @ " hours ago, ";
		}
		else if( %diff < 60 * 60 * 24 * 7 ) {
			%name = mFloor(%diff / 60 / 60 / 24) @ " days ago, ";
		}
		else {
			// TODO ask ruby for a proper date
			%name = $SkiFreeBlueprint[%i,Date] @ ", ";
		}
		
		// put number of runs and terrain
		%name = %name @ $SkiFreeBlueprint[%i,Completions] @ " runs, "
			@ getField($SkiFreeBlueprint[%i,Map], 0);
			
		// favorites should be brighter
		if( $SkiFreeBlueprint[%i,Favorite] ) %name = "\c4" @ %name;

		%list.addRow(%i, %name);
	}
}

function generateSkiFreeStandardTerrainList() {
	%list = SkiFreeGui_MissionList;
	%set = SkiFreeGui_Set.getSelected();

	exec("scripts/SkiFreeTerrains.cs");
	if( %set == 0 ) { // regulation terrains
		for( %i = 0; %i <= $SkiFreeTerrainListMAX; %i++ ) {
			%list.addRow(100000 + %i, $SkiFreeTerrainList[%i]);
			if( !isFile("terrains/" @ $SkiFreeTerrainList[%i]) )
				%list.setRowActive(100000 + %i, 0);
		}
	}
	else if( %set == 1 ) { // superhard terrains (april fools mode)
		for( %i = 0; %i <= $SkiFreeTerrainListSuperHardMAX; %i++ ) {
			%list.addRow(100000 + %i, $SkiFreeTerrainListSuperHard[%i]);
			if( !isFile("terrains/" @ $SkiFreeTerrainListSuperHard[%i]) )
				%list.setRowActive(100000 + %i, 0);
		}
	}
	else if( %set == 2 ) { // rejected terrains
		for( %i = 0; %i <= $SkiFreeTerrainListRejectedMAX; %i++ ) {
			%list.addRow(100000 + %i, $SkiFreeTerrainListRejected[%i]);
			if( !isFile("terrains/" @ $SkiFreeTerrainListRejected[%i]) )
				%list.setRowActive(100000 + %i, 0);
		}
	}
	else if( %set == 3 || %set == 4 ) { // unknown/all terrains
		// parse every TER on the install
		// anything that doesn't start SkiFree or exist on the terrain list is included
		%search = "terrains/*.ter";
		
		%i = 100000;

		for( %file = findFirstFile( %search ); %file !$= ""; %file = findNextFile( %search ) ) {
			%name = fileBase(%file) @ ".ter";
			
			// remove duplicates
			if( %dupe[%name] ) continue;
			%dupe[%name] = 1;
			
			// remove skifree terrains
			if( strpos(%name, "SkiFree") == 0 ) continue;
			
			// remove anything that has metadata in skifree (for unknowns)
			if( %set == 3 && $SkiFreeMeta[%name,MapPack] !$= "" ) continue;
			
			// terrain doesn't exist - add it
			%list.addRow(%i++, %name);
		}
	}
	
	deleteVariables("$SkiFreeTerrainList*");
	%list.sort(0, true);
}

function skiFreeRandomSelect() {
	%rng = getRandom(0, SkiFreeGui_MissionList.rowCount() - 1);
	SkiFreeGui_MissionList.setSelectedRow(%rng);
	generateSkiFreeText();
}

function setSkiFreeSimPing() {
	%ping = mFloor( SkiFreeGui_PingSimSlider.getValue() );
	SkiFreeGui_PingSimText.setValue(%ping @ "ms");
	$pref::Net::simPing = %ping;
}

function buildSkiFreeGui() {
	if( isObject(SkiFreeGui) ) SkiFreeGui.delete(); // rapid prototyping
	exec("gui/SkiFreeGui.gui");
}

function validateSkiFreeHasAllMapPacks() {
	exec("scripts/SkiFreeTerrains.cs");
	$SkiFreeMissingMapPack = "";
	for( %i = 0; %i <= $SkiFreeTerrainListMAX; %i++ ) {
		%file = $SkiFreeTerrainList[%i];
		if( !isFile("terrains/" @ %file) ) {
			$SkiFreeMissingMapPack = validationSkiFreeFail(%file);
		}
	}
	for( %i = 0; %i <= $SkiFreeTerrainListSuperHardMAX; %i++ ) {
		%file = $SkiFreeTerrainListSuperHard[%i];
		if( !isFile("terrains/" @ %file) ) {
			$SkiFreeMissingMapPack = validationSkiFreeFail(%file);
		}
	}
	for( %i = 0; %i <= $SkiFreeTerrainListRejectedMAX; %i++ ) {
		%file = $SkiFreeTerrainListRejected[%i];
		if( !isFile("terrains/" @ %file) ) {
			$SkiFreeMissingMapPack = validationSkiFreeFail(%file);
		}
	}
	deleteVariables("$SkiFreeTerrainList*");
}

function validationSkiFreeFail(%terrain) {
	%pack = $SkiFreeMeta[%terrain,MapPack];
	%index = getFieldCount($SkiFreeMissingMapPack);
	for( %i = 0; %i < %index; %i++ ) {
		if( %pack $= getField($SkiFreeMissingMapPack,%i) ) {
			return;
		}
	}
	$SkiFreeMissingMapPack = setField($SkiFreeMissingMapPack, %index, %pack);
}

function getVersionNumberForSkiFree() {
	//$SkiFreeVersionString = "1.06";
	if( $SkiFreeVersionString $= "" ) {
		%file = new FileObject();
		if( %file.openForRead( "scripts/SkiFreeGame.cs" ) ) {
			while ( !%file.isEOF() ) {
				%line = %file.readLine();
				
				if(
					strpos( %line, "$SkiFreeVersionString" ) $= 0 ||
					strpos( %line, "$SkiFreeBuildDate" ) $= 0
				) {
					eval(%line); // super fucking dangerous but who even cares
				}
			}
		}
		%file.delete();
	}
}

function tryStartSkiFreeSinglePlayer() {
	%id = SkiFreeGui_MissionList.getSelectedId();

	if( %id == -1 ) {
		MessageBoxOK("FAIL", "You done goofed.");
		return;
	}
	else if( %id > 99999 && !isFile("terrains/" @ SkiFreeGui_MissionList.getValue()) ) {
		MessageBoxOK("FIX UR SHIT", 
			"You are missing map pack:\n" @
			$SkiFreeMeta[SkiFreeGui_MissionList.getValue(), MapPack]
		);
		return;
	}
	else {
		// hard validations have been passed
		if( !hasClassicModForSkiFree() ) {
			MessageBoxYesNo(
				"SKIFREE CHECK", 
				"SkiFree is meant for Classic mod. Are you sure you want to play the game using this mod instead?",
				"startSkiFreeSinglePlayer();",
				"echo(\"then fuck off\");"
			);
		}
		else {
			startSkiFreeSinglePlayer();
		}
	}
}

// special lodear for skifree maps
function startSkiFreeSinglePlayer() {
	$SkiFreeCurrentBlueprintMap = "";

	%file = "SkiFreeSP_ProvingGrounds";
	%id = SkiFreeGui_MissionList.getSelectedId();
	$SkiFreeProvingGrounds = %id;
	$SkiFreeProvingGroundsTerrain = SkiFreeGui_MissionList.getValue();

	if( %id == 7 ) {
		// override to the correct map
		%file = "SkiFreeSP_Spring2021";
	}
	
	MessagePopup( "STARTING SKIFREE", "Initializing, please wait..." );
	Canvas.repaint();
	cancelServerQuery();

	$ServerName = "SkiFree - Proving Grounds";
			
	// setting $HostGameType to single player here keeps it off of the master server listing - it flips to SkiFree in the middle of loading
	$HostGameType = "SinglePlayer";
	CreateServer( %file, "SkiFree" );
		
	%playerPref = $pref::Player[$pref::Player::Current];
	%playerName = getField( %playerPref, 0 );
	%playerRaceGender = getField( %playerPref, 1 );
	%playerSkin = getField( %playerPref, 2 );
	%playerVoice = getField( %playerPref, 3 );
	%playerVoicePitch = getField( %playerPref, 4 );
	localConnect( %playerName, %playerRaceGender, %playerSkin, %playerVoice, %playerVoicePitch );
}

function removeSkiFreeFavorite(%id) {
	$SkiFreeBlueprint[%id,Favorite] = "";
	generateSkiFreeText();
	saveSkiFreePrefs();

	SkiFreeGui_MissionList.setRowById( SkiFreeGui_MissionList.getSelectedId(), strreplace(SkiFreeGui_MissionList.getValue(), "\c4", "") );
}

function hasClassicModForSkiFree() {
	return $Classic::gravSetting !$= "";
}

function generateSkiFreeText() {
	%id = SkiFreeGui_MissionList.getSelectedId();
	
	// why the fuck didn't i just make text files for this shit lol
	if( %id == -1 ) {
		%text = "EAT AT JOE'S";
	}
	else if( %id < 90000 ) {
		// a previous game
		%terrain = getField($SkiFreeBlueprint[%id,Map], 0) @ ".ter";
		%mapPack = $SkiFreeMeta[%terrain,MapPack];
		if( %mapPack $= "" ) %mapPack = "UNKNOWN";
		
		%fields = $SkiFreeBlueprint[%id,Fields];
		if( %fields !$= "" ) {
			%splits = "\n";
			%fieldCount = getFieldCount(%fields);
			%gate = 1;
			for( %i = 0; %i < %fieldCount; %i += 2 ) {
				%time = getField(%fields, %i);
				if( %i + 2 == %fieldCount ) %bestTime = %time;
				%splits = %splits
					@ "<spush><color:C0C0C0>Gate " @ %gate @ "<spop>: " @ %time @ " (" @ getField(%fields, %i + 1) @ "kph)\n";
				%gate++;
			}
		}
		else {
			%bestTime = "<spush><color:FF8080>DNF<spop>";
		}

		if( !$SkiFreeBlueprint[%id,Favorite] ) {
			%favorite = "<a:SkiFreeAddToFavorites\t" @ %id @ ">Add to Favorites</a>";
			
			if( %fields $= "" ) {
				%situation = "<spush><color:FF8080>Any map without a time<spop> will be deleted after <spush><color:FF8080>7 days<spop>. If you want to keep it after that point, add it to your Favorites.";
			}
			else {
				%situation = "<spush><color:FF8080>Any map with a time<spop> will be deleted after <spush><color:FF8080>31 days<spop>. If you want to keep it, add it to your Favorites.";
			}
		}
		else {
			%favorite = "This map is in your favorites.";
			
			%diff = getEpochOffset("-" @ $SkiFreeBlueprint[%id,Epoch]);
			if( (%fields $= "" && %diff >= 60 * 60 * 24 * 7) ||
				(%fields !$= "" && %diff >= 60 * 60 * 24 * 31)
			) {
				%favorite = %favorite @ " <spush><color:FF8080>Removing this map from your favorites will delete it!<spop>";
			}
			
			%favorite = %favorite @ "\n<a:SkiFreeRemoveFromFavorites\t" @ %id @ ">Remove from Favorites</a>";
			
			%situation = "Maps in your favorites will be kept indefinitely.";
		}
		
		%text =
			"Terrain: " @ %terrain @ "\n"
			@ "Map Pack: " @ %mapPack @ "\n"
			@ "Date of Game: " @ $SkiFreeBlueprint[%id,Date] @ "\n\n"
			
			@ "Completions: " @ $SkiFreeBlueprint[%id,Completions] @ " (" @ $SkiFreeBlueprint[%id,Attempts] @ ")\n"
			@ "Best Time: " @ %bestTime @ "\n"

			@ %splits @ "\n"
			
			@ %favorite @ "\n\n"
			
			@ %situation
		;
	}
	else if( %id > 99999 ) {
		// this is a normal terrain
		%terrain = SkiFreeGui_MissionList.getValue();
		
		%mapPack = $SkiFreeMeta[%terrain,MapPack];
		if( %mapPack $= "" ) %mapPack = "UNKNOWN";

		if( %mapPack $= "UNKNOWN" ) {
			%status = "\nThis terrain does not belong to a map pack used by SkiFree. It might be X, X2, Cluster, one of those Euro packs, or even a map pack that doesn't exist yet. Whatever the reason, it's not a regulation terrain that can ever show up on a server.\n\nAlso the Dynamix Final Map Pack doesn't exist in the Tribes 2 All-in-One install and it's not like any of those terrains will be missed in SkiFree.";
		}
		else if( $SkiFreeMeta[%terrain,RejectReason] $= "" ) {
			%status = "Regulation SkiFree Terrain\n\nThis terrain can show up on servers running SkiFree.";
		}
		else if( $SkiFreeMeta[%terrain,RejectReason] $= "SUPERHARD" ) {
			%status = "Superhard SkiFree Terrain\n\nThis terrain was rejected from regulation for some reason or another, but the developer thought it would be funny to specifically call it out as a joke.";
		}
		else if( $SkiFreeMeta[%terrain,RejectReason] $= "DEADSTOPS" ) {
			%status = "Reject Reason: DEADSTOPS\n\nThis terrain was rejected from regulation due to deadstops. Any terrain with wide patches of flat terrain are generally rejected for this reason. Remember, the ENTIRE terrain must be skiiable to be acceptable in SkiFree, including any OOB areas.";
			
			if( %terrain $= "Sundried.ter" ) {
				%status = %status @ "\n\nIn addition, SunDried.ter has the special distinction of being used as an exception handler whenever a terrain fails to load, so that you don't UE if you open the Command Circuit.";
			}
		}
		else if( $SkiFreeMeta[%terrain,RejectReason] $= "UNSKIIABLE" ) {
			%status = "Reject Reason: UNSKIIABLE\n\nThis terrain was rejected from regulation due to being unskiiable. Generally these terrains have way too many gigantic hills. Remember, the ENTIRE terrain must be skiiable to be acceptable in SkiFree, including any OOB areas.";
		}
		else if( $SkiFreeMeta[%terrain,RejectReason] $= "DUPLICATE" ) {
			%status = "Reject Reason: DUPLICATE\n\nThis is a duplicate terrain. The latest version of this terrain is probably a Regulation SkiFree Terrain, unless I made a mistake.";
		}
		else if( $SkiFreeMeta[%terrain,RejectReason] $= "OTHER" ) {
			%status = "Reject Reason: OTHER\n\nThis terrain was rejected for an unspecified reason. The textures on the terrain may be difficult to read due to bad texturing (Training5), or the terrain might be really boring to play on (TL_Magnum), or the developer of SkiFree might just have it out for this terrain (Lakefront). Whatever the reason, this terrain has been rejected.";
		}
		else {
			%status = "Metadata Error\n\n<spush><color:FF8080>There's something wrong with the Metadata for this terrain. RejectReason = " @ $SkiFreeMeta[%terrain,RejectReason] @ "<spop>";
		}
		
		%text =
			"Terrain: " @ %terrain @ "\n"
			@ "Map Pack: " @ %mapPack @ "\n"
			@ %status
		;
	}
	else if( %id == 90001 || %id == 90002 || %id == 90003 ) { // the daily
		%text = "The daily challenge of SkiFree.\n\n"
		
			@ "A new level will be generated every day. A completely random terrain that nobody has ever seen before, until now.\n\n"
			
			@ "Since the level will be the same regardless of who is playing, you can share times! Talk about how much better you are than everyone else! Forget about the time zone conversions and end up talking about the wrong map! Get banned from Discord for spamming this crap months after everyone else stops caring about it!\n\n"
			
			@ "Difficulty will determine how hard the terrain is. So it can really be thought of as a three-in-one challenge. Easy and Medium will just be varying levels of fBM Fractal, which makes some nice rolling hills. But the Hard terrain? You'll have to see it to believe it."
		;
	}
	else if( %id == 90004 || %id == 90005 || %id == 90006 ) { // randomizer
		%text = "The randomizer of SkiFree.\n\n"
		
			@ "This will generate completely random terrains that nobody has ever seen, and nobody will ever see again.\n\n"
			
			@ "Difficulty will determine how hard the terrain is. So it can really be thought of as a three-in-one challenge. Easy and Medium will just be varying levels of fBM Fractal, which makes some nice rolling hills. But the Hard terrain? You'll have to see it to believe it."
		;
	}
	else if( %id == 90007 ) { // spring 2021 tourney
		%text = "Spring 2021 Tourney\n\n"

			@ "The Spring 2021 Tourney was a Medium-difficulty daily generated on 2021-03-30 with Version 1.04.\n\n"
		
			@ "This map was created for the 20th anniversary of Tribes 2, and released on 2021-03-30. That day would have been the 20th anniversary of the Tribes 2 account of Red Shifter (SkiFree Lead Developer), if the Dynamix account system still existed.\n\n"
			
			@ "It was played as an offline tournament that ran from 2021-03-30 to 2021-04-05. This didn't end up being a very good tournament format, and a lot of people didn't really get into it until it was already over.\n\n"
			
			@ "Qualifying Time: 73.868\n"
			@ "Champion Time: 56.705\n\n"
			
			@ "Best Known Time (late submission): 54.753\n\n"
			@ "Hints:\n"
			@ "- Discjump twice before Gate 1 and try to hit 450kph\n"
			@ "- Ride that speed through Gate 5 if you can\n"
			@ "- Use the terrain to turn yourself towards Gate 6\n"
			@ "- Carrying high speed through the last two gates is sketchy"
		;
	}
	else {
		%text = "EAT AT JOE'S";
	}
	
	SkiFreeGui_BriefingText.setText(%text);
}

function helpForSkiFree() {
	%text = "SkiFree v" @ $SkiFreeVersionString @ " (" @ $SkiFreeBuildDate @ ")\n\n"

		@ "In SkiFree, a set of gates are randomly generated on a terrain. Try to ski through each one as fast as you can!\n\n"

		@ "This gametype can be played on a server, or offline. Online play is done from the HOST menu, like normal gametypes.\n"
		@ "<spush><color:C0C0C0>This tab is for offline play.<spop>\n\n"
		
		@ "First, you will want to select a mode. In general, the selections will have their own message text.\n\n"
		@ "<spush><color:C0C0C0>Previous Games<spop>: Allows you to recall a map you've played online, along with your best time and number of attempts.\n"
		@ "<spush><color:C0C0C0>Choose Terrain<spop>: Choose a terrain to play on, and it will generate as if on a normal server. You can choose from multiple sets of terrains.\n"
		@ "<spush><color:C0C0C0>Randomizers<spop>: Play a randomly generated terrain that's never been seen before and will never be seen again.\n"
		@ "<spush><color:C0C0C0>Special Maps<spop>: Play maps that were used for events.\n\n"
		
		@ "<spush><color:C0C0C0>Simulated Ping<spop> allows you to set your own ping for offline play. You'll probably need to do some fine-tuning to get it to work correctly, as the ping you have in-game will generally be slightly higher than the ping you select. <spush><color:C0C0C0>Note that this setting is global for ALL offline play, including TRAINING and anything from the HOST menu.<spop>\n\n"
	;
	
	if( $SkiFreeMissingMapPack !$= "" ) {
		%text = %text
			@ "<spush><color:FF8080>WARNING: <color:FFFF00>You are missing map packs expected by SkiFree. Please install them:\n";
		for( %i = 0; %i < getFieldCount($SkiFreeMissingMapPack); %i++ ) {
			%text = %text @ getField($SkiFreeMissingMapPack, %i) @ "\n";
		}
		%text = %text @ "<spop>\n";
	}

	if( !hasClassicModForSkiFree() ) {
		%text = %text
			@ "<spush><color:FF8080>WARNING: <color:FFFF00>You are not playing Classic mod. SkiFree was intended to be played under Classic mod.<spop>\n\n";
	}
	
	%text = %text @ "<spush><color:FFFFFF>CREDITS<spop>\n"
		@ "- Red Shifter: Lead Developer\n"
		@ "- DarkTiger: Gave the lead developer the code for phasing through players\n"
		@ "- Rooster128, Stormcrow IV, LOLCAPS, The D_e_V_i_L, and many others: Playtesting\n\n\n\n"
		
		@ "<spush><font:arial:12>SkiFree was created for the 20th Anniversary of Tribes 2 in 2021, and is dedicated to the memory of Zengato, who was always there to read over any shitty gametype idea I had, no matter how stupid it was.<spop>"
	;
	SkiFreeGui_BriefingText.setText(%text);
}

function SkiFreeGui::setKey() {
	// dunno what this even does but it spamming me
}