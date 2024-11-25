param (
    [int] $Digits = 750
)

& "$PSScriptRoot\..\RunWithModule.ps1" -Variables @{
    Digits = $Digits
    PICalculatorFilePath = "$PSScriptRoot\PICalculator.cs"
} -Script {
    Add-Type -TypeDefinition (Get-Content $PICalculatorFilePath -Raw)

    function Get-PIDigit {
        [CmdletBinding()]
        param (
            [Parameter()]
            [int] $Digit
        )

        process {
            $result = [PICalculator]::GetPiDigit($Digit)
            return [int] $result.ToString()[0]
        }
    }

    # Define similar examples to run and compare timings for efficiency.
    @(
        @{
            Name = "No Progress"
            Script = {
                $result = [ref] '3.'
                1..$Digits | ForEach-Object {
                    $result.Value += Get-PIDigit $_
                }
            }
        }
        @{
            Name = "Write-Progress"
            Script = {
                end {
                    $result = [ref] '3.'
                    1..$Digits | ForEach-Object {
                        Write-Progress -Activity "Calculate PI Digits" -PercentComplete ([double]$_/$Digits*100)
                        $result.Value += Get-PIDigit $_
                    }
                }
                clean {
                    Write-Progress -Activity "Calculate PI Digits" -Completed
                }
            }
        }
        @{
            Name = "Write-PipelineProgress"
            Script = {
                $result = [ref] '3.'
                1..$Digits | Write-PipelineProgress -Activity "Calculate PI Digits" | ForEach-Object {
                    $result.Value += Get-PIDigit $_
                }
            }
        }
    ) | ForEach-Object {
        Write-Host "Start: $($_.Name)"
        $startTime = Get-Date
        & $_.Script
        $duration = New-TimeSpan -Start $startTime
        Write-Host "End: $duration"
    }
}
