$asswhuppinVersion = "0.0.1";

package HyperDrive_Overloads {

// Overload Set 1: override all the re-equips
function DefaultGame::equip(%game, %player)
{
   buyFavorites(%player.client);
}

function CnHGame::equip(%game, %player) {
   DefaultGame::equip(%game, %player);
}

function CnHGame::equip(%game, %player) {
   DefaultGame::equip(%game, %player);
}

function BountyGame::equip(%game, %player) {
   DefaultGame::equip(%game, %player);
}

function DMGame::equip(%game, %player) {
   DefaultGame::equip(%game, %player);
}

function HuntersGame::equip(%game, %player) {
   DefaultGame::equip(%game, %player);
}

function TeamHuntersGame::equip(%game, %player) {
   DefaultGame::equip(%game, %player);
}



// Overload Set 2: Going to an inv buys deployable favorites (no armor switch)
function buyFavorites(%client) {
   buyDeployableFavorites(%client);
}



// Overload Set 3: Inventory gives you the same default items as normal
function buyDeployableFavorites(%client) {
   %player = %client.player;
   %prevPack = %player.getMountedImage($BackpackSlot);
   %player.clearInventory();
   %client.setWeaponsHudClearAll();
   %cmt = $CurrentMissionType;

   for(%i =0; %i<$InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

   //%player.setArmor("Light");
   %player.setInventory(RepairKit,2);
   %player.setInventory(Blaster,1);
   %player.setInventory(Disc,1);
   %player.setInventory(DiscAmmo,6);
   %player.setInventory(GrenadeLauncher,1);
   %player.setInventory(GrenadeLauncherAmmo,4);
   %player.setInventory(TargetingLaser, 1);
   %player.weaponCount = 6;
   
   %player.use("Blaster");

   // If a player picked up a pack somewhere, don't strip it from him
   if(%prevPack > 0)
      %player.setInventory( %prevPack.item, 1 );
   else
      %player.setInventory( EnergyPack, 1 );
}



// Overload Set 4: Targeting Laser code
function ShapeBase::cycleWeapon( %this, %data )
{
   if( %this.getMountedImage($WeaponSlot) == 0 ) {
      Parent::cycleWeapon(%this,%data);
   }
   else if( %this.getMountedImage($WeaponSlot).isTargetLaser ) {
      %client = %this.client;
      if( %data $= "prev" )
         %client.laserColor--;
      else
         %client.laserColor++;
      
      if( %client.laserColor > $TargetingLaserColors )
         %client.laserColor = 1;
      else if( %client.laserColor == 0 )
         %client.laserColor = $TargetingLaserColors;

// save a bandwidth, kill some text
//      bottomPrint(%client, $LaserColor[%client.laserColor] SPC "Laser", 1.5, 1);

      %this.use( TargetingLaser );
   }
   else {
      Parent::cycleWeapon( %this, %data );
   }
}



// Overload Set 5: Don't allow turrets to fire
function TurretData::selectTarget(%this, %turret)
{
   %turret.clearTarget();
   return;
}

// Overload Set 6: hack to get the ai over itself
function AIEngageWeaponRating(%client) {
	if (!isObject(%client.player)) {
		return;
	}
	else {
		return 999;
	}
}
function AIBuyInventory(%client, %requiredEquipment, %equipmentSets, %buyInvTime) {
	%buySet = "LightHyperdriveDefault LightHyperdriveDefault LightHyperdriveDefault";
	Parent::AIBuyInventory(%client, %requiredEquipment, %equipmentSets, %buyInvTime);
	
}

};

activatePackage(HyperDrive_Overloads);

$EquipConfigIndex = -1;
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "Light";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "EnergyPack";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "Blaster";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "Disc";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "DiscAmmo";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "GrenadeLauncher";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "GrenadeLauncherAmmo";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "TargetingLaser";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "RepairKit";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "Grenade";
$AIEquipmentSet[HyperdriveDefault, $EquipConfigIndex++] = "Mine";
