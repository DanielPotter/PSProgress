$rootDirectory = Split-Path $PSScriptRoot

$modulePath = Resolve-Path "$rootDirectory\PSProgress\bin\Debug\netstandard2.0\PSProgress.psd1" -ErrorAction Stop
$testsPath = Resolve-Path "$rootDirectory\PSProgress.Tests\bin\Debug\net7.0\PSProgress.Tests.dll" -ErrorAction Stop

pwsh.exe -NoProfile -WorkingDirectory $rootDirectory -Command "&{ Import-Module ""$($modulePath.Path)"" -Verbose -ErrorAction Stop; Add-Type -Path ""$($testsPath.Path)"" -Verbose -ErrorAction Stop; Invoke-Pester }"
