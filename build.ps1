function script:_build {
	$script:buildExists = (Test-Path -Path ./build)
	if (-Not $script:buildExists) {
		Set-Location ./dwindlist
		dotnet publish -o ../build --self-contained
		dotnet ef migrations bundle -o ../build/efbundle.exe --self-contained
		$script:dwindlistDbExists = (SqlLocalDB.exe i) -contains "dwindlistdb"
		if (-Not ($script:dwindlistDbExists)) {
			Set-Location ../build
			SqlLocalDB.exe c dwindlistdb
			./efbundle.exe
		}
		Set-Location ..
	}
}

if ($args.length -eq 0) {
	_build
}

switch ($args[0]) {
	"run" {
		_build
		Set-Location ./build
		try { ./dwindlist.exe } finally { Set-Location .. }
	}
	"package" {
		_build
		Set-Location ./build
    $script:packageExists = (Test-Path -Path ./dwindlist.zip)
		if (-Not $script:packageExists) {
			7z.exe a -tzip dwindlist *
		}
	}
	"clean" {
		Remove-Item -r -fo ./build -ErrorAction SilentlyContinue
		SqlLocalDB.exe p dwindlistdb
		SqlLocalDB.exe d dwindlistdb
		Remove-Item $HOME/dwindlist_log.ldf -ErrorAction SilentlyContinue
		Remove-Item $HOME/dwindlist.mdf -ErrorAction SilentlyContinue
	}
}
