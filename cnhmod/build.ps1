# if we're going to have a github, we might as well have a build command
del CaptureAndHoldMod.vl2
Compress-Archive -Path scripts,../LICENSE -DestinationPath CaptureAndHoldMod.zip
ren CaptureAndHoldMod.zip CaptureAndHoldMod.vl2
