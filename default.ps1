Framework '4.5.2'

properties {
    $name = "HSMVC"
    $birthYear = 2015
    $company = "Headspring"
    $configuration = 'Debug'
    $src = resolve-path '.\src'
    $projects = @(gci $src -rec -filter *.csproj)

    if ($env:APPVEYOR -eq "True") {
        $version = $env:APPVEYOR_BUILD_VERSION
        $dev_connection_string = $null
        $test_connection_string = $env:ConnectionString
    } else {
        $version = "1.0.32"
        $dev_connection_string = get-connection-string "$src\HSMVC\Web.config" "ConferenceDb"
        $test_connection_string = get-connection-string "$src\HSMVC.Tests\App.config" "ConferenceDb"
    }

    Write-Host "Building Version $version"

    $roundhouse_dir = "$src\packages\roundhouse.0.8.6\bin"
    $roundhouse_output_dir = "$roundhouse_dir\output"
    $roundhouse_exe_path = "$roundhouse_dir\rh.exe"
    $db_scripts_dir ="$src\HSMVC.Database\Database\HSMVC"
    $roundhouse_version_file = "$src\HSMVC\bin\HSMVC.dll"
}

task default -depends Compile, RebuildDevDatabase, RebuildTestDatabase, Test
task dev -depends Compile, UpdateDevDatabase, UpdateTestDatabase, Test
task ci -depends Compile, RebuildTestDatabase, Test

########
# Help #
########

task help {
    Write-Host ""
    Write-Help-For-Task "(default)" "Builds a clean local copy."
    Write-Help-For-Task "dev" "Optimized for local dev: updates databases instead of rebuilding."
    Write-Help-For-Task "ci" "Continuous Integration build."
    Write-Host ""
    Write-Host "For a complete list of build tasks, view build.ps1."
    Write-Host ""
    exit 0
}

function Write-Help-For-Task($task, $description) {
    Write-Host "build $task" -foregroundcolor Green -nonewline;
    Write-Host " = " -nonewline;
    Write-Host "$description"
}

####################
# Compile and Test #
####################

task Test -depends Compile {
    exec { & "$src\packages\NUnit.Console.3.0.1\tools\nunit3-console.exe" "$src\HSMVC.Tests\bin\Debug\HSMVC.Tests.dll" }
}

task Compile -depends ConnectionStrings, AssemblyInfo {
  # Example code triggers a few warnings that we can safely ignore with the NoWarn option:
  #
  #     CS0219 - The variable '...' is assigned but its value is never used
  #     CS0168 - The variable '...' is declared but never used
  #     CS0649 - Field '...' is never assigned to, and will always have its default value null
  
  exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $src\$name.sln }
  #exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\$name.sln '/p:NoWarn="219,168,649"' }
  #exec { msbuild "$src\HSMVC\HSMVC.csproj" /v:q /nologo /t:Package /p:PackageLocation="$src\deploy.zip" /p:PackageAsSingleFile=True '/p:NoWarn="219,168,649"' }
  
  if ($env:APPVEYOR_REPO_BRANCH -eq "master") {
    exec { msbuild /t:build /v:q /nolog /p:Configuration=$configuration $src\$name.sln '/p:NoWarn="219,168,649"' }
    exec { msbuild "$src\HSMVC\HSMVC.csproj" /v:q /nologo /t:Package /p:PackageLocation="$src\deploy.zip" /p:PackageAsSingleFile=True '/p:NoWarn="219,168,649"' }
    appveyor PushArtifact "$src\deploy.zip" -Type WebDeployPackage
  } else {
    exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\$name.sln /p:RunOctoPack=true /p:OctoPackPackageVersion=$version /p:OctoPackPublishPackageToFileShare="$src\packages" '/p:NoWarn="219,168,649"'}
  }
}

#######################
# Database Management #
#######################

task RebuildDevDatabase {
    deploy-database "Rebuild" $dev_connection_string "DEV"
}

task RebuildTestDatabase {
    deploy-database "Rebuild" $test_connection_string "TEST"
}

task UpdateDevDatabase {
    deploy-database "Update" $dev_connection_string "DEV"
}

task UpdateTestDatabase {
    deploy-database "Update" $test_connection_string "TEST"
}

function deploy-database($action, $connection_string, $env) {

    Write-Host "Executing RoundhousE for environment:" $env

    if ($action -eq "Update") {
        exec { & $roundhouse_exe_path --connectionstring $connection_string `
                                      --commandtimeout 300 `
                                      --env $env `
                                      --output $roundhouse_output_dir `
                                      --sqlfilesdirectory $db_scripts_dir `
                                      --versionfile $roundhouse_version_file `
                                      --transaction `
                                      --silent }
    }

    if ($action -eq "Rebuild") {
        exec { & $roundhouse_exe_path --connectionstring $connection_string `
                                      --commandtimeout 300 `
                                      --env $env `
                                      --output $roundhouse_output_dir `
                                      --silent `
                                      --drop }

        exec { & $roundhouse_exe_path --connectionstring $connection_string `
                                      --commandtimeout 300 `
                                      --env $env `
                                      --output $roundhouse_output_dir `
                                      --sqlfilesdirectory $db_scripts_dir `
                                      --versionfile $roundhouse_version_file `
                                      --transaction `
                                      --silent `
                                      --simple }
    }
}

#################
# Configuration #
#################

task ConnectionStrings {
    foreach ($configFile in @(gci $src -rec -filter App.config)) {
        set-connection-string $configFile.FullName "ConnectionString" $test_connection_string
    }
}

function try-poke-xml($filePath, $xpath, $value, $namespaces = @{}) {
    [xml] $fileXml = Get-Content $filePath

    if($namespaces -ne $null -and $namespaces.Count -gt 0) {
        $ns = New-Object Xml.XmlNamespaceManager $fileXml.NameTable
        $namespaces.GetEnumerator() | %{ $ns.AddNamespace($_.Key,$_.Value) }
        $node = $fileXml.SelectSingleNode($xpath,$ns)
    } else {
        $node = $fileXml.SelectSingleNode($xpath)
    }

    if ($node -ne $null) {
        if($node.NodeType -eq "Element") {
            if ($node.InnerText -ne $value) {
                Write-Host "Updating $filePath"
                $node.InnerText = $value
                $fileXml.Save($filePath)
            }
        } else {
            if ($node.Value -ne $value) {
                Write-Host "Updating $filePath"
                $node.Value = $value
                $fileXml.Save($filePath)
            }
        }
    }
}

function read-xml($filePath, $xpath, $namespaces = @{}) {
    [xml] $fileXml = Get-Content $filePath

    if($namespaces -ne $null -and $namespaces.Count -gt 0) {
        $ns = New-Object Xml.XmlNamespaceManager $fileXml.NameTable
        $namespaces.GetEnumerator() | %{ $ns.AddNamespace($_.Key,$_.Value) }
        $node = $fileXml.SelectSingleNode($xpath,$ns)
    } else {
        $node = $fileXml.SelectSingleNode($xpath)
    }

    if ($node -ne $null) {
        if($node.NodeType -eq "Element") {
            return $node.InnerText
        } else {
            return $node.Value
        }
    }
}

function get-connection-string($filePath, $connectionStringName) {
    return read-xml $filePath "/configuration/connectionStrings/add[@name = '$connectionStringName']/@connectionString"
}

function set-connection-string($filePath, $connectionStringName, $connectionString) {
    try-poke-xml $filePath "/configuration/connectionStrings/add[@name = '$connectionStringName']/@connectionString" $connectionString
}

##############
# Versioning #
##############

task AssemblyInfo {
    $copyright = get-copyright

    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)
        $assemblyInfoPath = "$($project.DirectoryName)\Properties\AssemblyInfo.cs"

        if (Test-Path $assemblyInfoPath)
        {
            regenerate-file $assemblyInfoPath @"
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct("$name")]
[assembly: AssemblyTitle("$projectName")]
[assembly: AssemblyVersion("$version")]
[assembly: AssemblyFileVersion("$version")]
[assembly: AssemblyCopyright("$copyright")]
[assembly: AssemblyCompany("$company")]
[assembly: AssemblyConfiguration("$configuration")]
"@
        }
    }
}

function get-copyright {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $birthYear) { $year } else { "$birthYear-$year" }
    return "Copyright © $copyrightSpan $company"
}

function regenerate-file($path, $newContent) {
    $oldContent = [IO.File]::ReadAllText($path)

    if ($newContent -ne $oldContent) {
        $relativePath = Resolve-Path -Relative $path
        write-host "Generating $relativePath"
        [System.IO.File]::WriteAllText($path, $newContent, [System.Text.Encoding]::UTF8)
    }
}