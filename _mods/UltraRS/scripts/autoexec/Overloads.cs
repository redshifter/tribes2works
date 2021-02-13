package UltraRS_Overloads {

// Overload Set 1: Equipment for every player is the same six weapons each time
function DefaultGame::equip(%game, %player)
{
   %player.setRepairRate(0.0008);
   buyFavorites(%player.client);
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
   %player.setInventory(Beacon, 3);
   %player.setInventory(TargetingLaser, 1);
   %player.weaponCount = 6;
   
   %player.use("Blaster");

   // If a player picked up a pack somewhere, don't strip it from him
   if(%prevPack > 0)
      %player.setInventory( %prevPack.item, 1 );
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

      bottomPrint(%client, $LaserColor[%client.laserColor] SPC "Laser", 1.5, 1);
      %this.use( TargetingLaser );
   }
   else {
      Parent::cycleWeapon( %this, %data );
   }
}

// Overload Set 5: Classic Mod flag hacks
function ShapeBase::throwObject(%this,%obj)
{
   //------------------------------------------------------------------
   // z0dd - ZOD, 4/15/02. Allow respawn switching during tourney wait.
   if(!$MatchStarted)
      return;
   //------------------------------------------------------------------
   
   // z0dd - ZOD, 5/26/02. Remove anti-hover so flag can be thrown properly
   if(%obj.getDataBlock().getName() $= "Flag")
   {
      %obj.static = false;
      // z0dd - ZOD - SquirrelOfDeath, 10/02/02. Hack for flag collision bug.
      if(Game.Class $= CTFGame || Game.Class $= PracticeCTFGame)
         %obj.searchSchedule = Game.schedule(10, "startFlagCollisionSearch", %obj);
   }
   //------------------------------------------------------------------

   %srcCorpse = (%this.getState() $= "Dead"); // z0dd - ZOD, 4/14/02. Flag tossed from corpse
   //if the object is being thrown by a corpse, use a random vector
   if (%srcCorpse && %obj.getDataBlock().getName() !$= "Flag") // z0dd - ZOD, 4/14/02. Except for flags..
   {
      %vec = (-1.0 + getRandom() * 2.0) SPC (-1.0 + getRandom() * 2.0) SPC getRandom();
      %vec = vectorScale(%vec, 10);
   }
   else // else Initial vel based on the dir the player is looking
   {
      %eye = %this.getEyeVector();
      %vec = vectorScale(%eye, 20);
   }

   // Add a vertical component to give the item a better arc
   %dot = vectorDot("0 0 1",%eye);
   if (%dot < 0)
      %dot = -%dot;
   %vec = vectorAdd(%vec,vectorScale("0 0 12",1 - %dot)); // z0dd - ZOD, 9/10/02. 10 was 8

   // Add player's velocity
   %vec = vectorAdd(%vec,%this.getVelocity());
   %pos = getBoxCenter(%this.getWorldBox());

   //since flags have a huge mass (so when you shoot them, they don't bounce too far)
   //we need to up the %vec so that you can still throw them...
   if (%obj.getDataBlock().getName() $= "Flag")
   {
      %vec = vectorScale(%vec, (%srcCorpse ? 40 : 75)); // z0dd - ZOD, 4/14/02. Throw flag force. Value was 40
      // ------------------------------------------------------------
      // z0dd - ZOD, 9/27/02. Delay on grabbing flag after tossing it
      %this.flagTossWait = true;
      %this.schedule(1000, resetFlagTossWait);
      // ------------------------------------------------------------
   }

   //
   %obj.setTransform(%pos);
   %obj.applyImpulse(%pos,%vec);
   %obj.setCollisionTimeout(%this);
   %data = %obj.getDatablock();

   %data.onThrow(%obj,%this);

   //call the AI hook
   AIThrowObject(%obj);
}
function Player::resetFlagTossWait(%this)
{
   %this.flagTossWait = false;
}
function CTFGame::playerTouchEnemyFlag(%game, %player, %flag)
{
   // ---------------------------------------------------------------
   // z0dd, ZOD - 9/27/02. Player must wait to grab after throwing it
   if((%player.flagTossWait !$= "") && %player.flagTossWait)
      return false;
      
   return Parent::playerTouchEnemyFlag(%game, %player, %flag);
}


// Overload Set 6: Remove the suicide delay
function DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation) {
   // call the parent
   Parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   // schedule a good respawn
   %game.schedule(2000, "clearWaitRespawn_UltraRS", %clVictim);
}
function DefaultGame::clearWaitRespawn_UltraRS(%game, %client) {
   %client.waitRespawn = 0;
}
function DefaultGame::clearWaitRespawn(%game, %client) {
   // remove this function to make things easier
}


};

activatePackage(UltraRS_Overloads);