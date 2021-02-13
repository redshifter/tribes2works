//--------------------------------------------------------------------------
// ELF BARREL PACK
//--------------------------------------------------------------------------

datablock ShapeBaseImageData(ELFBarrelPackImage) : AABarrelPackImage
{
   shapeFile = "pack_barrel_elf.dts";
   item = ELFBarrelPack;
   turretBarrel = "ELFBarrelLarge";    
};

datablock ItemData(ELFBarrelPack) : AABarrelPack
{
   shapeFile = "pack_barrel_elf.dts";
   image = "ELFBarrelPackImage";
   pickUpName = "an ELF barrel pack";
};
