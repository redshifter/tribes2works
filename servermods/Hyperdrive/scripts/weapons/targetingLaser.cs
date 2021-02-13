//--------------------------------------------------------------------------
// Targeting laser
// 
//--------------------------------------------------------------------------
datablock EffectProfile(TargetingLaserSwitchEffect)
{
   effectname = "weapons/generic_switch";
   minDistance = 2.5;
   maxDistance = 2.5;
};

datablock EffectProfile(TargetingLaserPaintEffect)
{
   effectname = "weapons/targetinglaser_paint";
   minDistance = 2.5;
   maxDistance = 2.5;
};

datablock AudioProfile(TargetingLaserSwitchSound)
{
   filename    = "fx/weapons/generic_switch.wav";
   description = AudioClosest3d;
   preload = true;
   effect = TargetingLaserSwitchEffect;
};

datablock AudioProfile(TargetingLaserPaintSound)
{
   filename    = "fx/weapons/targetinglaser_paint.wav";
   description = CloseLooping3d;
   preload = true;
   effect = TargetingLaserPaintEffect;
};


//--------------------------------------
// Projectile
//--------------------------------------
datablock TargetProjectileData(BasicTargeter)
{
   directDamage        	= 0.0;
   hasDamageRadius     	= false;
   indirectDamage      	= 0.0;
   damageRadius        	= 0.0;
   velInheritFactor    	= 1.0;

   maxRifleRange       	= 1000;
   beamColor           	= "0.1 1.0 0.1";
								
   startBeamWidth			= 0.20;
   pulseBeamWidth 	   = 0.15;
   beamFlareAngle 	   = 3.0;
   minFlareSize        	= 0.0;
   maxFlareSize        	= 400.0;
   pulseSpeed          	= 6.0;
   pulseLength         	= 0.150;

   textureName[0]      	= "special/nonlingradient";
   textureName[1]      	= "special/flare";
   textureName[2]      	= "special/pulse";
   textureName[3]      	= "special/expFlare";
   beacon               = true;
};

//--------------------------------------
// Rifle and item...
//--------------------------------------
datablock ItemData(TargetingLaser)
{
   className    = Weapon;
   catagory     = "Spawn Items";
   shapeFile    = "weapon_targeting.dts";
   image        = TargetingLaserImage;
   mass         = 1;
   elasticity   = 0.2;
   friction     = 0.6;
   pickupRadius = 2;
	pickUpName = "a targeting laser rifle";

   computeCRC = true;

};

datablock ShapeBaseImageData(TargetingLaserImage)
{
   className = WeaponImage;

   shapeFile = "weapon_targeting.dts";
   item = TargetingLaser;
   offset = "0 0 0";

   projectile = BasicTargeter;
   projectileType = TargetProjectile;
   deleteLastProjectile = true;
   
   isTargetLaser = true;

	usesEnergy = true;
	minEnergy = 0;

   stateName[0]                     = "Activate";
   stateSequence[0]                 = "Activate";
	stateSound[0]                    = TargetingLaserSwitchSound;
   stateTimeoutValue[0]             = 0.01;
   stateTransitionOnTimeout[0]      = "ActivateReady";

   stateName[1]                     = "ActivateReady";
   stateTransitionOnAmmo[1]         = "Ready";
   stateTransitionOnNoAmmo[1]       = "NoAmmo";

   stateName[2]                     = "Ready";
   stateTransitionOnNoAmmo[2]       = "NoAmmo";
   stateTransitionOnTriggerDown[2]  = "Fire";

   stateName[3]                     = "Fire";
	stateEnergyDrain[3]              = 0;
   stateFire[3]                     = true;
   //stateAllowImageChange[3]         = false;
   stateScript[3]                   = "onFire";
   stateTransitionOnTriggerUp[3]    = "Deconstruction";
   stateTransitionOnNoAmmo[3]       = "Deconstruction";
   stateSound[3]                    = TargetingLaserPaintSound;

   stateName[4]                     = "NoAmmo";
   stateTransitionOnAmmo[4]         = "Ready";

   stateName[5]                     = "Deconstruction";
   stateScript[5]                   = "deconstruct";
   stateTransitionOnTimeout[5]      = "Ready";
};

// super special targeting laser
datablock ShapeBaseImageData(RapidTargetingLaserImage)
{
   className = WeaponImage;

   shapeFile = "weapon_targeting.dts";
   item = TargetingLaser;
   offset = "0 0 0";

   projectile = BasicTargeter;
   projectileType = TargetProjectile;
   deleteLastProjectile = true;
   
   isTargetLaser = true;

	usesEnergy = true;
	minEnergy = 0;

   stateName[0]                     = "Activate";
   stateSequence[0]                 = "Activate";
	stateSound[0]                    = TargetingLaserSwitchSound;
   stateTimeoutValue[0]             = 0.01;
   stateTransitionOnTimeout[0]      = "ActivateReady";

   stateName[1]                     = "ActivateReady";
   stateTransitionOnAmmo[1]         = "Ready";
   stateTransitionOnNoAmmo[1]       = "NoAmmo";

   stateName[2]                     = "Ready";
   stateTransitionOnNoAmmo[2]       = "NoAmmo";
   stateTransitionOnTriggerDown[2]  = "Fire";

   stateName[3]                     = "Fire";
	stateEnergyDrain[3]              = 0;
   stateFire[3]                     = true;
   //stateAllowImageChange[3]         = false;
   stateScript[3]                   = "onFire";
   stateTransitionOnTriggerUp[3]    = "Deconstruction";
   stateTransitionOnNoAmmo[3]       = "Deconstruction";
   stateSound[3]                    = TargetingLaserPaintSound;
   stateTimeoutValue[3]             = 0.1;
   stateTransitionOnTimeout[3]      = "Reconstruct";

   stateName[4]                     = "NoAmmo";
   stateTransitionOnAmmo[4]         = "Ready";

   stateName[5]                     = "Deconstruction";
   stateScript[5]                   = "deconstruct";
   stateTransitionOnTimeout[5]      = "Ready";

   stateName[6]                     = "Reconstruct";
   stateScript[6]                   = "deconstruct";
   stateTransitionOnTimeout[6]      = "Fire";
};

// Black	0.0 0.0 0.0
// Red		1.0 0.1 0.1
// Orange	1.0 0.7 0.1
// Yellow	1.0 1.0 0.1
// Green	0.1 1.0 0.1
// Blue		0.1 0.1 1.0
// Purple	0.7 0.1 1.0
// White	1.0 1.0 1.0

$TargetingLaserColors = 10;
$LaserColor[1] = "<color:000000>Black";
$LaserColor[2] = "<color:FF0000>Red";
$LaserColor[3] = "<color:FFB000>Orange";
$LaserColor[4] = "<color:FFFF00>Yellow";
$LaserColor[5] = "<color:00FF00>Green";
$LaserColor[6] = "<color:0000FF>Blue";
$LaserColor[7] = "<color:B000FF>Purple";
$LaserColor[8] = "<color:FFFFFF>White";
$LaserColor[9] = "<color:FF0000>American<color:FFFFFF> Dream<color:0000FF>";
$LaserColor[10] = "<color:FF0000>R<color:FFB000>a<color:FFFF00>i<color:00FF00>n<color:0000FF>b<color:B000FF>o<color:FFFFFF>w<spush>";

// special lasers
datablock TargetProjectileData(BasicTargeter1) : BasicTargeter { beamColor = "0.0 0.0 0.0"; };
datablock TargetProjectileData(BasicTargeter2) : BasicTargeter { beamColor = "1.0 0.1 0.1"; };
datablock TargetProjectileData(BasicTargeter3) : BasicTargeter { beamColor = "1.0 0.7 0.1"; };
datablock TargetProjectileData(BasicTargeter4) : BasicTargeter { beamColor = "1.0 1.0 0.1"; };
datablock TargetProjectileData(BasicTargeter5) : BasicTargeter { beamColor = "0.1 1.0 0.1"; };
datablock TargetProjectileData(BasicTargeter6) : BasicTargeter { beamColor = "0.1 0.1 1.0"; };
datablock TargetProjectileData(BasicTargeter7) : BasicTargeter { beamColor = "0.7 0.1 1.0"; };
datablock TargetProjectileData(BasicTargeter8) : BasicTargeter { beamColor = "1.0 1.0 1.0"; };

// special weapon images
datablock ShapeBaseImageData(TargetingLaserImage1) : TargetingLaserImage { projectile = BasicTargeter1; };
datablock ShapeBaseImageData(TargetingLaserImage2) : TargetingLaserImage { projectile = BasicTargeter2; };
datablock ShapeBaseImageData(TargetingLaserImage3) : TargetingLaserImage { projectile = BasicTargeter3; };
datablock ShapeBaseImageData(TargetingLaserImage4) : TargetingLaserImage { projectile = BasicTargeter4; };
datablock ShapeBaseImageData(TargetingLaserImage5) : TargetingLaserImage { projectile = BasicTargeter5; };
datablock ShapeBaseImageData(TargetingLaserImage6) : TargetingLaserImage { projectile = BasicTargeter6; };
datablock ShapeBaseImageData(TargetingLaserImage7) : TargetingLaserImage { projectile = BasicTargeter7; };
datablock ShapeBaseImageData(TargetingLaserImage8) : TargetingLaserImage { projectile = BasicTargeter8; };

// Patriot Laser (cycles red-white-blue)
datablock ShapeBaseImageData(TargetingLaserImage9) : RapidTargetingLaserImage { projectile = BasicTargeter6; };

// Rainbow Laser (random selection)
datablock ShapeBaseImageData(TargetingLaserImage01) : RapidTargetingLaserImage { projectile = BasicTargeter6; };
datablock ShapeBaseImageData(TargetingLaserImage02) : RapidTargetingLaserImage { projectile = BasicTargeter6; };
datablock ShapeBaseImageData(TargetingLaserImage03) : RapidTargetingLaserImage { projectile = BasicTargeter6; };
datablock ShapeBaseImageData(TargetingLaserImage04) : RapidTargetingLaserImage { projectile = BasicTargeter6; };
datablock ShapeBaseImageData(TargetingLaserImage05) : RapidTargetingLaserImage { projectile = BasicTargeter6; };
datablock ShapeBaseImageData(TargetingLaserImage06) : RapidTargetingLaserImage { projectile = BasicTargeter6; };

function TargetingLaserIncrement( %step ) {
   if( !isObject(TargetingLaser) ) return;
   
   // Patriot Laser
   if( %step % 3 == 0 )
      TargetingLaserImage9.projectile = BasicTargeter2;
   else if( %step % 3 == 1 )
      TargetingLaserImage9.projectile = BasicTargeter8;
   else if( %step % 3 == 2 )
      TargetingLaserImage9.projectile = BasicTargeter6;
   
   // Rainbow Laser
   TargetingLaserImage01.projectile = "BasicTargeter" @ ((%step + 0) % 6 + 2);
   TargetingLaserImage02.projectile = "BasicTargeter" @ ((%step + 1) % 6 + 2);
   TargetingLaserImage03.projectile = "BasicTargeter" @ ((%step + 2) % 6 + 2);
   TargetingLaserImage04.projectile = "BasicTargeter" @ ((%step + 3) % 6 + 2);
   TargetingLaserImage05.projectile = "BasicTargeter" @ ((%step + 4) % 6 + 2);
   TargetingLaserImage06.projectile = "BasicTargeter" @ ((%step + 5) % 6 + 2);
   
   %step++;
   if( %step == 6 ) %step = 0;
   schedule(100, 0, TargetingLaserIncrement, %step);
}

TargetingLaserIncrement(0);