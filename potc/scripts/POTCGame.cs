// --------------------------------------------------------
// Pirates of the Caribbean v0.5
//
// Concept by Beau
// Coding by Red Shifter
// --------------------------------------------------------

// DisplayName = POTC

//--- GAME RULES BEGIN ---
//Fly a havoc above the high seas
//Kill those scurvy dogs on the other team
//YARRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR!
//--- GAME RULES END ---

$InvBanList[POTC, "Chaingun"] = 1;
$InvBanList[POTC, "MissileLauncher"] = 1;
$InvBanList[POTC, "SensorJammerPack"] = 1;
$InvBanList[POTC, "AmmoPack"] = 1;
$InvBanList[POTC, "Mine"] = 1;

package POTCGame {

// make all base equipment invincible
function StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType) {
	if( %data.deployedObject )
		Parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType);
}

// no shortcuts; we have to take the whole damn function for one little change
function ShockLanceImage::onFire(%this, %obj, %slot)
{
   // z0dd - ZOD, 4/10/04. ilys - Added rapidfire shocklance fix
   if(%obj.cantfire !$= "")
      return;

   %obj.cantfire = 1;
   %preventTime = %this.stateTimeoutValue[4];
   %obj.reloadSchedule = schedule(%preventTime * 1000, %obj, resetFire, %obj);

   if( %obj.getEnergyLevel() < %this.minEnergy ) // z0dd - ZOD, 5/22/03. Check energy level first
   {
      %obj.playAudio(0, ShockLanceMissSound);
      return;
   }
   if( %obj.isCloaked() )
   {
      if( %obj.respawnCloakThread !$= "" )
      {
         Cancel(%obj.respawnCloakThread);
         %obj.setCloaked( false );
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

   %muzzlePos = %obj.getMuzzlePoint(%slot);
   %muzzleVec = %obj.getMuzzleVector(%slot);
   %endPos    = VectorAdd(%muzzlePos, VectorScale(%muzzleVec, %this.projectile.extension));
   %damageMasks = $TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType |
                  $TypeMasks::StationObjectType | $TypeMasks::GeneratorObjectType |
                  $TypeMasks::SensorObjectType | $TypeMasks::TurretObjectType;

   %everythingElseMask = $TypeMasks::TerrainObjectType |
                         $TypeMasks::InteriorObjectType |
                         $TypeMasks::ForceFieldObjectType |
                         $TypeMasks::StaticObjectType |
                         $TypeMasks::MoveableObjectType |
                         $TypeMasks::DamagableItemObjectType;

   // did I miss anything? players, vehicles, stations, gens, sensors, turrets
   %hit = ContainerRayCast(%muzzlePos, %endPos, %damageMasks | %everythingElseMask, %obj);
   %noDisplay = true;

   if(%hit !$= "0")
   {
      %obj.setEnergyLevel(%obj.getEnergyLevel() - %this.hitEnergy);
      %hitobj = getWord(%hit, 0);
      %hitpos = getWord(%hit, 1) @ " " @ getWord(%hit, 2) @ " " @ getWord(%hit, 3);

      if(%hitObj.getType() & %damageMasks)
      {
         //-----------------------------
         // THIS IS WHERE THE CHANGE IS
         //-----------------------------
         if(!%obj.isMounted() ||
            %obj.mVehicle != %hitObj ||
            %obj.team != %hitObj.team
            )
            %hitobj.applyImpulse(%hitpos, VectorScale(%muzzleVec, %this.projectile.impulse));
         //-----------------------------
         // THIS IS WHERE THE CHANGE IS
         //-----------------------------

         %obj.playAudio(0, ShockLanceHitSound);

         // This is truly lame, but we need the sourceobject property present...
         %p = new ShockLanceProjectile() {
            dataBlock        = %this.projectile;
            initialDirection = %obj.getMuzzleVector(%slot);
            initialPosition  = %obj.getMuzzlePoint(%slot);
            sourceObject     = %obj;
            sourceSlot       = %slot;
            targetId         = %hit;
         };
         MissionCleanup.add(%p);

         %damageMultiplier = 1.0;
         
         if(%hitObj.getDataBlock().getClassName() $= "PlayerData")
         {
            // Now we see if we hit from behind...
            %forwardVec = %hitobj.getForwardVector();
            %objDir2D   = getWord(%forwardVec, 0) @ " " @ getWord(%forwardVec,1) @ " " @ "0.0";
            %objPos     = %hitObj.getPosition();
            %dif        = VectorSub(%objPos, %muzzlePos);
            %dif        = getWord(%dif, 0) @ " " @ getWord(%dif, 1) @ " 0";
            %dif        = VectorNormalize(%dif);
            %dot        = VectorDot(%dif, %objDir2D);

            // 120 Deg angle test...
            // 1.05 == 60 degrees in radians
            if (%dot >= mCos(1.05))
            {
               // Rear hit
               %damageMultiplier = 3.0;
               if(!%hitObj.getOwnerClient().isAIControlled())
                  %hitObj.getOwnerClient().rearshot = 1; // z0dd - ZOD, 8/25/02. Added Lance rear shot messages
            }
            // --------------------------------------------------------------
            // z0dd - ZOD, 8/25/02. Added Lance rear shot messages
            else
            {
               if(!%hitObj.getOwnerClient().isAIControlled())
                  %hitObj.getOwnerClient().rearshot = 0;
            }
            // --------------------------------------------------------------
         }
         
         %totalDamage = %this.Projectile.DirectDamage * %damageMultiplier;
         %hitObj.getDataBlock().damageObject(%hitobj, %p.sourceObject, %hitpos, %totalDamage, $DamageType::ShockLance);

         %noDisplay = false;
      }
   } 

   if( %noDisplay )
   {
      // Miss
      %obj.setEnergyLevel(%obj.getEnergyLevel() - %this.missEnergy);
      %obj.playAudio(0, ShockLanceMissSound);

      %p = new ShockLanceProjectile() {
         dataBlock        = %this.projectile;
         initialDirection = %obj.getMuzzleVector(%slot);
         initialPosition  = %obj.getMuzzlePoint(%slot);
         sourceObject     = %obj;
         sourceSlot       = %slot;
      };
      MissionCleanup.add(%p);
   }
   // z0dd - ZOD, 4/10/04. AI hook
   if(%obj.client != -1)
      %obj.client.projectile = %p;

   return %p;
}

function Player::maxInventory(%this, %data) {
   %originalMax = Parent::maxInventory(%this, %data);

   if(%data.getName() $= "FlareGrenade") {
      return 16;
   }
   else if(%originalMax > 1) {
      return 999;
   }
   else {
      return %originalMax;
   }
}

};

function POTCGame::activatePackages(%game) {
	DefaultGame::activatePackages(%game);

	%game.setDefaultValues();
	$VehicleMax[HAPCFlyer] = 8;
	HAPCFlyer.jetEnergyDrain = 3.0; // default 4.5 (classic) or 3.6 (base)
}

function POTCGame::deactivatePackages(%game) {
	DefaultGame::deactivatePackages(%game);

	$VehicleMax[HAPCFlyer] = $POTC_DefaultHavoc_VehicleMax;
	$VehicleMax[BomberFlyer] = $POTC_DefaultBomber_VehicleMax;
	HAPCFlyer.jetEnergyDrain = $POTC_DefaultHavoc_jetEnergyDrain;
}

function POTCGame::setDefaultValues(%game) {
	// only run this function once to get the proper values
	if( $POTC_DefaultValuesSet )
		return;
	$POTC_DefaultValuesSet = 1;

	$POTC_DefaultHavoc_VehicleMax = $VehicleMax[HAPCFlyer];
	$POTC_DefaultHavoc_jetEnergyDrain = HAPCFlyer.jetEnergyDrain;

	$POTC_DefaultBomber_VehicleMax = $VehicleMax[BomberFlyer];
}

// remove the chaingun from spawn loadout
function POTCGame::equip(%game, %player)
{
   for(%i =0; %i<$InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

   //%player.setArmor("Light");
   %player.setInventory(RepairKit,1);
   %player.setInventory(FlareGrenade,999);
   %player.setInventory(Blaster,1);
   %player.setInventory(Disc,1);
   %player.setInventory(ShockLance, 1);
   %player.setInventory(DiscAmmo, 999);
   %player.setInventory(Beacon, 3);
   %player.setInventory(TargetingLaser, 1);
   %player.weaponCount = 3;
   
   %player.use("Blaster");
}