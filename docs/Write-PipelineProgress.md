---
external help file: PSProgress.dll-Help.xml
Module Name: PSProgress
online version:
schema: 2.0.0
---

# Write-PipelineProgress

## SYNOPSIS
Displays a progress bar that updates as the items in the pipeline are processed.

## SYNTAX

```
Write-PipelineProgress [-InputObject <Object[]>] [-Activity] <String> [-ExpectedCount <Int32>] [-Id <Int32>]
 [-ParentId <Int32>] [-Status <ScriptBlock>] [-CurrentOperation <ScriptBlock>] [-RefreshInterval <TimeSpan>]
 [-DisplayThreshold <TimeSpan>] [-MinimumTimeLeftToDisplay <TimeSpan>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The Write-PipelineProgress cmdlet displays a progress bar that depicts the status of processing a pipeline of objects.

## EXAMPLES

### Example 1: Simple Progress Bar
```
1..10 | Write-PipelineProgress -Activity "Activity" | ForEach-Object {
    # Simulate a task.
    Start-Sleep -Seconds 1
}
```

### Example 2: Progress Bar with Custom Status
```
Get-ChildItem *.json -Recurse | Write-PipelineProgress -Activity "Read JSON Files" -Status { $_.Name } | ForEach-Object {
    # Simulate parsing the file.
    Start-Sleep -Seconds 1
}
```

### Example 3: Progress Bar with Expected Number of Items
```
$taskList = 1..1000000
$taskList | Write-PipelineProgress -Activity "Process Many Items" -ExpectedCount $taskList.Count | ForEach-Object {
    # Simulate a task.
    Start-Sleep -Milliseconds 10
}
```

## PARAMETERS

### -InputObject
Specifies the input objects.
Progress will be written for each of these objects.

```yaml
Type: Object[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Activity
Specifies the first line of text in the heading above the status bar.
This text describes the activity whose progress is being reported.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExpectedCount
Specifies the number of items that are expected to be processed.
Using this parameter will improve the speed and reduce the overhead of this command.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
Specifies an ID that distinguishes each progress bar from the others.
Use this parameter when you are creating more than one progress bar in a single command.
If the progress bars don't have different IDs, they're superimposed instead of being displayed in a series.
Negative values aren't allowed.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParentId
Specifies the parent activity of the current activity.
Use the value -1 if the current activity has no parent activity.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -Status
Specifies a script block expression that gets text that describes the current state of the activity, given the object being processed.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CurrentOperation
Specifies a script block expression that gets text that describes the operation that's currently taking place.
This parameter has no effect when the progress view is set to Minimal.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RefreshInterval
Specifies the interval at which progress should be returned.

```yaml
Type: TimeSpan
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 00:00:00.5000000
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayThreshold
Specifies the length of time from the first sample that progress should be returned.

```yaml
Type: TimeSpan
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 00:00:01
Accept pipeline input: False
Accept wildcard characters: False
```

### -MinimumTimeLeftToDisplay
Specifies the shortest length of time over which progress should be returned.
Set this to a longer time to avoid displaying progress moments from completion.

```yaml
Type: TimeSpan
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 00:00:02
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Object[]
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
