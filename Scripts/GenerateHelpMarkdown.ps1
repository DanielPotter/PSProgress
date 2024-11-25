if (-not (Get-Module playPS -ErrorAction SilentlyContinue)) {
    Install-Module -Name platyPS -Scope CurrentUser -ErrorAction Stop
}

Import-Module platyPS

& "$PSScriptRoot\RunWithModule.ps1" {
    New-MarkdownHelp -Module PSProgress -OutputFolder .\docs
}
