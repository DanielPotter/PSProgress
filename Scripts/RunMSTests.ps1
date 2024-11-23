$rootDirectory = Split-Path $PSScriptRoot

Push-Location $rootDirectory
try {
    dotnet.exe test
}
finally {
    Pop-Location
}
