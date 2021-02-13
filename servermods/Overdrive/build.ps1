# if we're going to have a github, we might as well have a build command
del Overdrive.zip
md Overdrive
move scripts Overdrive
Compress-Archive -Path Overdrive -DestinationPath Overdrive.zip
cd Overdrive
move scripts ..
cd ..
rd Overdrive
