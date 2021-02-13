//--------------------------------------------------------------------------
// MISSILE BARREL PACK
//--------------------------------------------------------------------------

datablock ShapeBaseImageData(MissileBarrelPackImage) : AABarrelPackImage
{
   shapeFile = "pack_barrel_missile.dts";
   item = MissileBarrelPack;
   turretBarrel = "MissileBarrelLarge";
};

datablock ItemData(MissileBarrelPack) : AABarrelPack
{
   shapeFile = "pack_barrel_missile.dts";
   image = "MissileBarrelPackImage";
   pickUpName = "a missile barrel pack";
};
