[CmdletBinding()]
param (
    [Parameter(
        Mandatory = $true,
        Position = 0
    )]
    [scriptblock] $Script,

    [Parameter()]
    [string] $WorkingDirectory
)

$rootDirectory = Split-Path $PSScriptRoot

$modulePath = Resolve-Path "$rootDirectory\PSProgress\bin\Debug\netstandard2.0\PSProgress.psd1" -ErrorAction Stop
$testsPath = Resolve-Path "$rootDirectory\PSProgress.Tests\bin\Debug\net7.0\PSProgress.Tests.dll" -ErrorAction Stop

if (-not $WorkingDirectory) {
    $WorkingDirectory = $rootDirectory
}

pwsh.exe -NoProfile -WorkingDirectory $WorkingDirectory -Command @"
&{
    Import-Module "$($modulePath.Path)" -Verbose -ErrorAction Stop
    Add-Type -Path "$($testsPath.Path)" -Verbose -ErrorAction Stop
    & {$Script}
}
"@
