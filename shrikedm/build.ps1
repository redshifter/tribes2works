# if we're going to have a github, we might as well have a build command
del ShrikeDMGameType.vl2
Compress-Archive -Path scripts,missions -DestinationPath ShrikeDMGameType.zip
move ShrikeDMGameType.zip ShrikeDMGameType.vl2