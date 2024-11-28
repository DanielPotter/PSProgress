[CmdletBinding()]
param (
    [Parameter(
        Mandatory = $true,
        Position = 0
    )]
    [scriptblock] $Script,

    [Parameter()]
    [string] $WorkingDirectory,

    [Parameter()]
    [System.Collections.IDictionary] $Variables,

    [Parameter()]
    [string] $ModulePath
)

$rootDirectory = Split-Path $PSScriptRoot

try {
    $ModulePath = $ModulePath ? $ModulePath : "$rootDirectory\PSProgress\bin\Debug\net8.0\PSProgress.psd1" | Resolve-Path -ErrorAction Stop
    $testsPath = Resolve-Path "$rootDirectory\PSProgress.Tests\bin\Debug\net8.0\PSProgress.Tests.dll" -ErrorAction Stop
}
catch {
    throw $_
}

if (-not $WorkingDirectory) {
    $WorkingDirectory = $rootDirectory
}

$variableScriptLines = ""
if ($Variables.Count) {
    $variableJson = $Variables | ConvertTo-Json
    $variableScriptLines = @(
        '        $variablesJsonString = @"'
        $variableJson
        '"@'
        '        $variables = $variablesJsonString.Trim() | ConvertFrom-Json'
        $Variables.Keys | ForEach-Object {
            "        `$$_ = `$variables.$_"
        }
    ) | Join-String -Separator "`r`n"
}

$command = @"
&{
    try {
        Import-Module "$($ModulePath)" -Verbose -ErrorAction Stop
        Add-Type -Path "$($testsPath.Path)" -Verbose -ErrorAction Stop
        $variableScriptLines
    }
    catch {
        throw `$_
    }
    & {$Script}
}
"@

pwsh.exe -NoProfile -WorkingDirectory $WorkingDirectory -Command $command
