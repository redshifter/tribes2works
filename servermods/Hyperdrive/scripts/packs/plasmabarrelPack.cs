//--------------------------------------------------------------------------
// PLASMA BARREL PACK
//--------------------------------------------------------------------------

datablock ShapeBaseImageData(PlasmaBarrelPackImage) : AABarrelPackImage 
{
   shapeFile = "pack_barrel_fusion.dts";
   item = PlasmaBarrelPack;
   turretBarrel = "PlasmaBarrelLarge";
};

datablock ItemData(PlasmaBarrelPack) : AABarrelPack
{
   shapeFile = "pack_barrel_fusion.dts";
   image = "PlasmaBarrelPackImage";
   pickUpName = "a plasma barrel pack";
};
