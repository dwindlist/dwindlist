# see https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
$script:target = "win-x64"

function script:_build {
	$script:buildExists = (Test-Path -Path ./build)
	if (-Not $script:buildExists) {
		Set-Location ./dwindlist
		dotnet publish -r $script:target -o ../build/$script:target/dwindlist --self-contained
		dotnet ef migrations bundle -o ../build/$script:target/dwindlist/efbundle.exe --self-contained
		$script:dwindlistDbExists = (SqlLocalDB.exe i) -contains "dwindlistdb"
		if (-Not ($script:dwindlistDbExists)) {
			Set-Location ../build/$script:target/dwindlist
			SqlLocalDB.exe c dwindlistdb
			./efbundle.exe
		}
		Set-Location ../../..
	}
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
			Set-Location ./build/$script:target/dwindlist
			7z.exe a -tzip ../../$script:target *
			Set-Location ../../..
		}
	}
	"clean" {
		cd ./dwindlist
		dotnet clean
		cd ..
		Remove-Item -r -fo ./build -ErrorAction SilentlyContinue
		SqlLocalDB.exe p dwindlistdb
		SqlLocalDB.exe d dwindlistdb
		Remove-Item $HOME/dwindlist_log.ldf -ErrorAction SilentlyContinue
		Remove-Item $HOME/dwindlist.mdf -ErrorAction SilentlyContinue
	}
}
