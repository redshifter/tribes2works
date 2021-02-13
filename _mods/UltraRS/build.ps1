# if we're going to have a github, we might as well have a build command
del UltraRS.zip
md UltraRS
move scripts UltraRS
Compress-Archive -Path UltraRS -DestinationPath UltraRS.zip
cd UltraRS
move scripts ..
cd ..
rd UltraRS
