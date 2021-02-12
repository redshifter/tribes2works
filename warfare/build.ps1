# if we're going to have a github, we might as well have a build command
del WarfareGameType.vl2
Compress-Archive -Path missions,terrains,scripts -DestinationPath WarfareGameType.zip
ren WarfareGameType.zip WarfareGameType.vl2