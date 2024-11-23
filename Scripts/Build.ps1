$rootDirectory = Split-Path $PSScriptRoot

Push-Location $rootDirectory
try {
    dotnet.exe build
}
finally {
    Pop-Location
}
