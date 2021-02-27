# if we're going to have a github, we might as well have a build command
del BlasterDMGameType.vl2
Compress-Archive -Path Readme.txt,missions,terrains,scripts,../LICENSE -DestinationPath BlasterDMGameType.zip
ren BlasterDMGameType.zip BlasterDMGameType.vl2

del BlasterDM_Client.vl2
cd blasterdm_client
Compress-Archive -Path scripts,../../LICENSE -DestinationPath BlasterDM_Client.zip
move BlasterDM_Client.zip ..\BlasterDM_Client.vl2