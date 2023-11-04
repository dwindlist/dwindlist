# see https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
$script:target = "win-x64"

function script:_build {
	$script:buildExists = (Test-Path -Path ./build/$script:target/dwindlist)
	if ($script:buildExists) { return }

	Set-Location ./dwindlist
	try {
		dotnet publish -r $script:target -o ../build/$script:target/dwindlist --self-contained
		dotnet ef migrations bundle -o ../build/$script:target/dwindlist/efbundle.exe --self-contained
	} finally { Set-Location .. }

	$script:dwindlistDbExists = (SqlLocalDB.exe i) -contains "dwindlistdb"
	if ($script:dwindlistDbExists) { return }

	Set-Location ./build/$script:target/dwindlist
	try {
		SqlLocalDB.exe c dwindlistdb
		./efbundle.exe
	} finally { Set-Location ../../.. }
}

if ($args.length -eq 0) {
	_build
}

switch ($args[0]) {
	"run" {
		_build
		Set-Location ./build/$script:target/dwindlist
		try { ./dwindlist.exe } finally { Set-Location ../../.. }
	}
	"package" {
		_build
		$script:packageExists = (Test-Path -Path ./build/$script:target.zip)
		if (-Not $script:packageExists) {
			7z.exe a -tzip ./build/$script:target ./build/$script:target/dwindlist
		}
	}
	"clean" {
		cd ./dwindlist
		try { dotnet clean } finally { cd .. }
		Remove-Item -r -fo ./build -ErrorAction SilentlyContinue
	}
	"nuke" {
		cd ./dwindlist
		try { dotnet clean } finally { cd .. }
		Remove-Item -r -fo ./build -ErrorAction SilentlyContinue
		SqlLocalDB.exe p dwindlistdb
		SqlLocalDB.exe d dwindlistdb
		Remove-Item $HOME/dwindlist_log.ldf -ErrorAction SilentlyContinue
		Remove-Item $HOME/dwindlist.mdf -ErrorAction SilentlyContinue
	}
}
