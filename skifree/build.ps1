# if we're going to have a github, we might as well have a build command
del SkiFreeGameType.vl2
Compress-Archive -Path scripts,missions,terrains,audio,textures,other,../LICENSE -DestinationPath SkiFreeGameType.zip
move SkiFreeGameType.zip SkiFreeGameType.vl2