# if we're going to have a github, we might as well have a build command
del Hyperdrive.zip
md Hyperdrive
move scripts Hyperdrive
Compress-Archive -Path Hyperdrive -DestinationPath Hyperdrive.zip
cd Hyperdrive
move scripts ..
cd ..
rd Hyperdrive
