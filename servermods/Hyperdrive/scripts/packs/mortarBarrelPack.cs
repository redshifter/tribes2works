//--------------------------------------------------------------------------
// MORTAR BARREL PACK
//--------------------------------------------------------------------------

datablock ShapeBaseImageData(MortarBarrelPackImage) : AABarrelPackImage
{
   shapeFile = "pack_barrel_mortar.dts";
   item = MortarBarrelPack;
   turretBarrel = "MortarBarrelLarge";
};

datablock ItemData(MortarBarrelPack) : AABarrelPack
{
   shapeFile = "pack_barrel_mortar.dts";
   image = "MortarBarrelPackImage";
   pickUpName = "a mortar barrel pack";
};
