# if we're going to have a github, we might as well have a build command
del UltraRS.zip
md UltraRS
move scripts UltraRS
copy ../../LICENSE UltraRS
Compress-Archive -Path UltraRS -DestinationPath UltraRS.zip
cd UltraRS
move scripts ..
del LICENSE
cd ..
rd UltraRS
