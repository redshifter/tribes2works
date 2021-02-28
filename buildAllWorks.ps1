Get-ChildItem -Path "." -Recurse -Filter *.ps1 |
ForEach-Object {
	cd $_.Directory
	& $_.FullName
}