BeforeAll {
    $Script:DefaultDateTimeProvider = [PSProgress.DateTimeProvider]::Default
    $Script:ProgressCompletePattern = 'Progress (?<id>\d+), Activity=<(?<activity>[^>]*)>, Completed'
    $Script:ProgressProcessingPattern = 'Progress (?<id>\d+), Activity=<(?<activity>[^>]*)>, Status=<(?<status>[^>]*)>, Operation=<(?<operation>[^>]*)>, PercentComplete=<(?<percentComplete>-?\d*)>, SecondsRemaining=<(?<secondsRemaining>-?\d*)>'
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
        $items | Write-PipelineProgress -Activity $activity @forceDisplayParameters -Debug 5>&1 | ForEach-Object {
            $expectedResult = $expectedResults[$resultIndex.Value]
            if ($expectedResult -is [int]) {
                $_ | Should -Be $expectedResult
                $mockDateTimeProvider.CurrentTime += New-TimeSpan -Seconds 1
            }
            else {
                $_ | Should -BeOfType [System.Management.Automation.DebugRecord]
                if ($expectedResult.Complete) {
                    $_.Message | Should -Match $Script:ProgressCompletePattern
                    if ($_.Message -match $Script:ProgressCompletePattern) {
                        if ($expectedResult.Activity) {
                            $Matches.activity | Should -Be $expectedResult.Activity
                        }
                    }
                }
                else {
                    $_.Message | Should -Match $Script:ProgressProcessingPattern
                    if ($_.Message -match $Script:ProgressProcessingPattern) {
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

    Context 'Status' {
        It 'Executes with $_ automatic variable' {
            $forceDisplayParameters = @{
                # Setting these to 0 will force progress to always be written.
                RefreshInterval = New-TimeSpan
                DisplayThreshold = New-TimeSpan
                MinimumTimeLeftToDisplay = New-TimeSpan
            }

            $expectedValue = 42
            $result = $expectedValue | Write-PipelineProgress -Activity "Activity" @forceDisplayParameters -Status { $_ } -Debug 5>&1 | Select-Object -First 1

            $result | Should -BeOfType [System.Management.Automation.DebugRecord]
            $result.Message | Should -Match $Script:ProgressProcessingPattern
            if ($result.Message -match $Script:ProgressProcessingPattern) {
                $Matches.status | Should -Be $expectedValue.ToString()
            }
        }

        It 'Can access local variables' {
            $forceDisplayParameters = @{
                # Setting these to 0 will force progress to always be written.
                RefreshInterval = New-TimeSpan
                DisplayThreshold = New-TimeSpan
                MinimumTimeLeftToDisplay = New-TimeSpan
            }

            $expectedValue = 42
            $result = 1 | Write-PipelineProgress -Activity "Activity" @forceDisplayParameters -Status { $expectedValue } -Debug 5>&1 | Select-Object -First 1

            $result | Should -BeOfType [System.Management.Automation.DebugRecord]
            $result.Message | Should -Match $Script:ProgressProcessingPattern
            if ($result.Message -match $Script:ProgressProcessingPattern) {
                $Matches.status | Should -Be $expectedValue.ToString()
            }
        }
    }

    Context 'CurrentOperation' {
        It 'Executes with $_ automatic variable' {
            $forceDisplayParameters = @{
                # Setting these to 0 will force progress to always be written.
                RefreshInterval = New-TimeSpan
                DisplayThreshold = New-TimeSpan
                MinimumTimeLeftToDisplay = New-TimeSpan
            }

            $expectedValue = 42
            $result = $expectedValue | Write-PipelineProgress -Activity "Activity" @forceDisplayParameters -CurrentOperation { $_ } -Debug 5>&1 | Select-Object -First 1

            $result | Should -BeOfType [System.Management.Automation.DebugRecord]
            $result.Message | Should -Match $Script:ProgressProcessingPattern
            if ($result.Message -match $Script:ProgressProcessingPattern) {
                $Matches.operation | Should -Be $expectedValue.ToString()
            }
        }

        It 'Can access local variables' {
            $forceDisplayParameters = @{
                # Setting these to 0 will force progress to always be written.
                RefreshInterval = New-TimeSpan
                DisplayThreshold = New-TimeSpan
                MinimumTimeLeftToDisplay = New-TimeSpan
            }

            $expectedValue = 42
            $result = 1 | Write-PipelineProgress -Activity "Activity" @forceDisplayParameters -CurrentOperation { $expectedValue } -Debug 5>&1 | Select-Object -First 1

            $result | Should -BeOfType [System.Management.Automation.DebugRecord]
            $result.Message | Should -Match $Script:ProgressProcessingPattern
            if ($result.Message -match $Script:ProgressProcessingPattern) {
                $Matches.operation | Should -Be $expectedValue.ToString()
            }
        }
    }
}