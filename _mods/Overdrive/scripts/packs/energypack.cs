$OverdriveLength = 20; // in increments of 500ms

// ------------------------------------------------------------------
// ENERGY PACK
// can be used by any armor type
// does not have to be activated
// increases the user's energy recharge rate

datablock ShapeBaseImageData(EnergyPackImage)
{
   shapeFile = "pack_upgrade_energy.dts";
   item = EnergyPack;
   mountPoint = 1;
   offset = "0 0 0";
   rechargeRateBoost = 0.0; // no recharge rate boosting here

   usesEnergy = true;
   minEnergy = 0;

   stateName[0] = "Idle";
   stateTransitionOnTriggerDown[0] = "Activate";

   stateName[1] = "Activate";
   stateScript[1] = "onActivate";
   stateTransitionOnTriggerUp[1] = "ReadyRelease";

   stateName[2] = "ReadyRelease";
   stateTransitionOnTimeout[2] = "Idle";
   stateTimeoutValue[2] = 0.1;
};

datablock ItemData(EnergyPack)
{
   className = Pack;
   catagory = "Packs";
   shapeFile = "pack_upgrade_energy.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   rotate = true;
   image = "EnergyPackImage";
	pickUpName = "an overdrive pack";

   computeCRC = true;

};



function EnergyPackImage::onMount(%data, %obj, %node)
{
   %obj.hasEnergyPack = true; // set for sniper check
}

function EnergyPackImage::onUnmount(%data, %obj, %node)
{
   %obj.hasEnergyPack = "";
}

function EnergyPack::onPickup(%this, %obj, %shape, %amount)
{
	// created to prevent console errors
}

function EnergyPackImage::onActivate(%data, %obj, %slot)
{
	// TODO: crazy shit
	%weapon = %obj.getMountedImage($WeaponSlot).item;

	if( %weapon $= Blaster )
		%obj.mountImage(BlasterOverdriveImage, $WeaponSlot);
	else if( %weapon $= Plasma )
		%obj.mountImage(PlasmaOverdriveImage, $WeaponSlot);
	else if( %weapon $= Disc )
		%obj.mountImage(DiscOverdriveImage, $WeaponSlot);
	else if( %weapon $= GrenadeLauncher )
		%obj.mountImage(GrenadeLauncherOverdriveImage, $WeaponSlot);
	else if( %weapon $= Mortar )
		%obj.mountImage(MortarOverdriveImage, $WeaponSlot);
	else if( %weapon $= ShockLance )
		%obj.mountImage(ShockLanceOverdriveImage, $WeaponSlot);
	else if( %weapon $= superBlaster ) {
		// don't remove the energy pack on the hyper blaster
		%obj.HyperMode = !%obj.HyperMode;
		return;
	}
	else
		return;

	%obj.setInventory(EnergyPack, 0);

	%obj.setInvincibleMode($OverdriveLength / 2, 0.02);
	decOverdrive(%obj, $OverdriveLength);
}

function decOverdrive( %obj, %count ) {
	%debug = 0;
	if( %obj.getState() $= "Dead" ) {
		%obj.setInvincibleMode(0,0);
		if(%debug) error("overdrive over: dead");
		return;
	}

	%weapon = %obj.getMountedImage($WeaponSlot).item;
	if( %count > 0 ) {
		if( %weapon $= BlasterOverdrive || 
			%weapon $= PlasmaOverdrive ||
			%weapon $= DiscOverdrive ||
			%weapon $= GrenadeLauncherOverdrive ||
			%weapon $= MortarOverdrive ||
			%weapon $= ShockLanceOverdrive ) {
			// continue overdrive	
		}
		else {
			%obj.client.setAmmoHudCount("");
			%obj.setInvincibleMode(0,0);
			if(%debug) error("overdrive over: weapon change");
			schedule(1000,0,resetOverdriveWeapon,%obj);
			return;
		}
	}
	else {
		resetOverdriveWeapon(%obj);

		%obj.client.setAmmoHudCount("");
		%obj.setInvincibleMode(0,0);
		if(%debug) error("overdrive over: timeout");
		return;
	}
	%obj.client.setAmmoHudCount(mCeil(%count / 2));
	makeOverdriveAura(%obj);
	schedule(500, 0, decOverdrive, %obj, %count - 1);

}


function resetOverdriveWeapon( %obj ) {
	%weapon = %obj.getMountedImage($WeaponSlot).item;

	if( %weapon $= BlasterOverdrive )
		%obj.mountImage(BlasterImage, $WeaponSlot);
	else if( %weapon $= PlasmaOverdrive )
		%obj.mountImage(PlasmaImage, $WeaponSlot);
	else if( %weapon $= DiscOverdrive )
		%obj.mountImage(DiscImage, $WeaponSlot);
	else if( %weapon $= GrenadeLauncherOverdrive )
		%obj.mountImage(GrenadeLauncherImage, $WeaponSlot);
	else if( %weapon $= MortarOverdrive )
		%obj.mountImage(MortarImage, $WeaponSlot);
	else if( %weapon $= ShockLanceOverdrive )
		%obj.mountImage(ShockLanceImage, $WeaponSlot);
}




//----------------------------
// Crazy Aura ****
//----------------------------

datablock ShockwaveData(AuraBlasterShockwave)
{
   width = 4.0;
   numSegments = 20;
   numVertSegments = 2;
   velocity = 5;
   acceleration = 10.0;
   lifetimeMS = 750;
   height = 1.0; // was 1.0
   is2D = true; // was true

   texture[0] = "special/shockwave4";
   texture[1] = "special/gradient";
   texWrap = 6.0;

   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;

   colors[0] = "1.0 0.0 0.0 1.00";
   colors[1] = "1.0 0.0 0.0 0.20";
   colors[2] = "1.0 1.0 0.0 0.0";
};
datablock ShockwaveData(AuraPlasmaShockwave) : AuraBlasterShockwave {
   colors[0] = "1.0 0.5 0.0 1.00";
   colors[1] = "1.0 0.5 0.0 0.20";
};
datablock ShockwaveData(AuraDiscShockwave) : AuraBlasterShockwave {
   colors[0] = "0.0 0.0 1.0 1.00";
   colors[1] = "0.0 0.0 1.0 0.20";
};
datablock ShockwaveData(AuraGrenadeShockwave) : AuraBlasterShockwave {
   colors[0] = "0.5 0.5 0.5 1.00";
   colors[1] = "0.5 0.5 0.5 0.20";
};
datablock ShockwaveData(AuraMortarShockwave) : AuraBlasterShockwave {
   colors[0] = "0.0 1.0 0.0 1.00";
   colors[1] = "0.0 1.0 0.0 0.20";
};
datablock ShockwaveData(AuraLanceShockwave) : AuraBlasterShockwave {
   colors[0] = "1.0 1.0 1.0 1.00";
   colors[1] = "1.0 1.0 1.0 0.20";
};


datablock AudioProfile(AuraExplosionSound)
{
   filename = "fx/Bonuses/down_passback2_moyoyo.wav";
   description = AudioExplosion3d;
   preload = true;
   effect = ConcussionGrenadeExplosionEffect;
};


datablock ExplosionData(AuraBlasterEXP)
{
   soundProfile   = AuraExplosionSound;
   shockwave =  AuraBlasterShockwave;

   shakeCamera = false;
};
datablock ExplosionData(AuraPlasmaEXP) : AuraBlasterEXP {   shockwave = AuraPlasmaShockwave; };
datablock ExplosionData(AuraDiscEXP) : AuraBlasterEXP {   shockwave = AuraDiscShockwave; };
datablock ExplosionData(AuraGrenadeEXP) : AuraBlasterEXP {   shockwave = AuraGrenadeShockwave; };
datablock ExplosionData(AuraMortarEXP) : AuraBlasterEXP {    shockwave = AuraMortarShockwave; };
datablock ExplosionData(AuraLanceEXP) : AuraBlasterEXP {    shockwave = AuraLanceShockwave; };


// we use grenade objects to create the aura
// trying to invoke an explosion directly = ue city
datablock ItemData(AuraBlaster)
{
//   shapeFile = "grenade.dts";
   mass = 0.7;
   elasticity = 0.2;
   friction = 1;
   pickupRadius = 0;
   maxDamage = 0.5;
   explosion = AuraBlasterEXP;
   damageRadius        = 0.0;
   radiusDamageType    = 0;
   kickBackStrength    = 0;

   computeCRC = false;
};
datablock ItemData(AuraPlasma) : AuraBlaster { explosion = AuraPlasmaEXP; };
datablock ItemData(AuraDisc) : AuraBlaster { explosion = AuraDiscEXP; };
datablock ItemData(AuraGrenade) : AuraBlaster { explosion = AuraGrenadeEXP; };
datablock ItemData(AuraMortar) : AuraBlaster { explosion = AuraMortarEXP; };
datablock ItemData(AuraLance) : AuraBlaster { explosion = AuraLanceEXP; };


function makeOverdriveAura(%obj) {
   %aura = %obj.getMountedImage($WeaponSlot).aura;

   %auraItem = new Item()
   {
      dataBlock = %aura;
      sourceObject = %obj;
   };
   MissionCleanup.add(%auraItem);

   %auraItem.setTransform(getBoxCenter(%obj.getWorldBox()));
   %auraItem.schedule(100, "setDamageState", "Destroyed");
   %auraItem.schedule(1000, "delete");
}
