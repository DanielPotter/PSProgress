BeforeAll {
    $Script:DefaultDateTimeProvider = [PSProgress.DateTimeProvider]::Default
}

Describe 'Write-PipelineProgress' {

    BeforeEach {
        [PSProgress.DateTimeProvider]::Default = $Script:DefaultDateTimeProvider
    }

    It 'Writes all progress when forced' {
        $mockDateTimeProvider = [PSProgress.Tests.MockDateTimeProvider]::new()
        $mockDateTimeProvider.CurrentTime = Get-Date -Year 2024 -Month 1 -Day 1 -Hour 0 -Minute 0 -Second 0 -Millisecond 0
        [PSProgress.DateTimeProvider]::Default = $mockDateTimeProvider

        $items = 1..4
        $activity = "Counting"
        $expectedResults = @(
            @{
                Activity = $activity
                PercentComplete = 0
                SecondsRemaining = -1
            }
            1
            @{
                Activity = $activity
                PercentComplete = 25
                SecondsRemaining = 3
            }
            2
            @{
                Activity = $activity
                PercentComplete = 50
                SecondsRemaining = 2
            }
            3
            @{
                Activity = $activity
                PercentComplete = 75
                SecondsRemaining = 1
            }
            4
            @{
                Activity = $activity
                Complete = $true
            }
        )

        $forceDisplayParameters = @{
            # Setting these to 0 will force progress to always be written.
            RefreshInterval = New-TimeSpan
            DisplayThreshold = New-TimeSpan
            MinimumTimeLeftToDisplay = New-TimeSpan
        }

        $resultIndex = [ref] 0
        $items | ForEach-Object {
            Write-Output $_
        } | Write-PipelineProgress -Activity $activity @forceDisplayParameters -Debug 5>&1 | ForEach-Object {
            $expectedResult = $expectedResults[$resultIndex.Value]
            if ($expectedResult -is [int]) {
                $_ | Should -Be $expectedResult
                $mockDateTimeProvider.CurrentTime += New-TimeSpan -Seconds 1
            }
            else {
                $_ | Should -BeOfType [System.Management.Automation.DebugRecord]
                if ($expectedResult.Complete) {
                    $completePattern = 'Progress (?<id>\d+), Activity=<(?<activity>[^>]*)>, Completed'
                    $_.Message | Should -Match $completePattern
                    if ($_.Message -match $completePattern) {
                        if ($expectedResult.Activity) {
                            $Matches.activity | Should -Be $expectedResult.Activity
                        }
                    }
                }
                else {
                    $progressPattern = 'Progress (?<id>\d+), Activity=<(?<activity>[^>]*)>, Status=<(?<status>[^>]*)>, Operation=<(?<operation>[^>]*)>, PercentComplete=<(?<percentComplete>-?\d*)>, SecondsRemaining=<(?<secondsRemaining>-?\d*)>'
                    $_.Message | Should -Match $progressPattern
                    if ($_.Message -match $progressPattern) {
                        if ($expectedResult.Activity) {
                            $Matches.activity | Should -Be $expectedResult.Activity
                        }
                        if ($null -ne $expectedResult.PercentComplete) {
                            [int] $Matches.percentComplete | Should -Be $expectedResult.PercentComplete
                        }
                        if ($null -ne $expectedResult.SecondsRemaining) {
                            [int] $Matches.secondsRemaining | Should -Be $expectedResult.SecondsRemaining
                        }
                    }
                }
            }
            $resultIndex.Value++
        }
    }
}