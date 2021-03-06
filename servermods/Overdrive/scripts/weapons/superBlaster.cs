//--------------------------------------------------------------------------
// Sublimely Magnificent Bullet Hell Blaster of Doom
//--------------------------------------
datablock ShapeBaseImageData(superBlasterImage)
{
   className = WeaponImage;
   shapeFile = "weapon_energy.dts";
   rotation = "0 1 0 -90";
   offset = "0 0 0.21";

   item = superBlaster;

   projectileSpread1 = 25.0 / 1000;
   projectileSpread2 = 111.0 / 1000;

   projectileType = EnergyProjectile;

   usesEnergy = true;
   fireEnergy = 0;
   minEnergy = 0;

   stateName[0] = "Activate";
   stateTransitionOnTimeout[0] = "ActivateReady";
   stateTimeoutValue[0] = 1.0;
   stateSequence[0] = "Activate";
   stateSound[0] = SatchelChargeActivateSound;

   stateName[1] = "ActivateReady";
   stateTransitionOnLoaded[1] = "Ready";
   stateTransitionOnNoAmmo[1] = "NoAmmo";

   stateName[2] = "Ready";
   stateTransitionOnNoAmmo[2] = "NoAmmo";
   stateTransitionOnTriggerDown[2] = "Fire";

   stateName[3] = "Fire";
   stateTransitionOnTimeout[3] = "Reload";
   stateTimeoutValue[3] = 0.25;
   stateFire[3] = true;
   stateRecoil[3] = NoRecoil;
//   stateAllowImageChange[3] = false;
   stateSequence[3] = "Fire";
   stateSound[3] = BomberTurretFireSound;
   stateScript[3] = "massiveHax";

   stateName[4] = "Reload";
   stateTransitionOnNoAmmo[4] = "NoAmmo";
   stateTransitionOnTimeout[4] = "Ready";
//   stateAllowImageChange[4] = false;
   stateSequence[4] = "Reload";

   stateName[5] = "NoAmmo";
   stateTransitionOnAmmo[5] = "Reload";
   stateSequence[5] = "NoAmmo";
   stateTransitionOnTriggerDown[5] = "DryFire";
   
   stateName[6] = "DryFire";
   stateTimeoutValue[6] = 0.1;
   stateSound[6] = BlasterDryFireSound;
   stateTransitionOnTimeout[6] = "Ready";
};

datablock ItemData(superBlaster)
{
   className = Weapon;
   catagory = "Spawn Items";
   shapeFile = "weapon_energy.dts";
   image = superBlasterImage;
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
	pickUpName = "a sublimely magnificent bullet hell blaster of doom";
};

//--------------------------
//special blaster projectile that doesn't bounce
//--------------------------
datablock EnergyProjectileData(EnergyBolt2) : EnergyBolt
{
   armingDelayMS = 250;
};
// low values aren't allowed for armingDelayMS due to engine limitation
// but they can be defined manually
EnergyBolt2.armingDelayMS = 1;

//--------------------------------------------------------------------------
//MASSIVE HAX
// each attack consists of 5+4+3+3=15 shots
// plasma, blaster, disc, belly turret
//--------------------------------------
function superBlasterImage::massiveHax(%data, %obj, %slot)
{
   %data.lightStart = getSimTime();

   if( %obj.station $= "" && %obj.isCloaked() )
   {
      if( %obj.respawnCloakThread !$= "" )
      {
         Cancel(%obj.respawnCloakThread);
         %obj.setCloaked( false );
         %obj.respawnCloakThread = "";
      }
      else
      {
         if( %obj.getEnergyLevel() > 20 )
         {   
            %obj.setCloaked( false );
            %obj.reCloak = %obj.schedule( 500, "setCloaked", true ); 
         }
      }   
   }

   if( %obj.client > 0 && %obj.invincible )
   {   
      %obj.setInvincibleMode(0 ,0.00);
      %obj.setInvincible( false ); // fire your weapon and your invincibility goes away.   
   }
   
   %energy = %obj.getEnergyLevel();
      
   if(%energy < %data.minEnergy)
      return;

// decide on what weapon gets the 5 and 4
%five = getRandom(0,3);
%four = %five;
while(%four == %five) %four = getrandom(0,3);

for( %i = 0; %i < 4; %i++ ) %randomizer[%i] = 3;
%randomizer[%five] = 5;
%randomizer[%four] = 4;

// see what spread we use
if( %obj.HyperMode == 1 )
   %spread = %data.projectileSpread2;
else
   %spread = %data.projectileSpread1;

// plasma
for( %i = 0; %i < %randomizer[0]; %i++ ) {

      %x = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;
      %y = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;
      %z = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;

   %vector = %obj.getMuzzleVector(%slot);
   %mat = MatrixCreateFromEuler(%x @ " " @ %y @ " " @ %z);
   %vector = MatrixMulVector(%mat, %vector);


   %pa[%i] = new LinearFlareProjectile() {
      dataBlock        = PlasmaBolt;
      initialDirection = %vector;
      initialPosition  = %obj.getMuzzlePoint(%slot);
      sourceObject     = %obj;
      sourceSlot       = %slot;
   };
   MissionCleanup.add(%pa[%i]);
}
// blaster
for( %i = 0; %i < %randomizer[1]; %i++ ) {

      %x = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;
      %y = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;
      %z = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;

   %vector = %obj.getMuzzleVector(%slot);
   %mat = MatrixCreateFromEuler(%x @ " " @ %y @ " " @ %z);
   %vector = MatrixMulVector(%mat, %vector);


   %pb[%i] = new EnergyProjectile() {
      dataBlock        = EnergyBolt2;
      initialDirection = %vector;
      initialPosition  = %obj.getMuzzlePoint(%slot);
      sourceObject     = %obj;
      sourceSlot       = %slot;
   };
   MissionCleanup.add(%pb[%i]);
}
// bomber turret
for( %i = 0; %i < %randomizer[2]; %i++ ) {

      %x = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;
      %y = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;
      %z = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;

   %vector = %obj.getMuzzleVector(%slot);
   %mat = MatrixCreateFromEuler(%x @ " " @ %y @ " " @ %z);
   %vector = MatrixMulVector(%mat, %vector);


   %pc[%i] = new LinearFlareProjectile() {
      dataBlock        = BomberFusionBolt;
      initialDirection = %vector;
      initialPosition  = %obj.getMuzzlePoint(%slot);
      sourceObject     = %obj;
      sourceSlot       = %slot;
   };
   MissionCleanup.add(%pc[%i]);
}
// disc
for( %i = 0; %i < %randomizer[3]; %i++ ) {

      %x = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;
      %y = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;
      %z = (getRandom() - 0.5) * 2 * 3.1415926 * %spread;

   %vector = %obj.getMuzzleVector(%slot);
   %mat = MatrixCreateFromEuler(%x @ " " @ %y @ " " @ %z);
   %vector = MatrixMulVector(%mat, %vector);


   %pd[%i] = new LinearProjectile() {
      dataBlock        = DiscProjectile;
      initialDirection = %vector;
      initialPosition  = %obj.getMuzzlePoint(%slot);
      sourceObject     = %obj;
      sourceSlot       = %slot;
   };
   MissionCleanup.add(%pd[%i]);
}


   if(%data.usesEnergy)
   {
      if(%data.useMountEnergy)
      {   
         if( %data.useCapacitor )
         {   
            %vehicle.turretObject.setCapacitorLevel( %vehicle.turretObject.getCapacitorLevel() - %data.fireEnergy );
         }
         else
            %useEnergyObj.setEnergyLevel(%energy - %data.fireEnergy);
      }
      else
         %obj.setEnergyLevel(%energy - %data.fireEnergy);
   }
   else
      %obj.decInventory(%data.ammo,1);
   return %p;
}
