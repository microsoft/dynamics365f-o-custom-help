param (
    [string]$sourcePath = $( Read-Host "Please specify source path with AX 2012 HTML files" ),
	[string]$outPath = $( Read-Host "Please specify destination path for processed AX 2012 HTML files" )
 )

If(!(test-path $sourcePath))
{
	Write-Error -Message ("Error: The source path doesn't exist: " + $sourcePath) -Category ResourceUnavailable -CategoryReason "Specified path could not be found"
	return
}

If(!(test-path $outPath))
{
	Write-Host "Creating directory" $outPath
	New-Item -ItemType Directory -Force -Path $outPath
}

$metas = @('<meta name="ms.search.region" content="Global" />',
    '<meta name="ms.search.scope" content="Operations, Core" />',
    '<meta name="ms.dyn365.ops.version" content="AX 7.0.0" />',
    '<meta name="ms.search.validFrom" content="2016-05-31" />',
    '<meta name="ms.search.industry" content="cross" />')

$patternMetaWithSpaces = '\s*?<meta name="Microsoft.Help.F1".*?\/>'
$patternMeta = '<meta name="Microsoft.Help.F1".*?\/>'
$patternName = 'name=["|''](.*?)["|'']'
$patternContent = 'content=["|''](.*?)["|'']'
$patternGuid = '(^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12})?;?(.*)'
$patternEnd1 = '(\.+)(?=[^.]*$)(.*[^:;\s])(?:[:;\s$])'
$patternEnd2 = '(\.+)(?=[^.]*$)(.*[^:;\s])(?:[:;\s$])?'
$patternHeadEnd = '\s*?<\/head>'

$filesProcessed = 0
$filesFailed = 0

$files = Get-ChildItem $sourcePath -Filter *.htm
Write-Host "Processing" $files.Length "files. Please wait..."
	foreach ($file in $files)
	{
		$sr = New-Object System.IO.StreamReader ($file.FullName)
		$content = $sr.ReadToEnd()
		$sr.Close()

		If ($content -match $patternMetaWithSpaces)
		{
			$metaString = $Matches[0]
		
			If ($content -match $patternHeadEnd)
			{
				$metaString = $metaString -replace "`n|`r"
				$spacesCount = $metaString.IndexOf("<")
				$spacesForMetas = ""
				for ($i = 1; $i -le $spacesCount; $i++)
				{
					$spacesForMetas = $spacesForMetas + " "
				}
		
				#Write-Host "META:" $metaString
				If ($metaString -match $patternContent)
				{
					$attrContent = $Matches[1]
					#Write-Host "ATTR.CONTENT:" $attrContent
					$newAttrContent = ""
					If ($attrContent -match $patternGuid)
					{
						$newAttrContent = $Matches[1]
						#Write-Host "GUID:" $newAttrContent
					}
			
					$afterGuid = $Matches[2]
			
					If ($afterGuid -match $patternEnd1)
					{
						$afterGuid = $Matches[2]
					}
					ElseIf ($afterGuid -match $patternEnd2)
					{
						$afterGuid = $Matches[2]
					}
			
					#Write-Host "AFTER GUID:" $afterGuid
			
					If ($afterGuid -is [string] -AND $afterGuid.Trim() -ne "")
					{
						$newAttrContent = $newAttrContent + "; " + $afterGuid;
					}
					#Write-Host "NEW ATTR. CONTENT:" $newAttrContent
			
					$metaString = ($metaString.Trim() -replace $patternContent, ('content="' + $newAttrContent + '"'))
					$metaString = ($metaString -replace $patternName, 'name="ms.search.form"')
					$content = $content -replace $patternMeta, $metaString
			
					If ($content -match $patternHeadEnd)
					{
						$headEndString = $Matches[0]
				
						$a = $metas -join ("`n" + $spacesForMetas)
						$content = $content -replace $patternHeadEnd, ("`n" + $spacesForMetas + $a + $headEndString)
					}

					$sw = New-Object System.IO.StreamWriter ($outPath + "\" + $file.Name)
					$sw.WriteLine($content)
					$sw.Close()
					$filesProcessed++
					#break
				}
				Else
				{
					Write-Error -Message "Error: Attribute CONTENT doesn't exist" -Category InvalidData
					$filesFailed++
				}
			}
			Else
			{
				Write-Error -Message "Error: </HEAD> doesn't exist" -Category InvalidData
				$filesFailed++
			}
		}
		Else
		{
			Write-Error -Message "Error: META doesn't exist" -Category InvalidData
			$filesFailed++
		}
	}

	Write-Host "Files processed:" $filesProcessed
	If ($filesFailed -gt 0)
	{
		Write-Host "Files failed:" $filesFailed
	}