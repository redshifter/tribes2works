$asswhuppinVersion = "0.0.1";

package UltraRS_Overloads {

// Overload Set 1: Equipment for every player is the same six weapons each time
function DefaultGame::equip(%game, %player)
{
   %player.setRepairRate(0.0008);
   buyFavorites(%player.client);
   %player.setInventory( EnergyPack, 0 );
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
   %player.setInventory(Plasma,1);
   %player.setInventory(Disc,1);
   %player.setInventory(GrenadeLauncher,1);
   %player.setInventory(Mortar,1);
   %player.setInventory(ShockLance,1);
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

};

activatePackage(UltraRS_Overloads);