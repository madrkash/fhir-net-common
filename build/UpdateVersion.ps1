﻿#set the next two variables or get them from an Invoke-Expression, the rest should work automatically.
Param(
  [string]$newVersion,
  [string]$suffix
)

#Go to src (version is not relevant to test projects)
Push-Location $PsScriptRoot\..\src

$xml = [xml](get-content fhir-net-common.props) 

# newVersion is not set, so getting it from the fhir-net-api.props
If ([string]::IsNullOrEmpty($newVersion)) 
{
	#Get the version (without suffix)
	$newVersion = $xml.Project.PropertyGroup.VersionPrefix
}

#Get the existing suffix
[string] $oldSuffix = $xml.Project.PropertyGroup.VersionSuffix
$oldSuffix = $oldSuffix.Trim()

# when the suffix is not alpha (probably beta), it cannot be overriden by the parameter 
if (!$oldSuffix.StartsWith("alpha") -or [string]::IsNullOrEmpty($oldSuffix))
{
	$suffix = $oldSuffix
}

Write-Host "Replacing version information with version: [$newVersion] suffix: [$suffix]" 

#Replacing the version and suffix
(Get-Content fhir-net-common.props) |
    Foreach-Object { $_ `
        -replace "<VersionPrefix>.*</VersionPrefix>", "<VersionPrefix>$newVersion</VersionPrefix>" `
        -replace "<VersionSuffix>.*</VersionSuffix>", "<VersionSuffix>$suffix</VersionSuffix>" `
    } |
    Set-Content fhir-net-common.props

#go back to the original directory
Pop-Location
