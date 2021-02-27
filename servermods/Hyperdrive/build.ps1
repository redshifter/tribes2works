# if we're going to have a github, we might as well have a build command
del Hyperdrive.zip
md Hyperdrive
move scripts Hyperdrive
copy ../../LICENSE Hyperdrive
Compress-Archive -Path Hyperdrive -DestinationPath Hyperdrive.zip
cd Hyperdrive
move scripts ..
del LICENSE
cd ..
rd Hyperdrive
