# if we're going to have a github, we might as well have a build command
del POTCGameType.vl2
Compress-Archive -Path missions,terrains,scripts,../LICENSE -DestinationPath POTCGameType.zip
ren POTCGameType.zip POTCGameType.vl2