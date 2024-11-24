BeforeAll {
    $Script:DefaultDateTimeProvider = [PSProgress.DateTimeProvider]::Default
    $Script:ProgressCompletePattern = 'Progress (?<id>\d+), Activity=<(?<activity>[^>]*)>, Completed'
    $Script:ProgressProcessingPattern = 'Progress (?<id>\d+), Activity=<(?<activity>[^>]*)>, Status=<(?<status>[^>]*)>, Operation=<(?<operation>[^>]*)>, PercentComplete=<(?<percentComplete>-?\d*)>, SecondsRemaining=<(?<secondsRemaining>-?\d*)>'

    $Script:ForceAllProgressParameters = @{
        # Setting these to 0 will force progress to always be written.
        RefreshInterval = New-TimeSpan
        DisplayThreshold = New-TimeSpan
        MinimumTimeLeftToDisplay = New-TimeSpan
    }
}

Describe 'ProgressSession' {
    BeforeEach {
        [PSProgress.DateTimeProvider]::Default = $Script:DefaultDateTimeProvider
    }

    It 'Writes progress' {
        $mockDateTimeProvider = [PSProgress.Tests.MockDateTimeProvider]::new()
        $mockDateTimeProvider.CurrentTime = Get-Date -Year 2024 -Month 1 -Day 1 -Hour 0 -Minute 0 -Second 0 -Millisecond 0
        [PSProgress.DateTimeProvider]::Default = $mockDateTimeProvider

        $activityId = 10
        $session = New-ProgressSession -Activity 'Progress Session' -Id $activityId -ExpectedCount 10 -CurrentOperation { $_ } @Script:ForceAllProgressParameters

        1..10 | ForEach-Object {
            $item = $_
            $result = $item | Update-ProgressSession -Session $session -Debug 5>&1
            $result | Should -BeOfType [System.Management.Automation.DebugRecord]
            $result.Message | Should -Match $Script:ProgressProcessingPattern
            if ($result.Message -match $Script:ProgressProcessingPattern) {
                [int] $Matches.percentComplete | Should -Be (($item - 1) * 10)
                [int] $Matches.id | Should -Be $activityId
                $Matches.operation | Should -Be $item
            }

            $mockDateTimeProvider.CurrentTime += New-TimeSpan -Seconds 1
        }

        $result = Stop-ProgressSession -Session $session -Debug 5>&1
        $result | Should -BeOfType [System.Management.Automation.DebugRecord]
        $result.Message | Should -Match $Script:ProgressCompletePattern
        if ($result.Message -match $Script:ProgressProcessingPattern) {
            [int] $Matches.id | Should -Be $activityId
        }
    }
}
