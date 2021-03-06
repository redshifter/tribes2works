// terrain list (see timestamp)
// this needs fine-tuning, though a lot has already

// a good terrain has the following qualities:
// - doesn't have a bunch of flat ground, even if it's right outside the mission bounds (high octane)
// - is not fucking gigantic (stripmine, a bunch of other TR2 terrains)
// - doesn't have a bunch of steep plateaus

// use $TerrainTest to type in a terrain to use

%i = -1; // %i++ is a pre-increment for some reason
%loadDynamixMaps = false; // probably not much good in here
%loadClassicMaps = true;
%loadTR2Maps     = true;  // balancing done. 3/8 terrains accepted
%loadTWLMaps     = true;
%loadTWL2Maps    = true;
%loadS5Maps      = true;  // balancing done. 8/11 terrains accepted
%loadS8Maps      = true;  // balancing done. 3/6 terrains accepted
%loadDMPMaps     = true;

// dynamix maps
if( %loadDynamixMaps ) {
	$SkiFreeTerrainList[%i++] = "Abominable.ter";
	$SkiFreeTerrainList[%i++] = "AgentsOfFortune.ter";
	$SkiFreeTerrainList[%i++] = "Alcatraz.ter";
	$SkiFreeTerrainList[%i++] = "Archipelago.ter";
	$SkiFreeTerrainList[%i++] = "AshesToAshes.ter";
	$SkiFreeTerrainList[%i++] = "BeggarsRun.ter";
	$SkiFreeTerrainList[%i++] = "Caldera.ter";
	$SkiFreeTerrainList[%i++] = "Casern_Cavite.ter";
	$SkiFreeTerrainList[%i++] = "CompUSA_Melee.ter";
	$SkiFreeTerrainList[%i++] = "Damnation.ter";
	$SkiFreeTerrainList[%i++] = "DeathBirdsFly.ter";
	$SkiFreeTerrainList[%i++] = "Desiccator.ter";
	$SkiFreeTerrainList[%i++] = "DustToDust.ter";
	$SkiFreeTerrainList[%i++] = "EB_Hades.ter";
	$SkiFreeTerrainList[%i++] = "Equinox.ter";
	$SkiFreeTerrainList[%i++] = "Escalade.ter";
	$SkiFreeTerrainList[%i++] = "Extra_Badlands1.ter";
	$SkiFreeTerrainList[%i++] = "Firestorm.ter";
	$SkiFreeTerrainList[%i++] = "Flashpoint.ter";
	$SkiFreeTerrainList[%i++] = "Fracas.ter";
	$SkiFreeTerrainList[%i++] = "Gauntlet.ter";
	$SkiFreeTerrainList[%i++] = "Gehenna.ter";
	$SkiFreeTerrainList[%i++] = "IceBound.ter";
	$SkiFreeTerrainList[%i++] = "Insalubria.ter";
	$SkiFreeTerrainList[%i++] = "Invictus.ter";
	$SkiFreeTerrainList[%i++] = "JacobsLadder.ter";
	//$SkiFreeTerrainList[%i++] = "Katabatic.ter"; // this map is really shitty to ski on
	$SkiFreeTerrainList[%i++] = "Masada.ter";
	$SkiFreeTerrainList[%i++] = "Minotaur.ter";
	$SkiFreeTerrainList[%i++] = "MyrkWood.ter";
	$SkiFreeTerrainList[%i++] = "Oasis.ter";
	$SkiFreeTerrainList[%i++] = "Overreach.ter";
	$SkiFreeTerrainList[%i++] = "Pyroclasm.ter";
	$SkiFreeTerrainList[%i++] = "Quagmire.ter";
	$SkiFreeTerrainList[%i++] = "Rasp.ter";
	$SkiFreeTerrainList[%i++] = "Recalescence.ter";
	$SkiFreeTerrainList[%i++] = "Respite.ter";
	$SkiFreeTerrainList[%i++] = "Reversion.ter";
	$SkiFreeTerrainList[%i++] = "Rimehold.ter";
	$SkiFreeTerrainList[%i++] = "RiverDance.ter";
	$SkiFreeTerrainList[%i++] = "Sanctuary.ter";
	$SkiFreeTerrainList[%i++] = "Sirocco.ter";
	$SkiFreeTerrainList[%i++] = "Slapdash.ter";
	//$SkiFreeTerrainList[%i++] = "SunDried.ter"; // this terrain is being used to indicate a fail condition in loading
	$SkiFreeTerrainList[%i++] = "Talus.ter";
	//$SkiFreeTerrainList[%i++] = "ThinIce.ter"; // lol
	$SkiFreeTerrainList[%i++] = "Tombstone.ter";
	$SkiFreeTerrainList[%i++] = "Training1.ter";
	$SkiFreeTerrainList[%i++] = "Training2.ter";
	$SkiFreeTerrainList[%i++] = "Training3.ter";
	$SkiFreeTerrainList[%i++] = "Training4.ter";
	$SkiFreeTerrainList[%i++] = "Training5.ter";
	$SkiFreeTerrainList[%i++] = "UltimaThule.ter";
	$SkiFreeTerrainList[%i++] = "Underhill.ter";
	$SkiFreeTerrainList[%i++] = "Whiteout.ter";
}

// classic maps
if( %loadClassicMaps ) {
	$SkiFreeTerrainList[%i++] = "AcidRain.ter";
	$SkiFreeTerrainList[%i++] = "Broadside_nef.ter";
	//$SkiFreeTerrainList[%i++] = "Confusco.ter"; // so big it breaks the parser, and also has obvious deadstops
	$SkiFreeTerrainList[%i++] = "DangerousCrossing_nef.ter";
	$SkiFreeTerrainList[%i++] = "DesertofDeath_nef.ter";
	$SkiFreeTerrainList[%i++] = "Gorgon.ter";
	//$SkiFreeTerrainList[%i++] = "Hillside.ter"; // so big it breaks the parser
	$SkiFreeTerrainList[%i++] = "IceRidge_nef.ter";
	$SkiFreeTerrainList[%i++] = "Lakefront.ter";
	$SkiFreeTerrainList[%i++] = "Magmatic.ter";
	$SkiFreeTerrainList[%i++] = "Raindance_nef.ter";
	$SkiFreeTerrainList[%i++] = "Ramparts.ter";
	$SkiFreeTerrainList[%i++] = "Rollercoaster_nef.ter";
	$SkiFreeTerrainList[%i++] = "Sandstorm.ter";
	//$SkiFreeTerrainList[%i++] = "Scarabrae_nef.ter"; // hills are a bit too extreme
	$SkiFreeTerrainList[%i++] = "ShockRidge.ter";
	$SkiFreeTerrainList[%i++] = "Snowblind_nef.ter";
	$SkiFreeTerrainList[%i++] = "Starfallen.ter";
	$SkiFreeTerrainList[%i++] = "Stonehenge_nef.ter";
	$SkiFreeTerrainList[%i++] = "SubZero.ter";
	$SkiFreeTerrainList[%i++] = "Surreal.ter";
	$SkiFreeTerrainList[%i++] = "Titan.ter";
	$SkiFreeTerrainList[%i++] = "WhiteDwarf.ter";
}

// tr2 maps
if( %loadTR2Maps ) {
	$SkiFreeTerrainList[%i++] = "Crater71.ter"; // it's big, but it's a skiiable big
	//$SkiFreeTerrainList[%i++] = "FrozenFury.ter"; // this map is WAY too difficult when you get outside the tr2 playable area
	//$SkiFreeTerrainList[%i++] = "GodsRift.ter"; // this map can very easily generate a path that's very unskiiable
	$SkiFreeTerrainList[%i++] = "Haven.ter"; // seems fine
	//$SkiFreeTerrainList[%i++] = "PhasmaDust.ter"; // high octane has way too much flat area at the edges
	$SkiFreeTerrainList[%i++] = "SkinnyDip.ter"; // perfect
	//$SkiFreeTerrainList[%i++] = "SolsDescent.ter"; // no, absolutely not
	//$SkiFreeTerrainList[%i++] = "TreasureIsland.ter"; // the flat borders that seem common with tr2 maps really limit their use in this gametype
}

// twl pack maps
if( %loadTWLMaps ) {
	$SkiFreeTerrainList[%i++] = "TWL-Abaddon.ter";
	$SkiFreeTerrainList[%i++] = "TWL-BaNsHee.ter";
	$SkiFreeTerrainList[%i++] = "TWL-BeachBlitz.ter";
	$SkiFreeTerrainList[%i++] = "TWL-BeggarsRun.ter";
	//$SkiFreeTerrainList[%i++] = "TWL-BlueMoon.ter"; // part of TWL2, doesn't need to be in both sets
	$SkiFreeTerrainList[%i++] = "TWL-Boss.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Chokepoint.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Cinereous.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Clusterfuct.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Curtilage.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Damnation.ter";
	$SkiFreeTerrainList[%i++] = "TWL-DeadlyBirdsSong.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Deserted.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Desiccator.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Drifts.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Euro_Feign.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Frostclaw.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Frozen.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Harvester.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Horde.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Katabatic.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Neve.ter";
	$SkiFreeTerrainList[%i++] = "TWL-NoShelter.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Os_Iris.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Pandemonium.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Runenmacht.ter";
	$SkiFreeTerrainList[%i++] = "TWL-Slapdash.ter";
	$SkiFreeTerrainList[%i++] = "TWL-SubZero.ter";
	$SkiFreeTerrainList[%i++] = "TWL-WilderZone.ter";
	$SkiFreeTerrainList[%i++] = "TWL-WoodyMyrk.ter";
	$SkiFreeTerrainList[%i++] = "TWL_Crossfire.ter";
}

// twl2 pack maps (no idea why they didn't name everything twl2)
if( %loadTWL2Maps ) {
	$SkiFreeTerrainList[%i++] = "CCD.ter";
	$SkiFreeTerrainList[%i++] = "CeleritySE.ter";
	$SkiFreeTerrainList[%i++] = "Euro4_Bleed.ter";
	$SkiFreeTerrainList[%i++] = "Euro4_Dissention.ter";
	$SkiFreeTerrainList[%i++] = "Euro4_FrozenHope.ter";
	$SkiFreeTerrainList[%i++] = "Euro_Drifts_SE.ter";
	$SkiFreeTerrainList[%i++] = "Hildebrand.ter";
	$SkiFreeTerrainList[%i++] = "Ocular.ter";
	$SkiFreeTerrainList[%i++] = "TL_Drorck.ter";
	$SkiFreeTerrainList[%i++] = "TL_Magnum.ter";
	$SkiFreeTerrainList[%i++] = "TL_MuddySwamp.ter";
	$SkiFreeTerrainList[%i++] = "TL_RoughLand.ter";
	//$SkiFreeTerrainList[%i++] = "TL_Skylight.ter"; // the edges of the maps are so big it breaks the parser. no way you're crossing that
	$SkiFreeTerrainList[%i++] = "TWL-BlueMoon.ter";
	$SkiFreeTerrainList[%i++] = "TWL2_Crevice.ter";
	$SkiFreeTerrainList[%i++] = "TWL2_Frozenglory.ter";
	$SkiFreeTerrainList[%i++] = "TWL2_Ruined.ter";
	//$SkiFreeTerrainList[%i++] = "WoodyMyrkSE.ter"; // part of S5, doesn't need to be in both sets
	$SkiFreeTerrainList[%i++] = "cloak.ter";
	$SkiFreeTerrainList[%i++] = "icedagger.ter";
	$SkiFreeTerrainList[%i++] = "jaggedclaw.ter";
	$SkiFreeTerrainList[%i++] = "mmd.ter";
	$SkiFreeTerrainList[%i++] = "norty.ter";
}

// s5 maps
if( %loadS5Maps ) {
	//$SkiFreeTerrainList[%i++] = "S5-Icedance.ter";  // why are there two icedances
	//$SkiFreeTerrainList[%i++] = "S5-Mordacity.ter"; // you can use dashes in terrain names
	//$SkiFreeTerrainList[%i++] = "S5-massive.ter";   // it's just the mis name that can't have it
	$SkiFreeTerrainList[%i++] = "S5_Centaur.ter"; // yeah sure why not
	$SkiFreeTerrainList[%i++] = "S5_Drache.ter"; // this is fine
	$SkiFreeTerrainList[%i++] = "S5_Icedance.ter"; // this is also fine
	$SkiFreeTerrainList[%i++] = "S5_Mordacity.ter"; // fine
	$SkiFreeTerrainList[%i++] = "S5_Sherman.ter"; // might actually be too easy
	//$SkiFreeTerrainList[%i++] = "S5_massive.ter"; // has a sneaky deadstop - probably not worth keeping when there's a lot of good maps without them
	//$SkiFreeTerrainList[%i++] = "S5_rst_hawkingheat.ter"; // this level is too extreme in elevation and isn't really skiiable
	$SkiFreeTerrainList[%i++] = "S5_rst_misadventure.ter"; // this level is fine
	$SkiFreeTerrainList[%i++] = "S5_rst_reynard.ter"; // this is pushing it a little but it's okay
	//$SkiFreeTerrainList[%i++] = "S5_rst_silenus.ter"; // this is pushing it too far, and rejecting it means there's only 8 accepted terrains
	$SkiFreeTerrainList[%i++] = "WoodyMyrkSE.ter"; // gave me my first 700+ run (5459.3m) so it's safe to say it's in
}

// s8 maps
if( %loadS8Maps ) {
	$SkiFreeTerrainList[%i++] = "Cardiac.ter"; // this is an ideal map for this gametype
	//$SkiFreeTerrainList[%i++] = "Geothermal.ter"; // the plateau where the spawn usually appears kinda sucks
	$SkiFreeTerrainList[%i++] = "S8_rst_dogma.ter"; // this is fine - code already prevents putting a spawn point in the canyon
	$SkiFreeTerrainList[%i++] = "S8_rst_opus.ter"; // good map i guess
	//$SkiFreeTerrainList[%i++] = "S8_zilch.ter"; // this level has some very extreme mountains next to each other, and is generally too difficult
	//$SkiFreeTerrainList[%i++] = "mountking.ter"; // this is not a skiiable map what the fuck is this shit
}

// dmp maps
if( %loadDMPMaps ) {
	$SkiFreeTerrainList[%i++] = "Attrition.ter";
	$SkiFreeTerrainList[%i++] = "BastardForge.ter";
	$SkiFreeTerrainList[%i++] = "Bunkered.ter";
	$SkiFreeTerrainList[%i++] = "Chasmaclysmic.ter";
	$SkiFreeTerrainList[%i++] = "Cinerarium.ter";
	$SkiFreeTerrainList[%i++] = "Coppera.ter";
	$SkiFreeTerrainList[%i++] = "DBS_Smoothed.ter";
	//$SkiFreeTerrainList[%i++] = "DMP_Pantheon.ter"; // has deadstops, and even if it didn't, there isn't much here anyway
	$SkiFreeTerrainList[%i++] = "DX_Badlands.ter";    // probably fine
	$SkiFreeTerrainList[%i++] = "DX_Desert.ter";      // also probably fine
	$SkiFreeTerrainList[%i++] = "DX_Ice.ter";         // what ever
	$SkiFreeTerrainList[%i++] = "Embers.ter";
	//$SkiFreeTerrainList[%i++] = "HO_Badlands.ter";  // deadstop city
	//$SkiFreeTerrainList[%i++] = "HO_Desert.ter";    // also deadstop city
	//$SkiFreeTerrainList[%i++] = "HO_Ice.ter";       // did we really need
	//$SkiFreeTerrainList[%i++] = "HO_Lush.ter";      // this many high octanes
	$SkiFreeTerrainList[%i++] = "HillKing.ter";
	$SkiFreeTerrainList[%i++] = "Hoth.ter";
	$SkiFreeTerrainList[%i++] = "IceGiant.ter";
	$SkiFreeTerrainList[%i++] = "LavaGods.ter";
	$SkiFreeTerrainList[%i++] = "Magellan.ter";
	$SkiFreeTerrainList[%i++] = "MapAssets.ter";
	$SkiFreeTerrainList[%i++] = "MoonDance2.ter";
	$SkiFreeTerrainList[%i++] = "Moonwalk.ter";
	//$SkiFreeTerrainList[%i++] = "Octane.ter";       // HOW MANY FUCKING VERSIONS OF THIS TERRAIN ARE THERE
	$SkiFreeTerrainList[%i++] = "Paranoia.ter";
	$SkiFreeTerrainList[%i++] = "Pariah.ter";
	$SkiFreeTerrainList[%i++] = "Pariah2.ter";
	$SkiFreeTerrainList[%i++] = "PlanetX2.ter";
	$SkiFreeTerrainList[%i++] = "PuliVeivari.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer1.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer10.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer2.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer3.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer4.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer5.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer6.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer7.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer8.ter";
	$SkiFreeTerrainList[%i++] = "RandomTer9.ter";
	$SkiFreeTerrainList[%i++] = "Ravine.ter";
	$SkiFreeTerrainList[%i++] = "RavineV.ter";
	$SkiFreeTerrainList[%i++] = "Rst_ScorchedEarth.ter";
	$SkiFreeTerrainList[%i++] = "Rush.ter";
	//$SkiFreeTerrainList[%i++] = "SC_Badlands.ter";  // has
	//$SkiFreeTerrainList[%i++] = "SC_Desert.ter";    // edges
	//$SkiFreeTerrainList[%i++] = "SC_Ice.ter";       // that
	//$SkiFreeTerrainList[%i++] = "SC_Lush.ter";      // cause
	//$SkiFreeTerrainList[%i++] = "SC_Night.ter";     // deadstops
	//$SkiFreeTerrainList[%i++] = "SC_Normal.ter";    // lol
	$SkiFreeTerrainList[%i++] = "SpinCycle.ter";
	$SkiFreeTerrainList[%i++] = "StarFallCTF2.ter";
	$SkiFreeTerrainList[%i++] = "Tyre.ter";
	$SkiFreeTerrainList[%i++] = "Wasteland.ter"; // seems fine
	$SkiFreeTerrainList[%i++] = "Xtra_AshenPowder.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_Bastage.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_Birthright.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_Crown.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_DesertedSE.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_Helion.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_SoupLadle.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_StarFall_T1.ter";
	//$SkiFreeTerrainList[%i++] = "Xtra_Stripmine.ter"; // HILLS ARE TOO FUCKING BIG FOR THIS GAME
	$SkiFreeTerrainList[%i++] = "Xtra_ThunderGiant.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_VanDamned.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_Voodoo.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_Xerxes.ter";
	$SkiFreeTerrainList[%i++] = "Xtra_ziggurat.ter";
	$SkiFreeTerrainList[%i++] = "rst_Astro.ter";
	$SkiFreeTerrainList[%i++] = "rst_FaceCrossing.ter";
	$SkiFreeTerrainList[%i++] = "rst_SimpleFlagArena.ter";
	$SkiFreeTerrainList[%i++] = "rst_agroleon.ter";
	$SkiFreeTerrainList[%i++] = "rst_bittergorge.ter";
	$SkiFreeTerrainList[%i++] = "rst_crumpie.ter";
	$SkiFreeTerrainList[%i++] = "rst_dermcity.ter";
	$SkiFreeTerrainList[%i++] = "rst_isledebatalla.ter";
	$SkiFreeTerrainList[%i++] = "rst_spit.ter";
}

$SkiFreeTerrainListMAX = %i;