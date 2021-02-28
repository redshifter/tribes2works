Get-ChildItem -Path "." -Recurse -Filter build.ps1 |
ForEach-Object {
	cd $_.Directory
	& ./build.ps1
}