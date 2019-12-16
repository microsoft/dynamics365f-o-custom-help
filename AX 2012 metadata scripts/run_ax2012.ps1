param(
    [string]$SourcePath = $( Read-Host "Please specify source path with AX 2012 HTML files" ),
    [string]$OutPath = $( Read-Host "Please specify destination path for processed AX 2012 HTML files" )
 )

if(-not(Test-Path $SourcePath))
{
    Write-Error -Message ("Error: The source path doesn't exist: " + $SourcePath) -Category ResourceUnavailable -CategoryReason "Specified path could not be found"
    return
}

if(-not(Test-Path $OutPath))
{
    Write-Host "Creating directory" $OutPath
    New-Item -ItemType Directory -Force -Path $OutPath
}

function Process-Meta([string]$SourcePath, [string]$OutPath)
{
    $Metas = @('<meta name="ms.search.region" content="Global" />',
        '<meta name="ms.search.scope" content="Operations, Core" />',
        '<meta name="ms.dyn365.ops.version" content="AX 7.0.0" />',
        '<meta name="ms.search.validFrom" content="2016-05-31" />',
        '<meta name="ms.search.industry" content="cross" />')

    $PatternMetaWithSpaces = '\s*?<meta name="Microsoft.Help.F1".*?\/>'
    $PatternMeta = '<meta name="Microsoft.Help.F1".*?\/>'
    $PatternName = 'name=["|''](.*?)["|'']'
    $PatternContent = 'content=["|''](.*?)["|'']'
    $PatternGuid = '(^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12})?;?(.*)'
    $PatternEnd1 = '(\.+)(?=[^.]*$)(.*[^:;\s])(?:[:;\s$])'
    $PatternEnd2 = '(\.+)(?=[^.]*$)(.*[^:;\s])(?:[:;\s$])?'
    $PatternHeadEnd = '\s*?<\/head>'

    $FilesProcessed = 0
    $FilesFailed = 0

    $Files = Get-ChildItem $SourcePath -Filter *.htm
    $Counter = 0
    Write-Host "Processing" $Files.Length "files. Please wait..."
    foreach ($File in $Files)
    {
        Write-Progress -Activity "Metadata processing" -Status "Current file" -CurrentOperation $File.FullName -PercentComplete (100 * $Counter/$Files.Length)
        $Content = [System.IO.File]::ReadAllText($File.FullName)
        if ($Content -match $PatternMetaWithSpaces)
        {
            $MetaString = $Matches[0]
        
            if ($Content -match $PatternHeadEnd)
            {
                $MetaString = $MetaString -replace "`n|`r"
                $SpacesCount = $MetaString.IndexOf("<")
                $SpacesForMetas = ""
                for ($i = 1; $i -le $SpacesCount; $i++)
                {
                    $SpacesForMetas = $SpacesForMetas + " "
                }
        
                #Write-Host "META:" $MetaString
                if ($MetaString -match $PatternContent)
                {
                    $AttrContent = $Matches[1]
                    #Write-Host "ATTR.CONTENT:" $AttrContent
                    $newAttrContent = ""
                    if ($AttrContent -match $PatternGuid)
                    {
                        $newAttrContent = $Matches[1]
                        #Write-Host "GUID:" $newAttrContent
                    }
            
                    $AfterGuid = $Matches[2]
            
                    if ($AfterGuid -match $PatternEnd1)
                    {
                        $AfterGuid = $Matches[2]
                    }
                    elseif ($AfterGuid -match $PatternEnd2)
                    {
                        $AfterGuid = $Matches[2]
                    }
            
                    #Write-Host "AFTER GUID:" $AfterGuid
            
                    if ($AfterGuid -is [string] -AND $AfterGuid.Trim() -ne "")
                    {
                        $newAttrContent = $newAttrContent + "; " + $AfterGuid;
                    }
                    #Write-Host "NEW ATTR. CONTENT:" $newAttrContent
            
                    $MetaString = ($MetaString.Trim() -replace $PatternContent, ('content="' + $newAttrContent + '"'))
                    $MetaString = ($MetaString -replace $PatternName, 'name="ms.search.form"')
                    $Content = $Content -replace $PatternMeta, $MetaString
            
                    if ($Content -match $PatternHeadEnd)
                    {
                        $HeadEndString = $Matches[0]
                
                        $A = $Metas -join ("`n" + $SpacesForMetas)
                        $Content = $Content -replace $PatternHeadEnd, ("`n" + $SpacesForMetas + $A + $HeadEndString)
                    }

                    $Content = $Content -replace '<meta name="Title"', '<meta name="title"'
                    
                    
                    $Content | Out-File -FilePath (Join-Path -Path $OutPath -ChildPath $File.Name)
                    $FilesProcessed++
                }
                else
                {
                    Write-Error -Message "Error: Attribute CONTENT doesn't exist" -Category InvalidData
                    $FilesFailed++
                }
            }
            else
            {
                Write-Error -Message "Error: </HEAD> doesn't exist" -Category InvalidData
                $FilesFailed++
            }
        }
        else
        {
            Write-Error -Message "Error: META doesn't exist" -Category InvalidData
            $FilesFailed++
        }
        $Counter++
    }

    Write-Host "Files processed:" $FilesProcessed
    if ($FilesFailed -gt 0)
    {
        Write-Host "Files failed:" $FilesFailed
    }
}

function Process-Htm([string]$OutPath)
{
    Write-Host "Processing HTM links and file extensions"
    $Files = Get-ChildItem $OutPath | Where-Object {$_.Name.EndsWith(".htm")}
    $Counter = 0
    foreach ($File in $Files)
    {
        Write-Progress -Activity "Replace htm with html" -Status "Current file" -PercentComplete (100 * $Counter/$Files.Length) -CurrentOperation $File.FullName
        $updated = $false
        $Content = [System.IO.File]::ReadAllText($File.FullName)
        $Captured = $Content | Select-String -Pattern 'href\s*=\s*"(.+\.htm)"' -AllMatches
        for ($i = 0; $i -lt $Captured.Matches.Count; $i++)
        {
            $Uri = $null
            $Href =  $Captured.Matches[$i].Groups[1].Value
            if (-not ([System.Uri]::TryCreate($Href, [System.UriKind]::Absolute, [ref] $Uri)))
            {
                $UpdatedHref = $Href + "l"# htm -> html
                $Content = $Content -replace $Href,$UpdatedHref
                $updated = $true   
            }
        }
        if ($Updated)
        {
            $Content | Out-File -FilePath $File.FullName
            Write-Host ("Replaced HTM links in file: '{0}'" -f $File.FullName)
        }

        $Html = $File.FullName + "l"
        Move-Item -Path $File.FullName -Destination $Html -Force
        $Counter++
    }
    Write-Host "Processing complete"
}

Process-Meta -SourcePath $SourcePath -OutPath $OutPath
Process-Htm -OutPath $OutPath