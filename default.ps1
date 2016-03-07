Framework "4.5.2"

$projectName = "HSMVC"
$configuration = 'Debug'
$basedir = resolve-path '.\'
$src = "$basedir\src"
$sln = "$src\HSMVC.sln"

$test_results = "$basedir\TestResults"

$database_instance = ".\sqlexpress"
$database_name = "Conference"
$scripts_path = "$src\HSMVC.Database\Database\HSMVC"

$rh_path = "$src\packages\roundhouse.0.8.6\bin"
$rh_exe = "$rh_path\rh.exe"
$rh_output_path = "$rh_path\output"
$rh_cmd_timeout = 600
$rh_version_file = "$src\HSMVC\bin\HSMVC.dll"

task default -depends RebuildDatabase, Test

task CommonAssemblyInfo {
    $version = (Get-Date).ToString("yyyy.MM.dd.HHmm")
    create-commonAssemblyInfo "$version" $projectName "$src\CommonAssemblyInfo.cs"
	$packageVersion = $version
}

task Compile -depends CommonAssemblyInfo {
    exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $sln }
    exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $sln }
}

task Test -depends RebuildDatabase {
	exec { & "$src\packages\NUnit.Console.3.0.1\tools\nunit3-console.exe" "$src\HSMVC.Tests\bin\Debug\HSMVC.Tests.dll" }
}

task RebuildDatabase -depends Compile {    
    exec { &$rh_Exe -s $database_instance -d $database_name -vf $rh_version_file --silent -drop -o $rh_output_path --ct $rh_cmd_timeout }
    exec { &$rh_Exe -s $database_instance -d $database_name -f $scripts_path -vf $rh_version_file --simple --silent -o $rh_output_path --ct $rh_cmd_timeout }
}

function global:create-commonAssemblyInfo($version,$applicationName,$filename) {
  write-host ("Version: " + $version) -ForegroundColor Green
  write-host ("Project Name: " + $projectName) -ForegroundColor Green

"using System;
using System.Reflection;
using System.Runtime.InteropServices;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behaviour and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: ComVisibleAttribute(false)]
[assembly: AssemblyVersionAttribute(""$version"")]
[assembly: AssemblyFileVersionAttribute(""$version"")]
[assembly: AssemblyCopyrightAttribute(""Copyright 2015"")]
[assembly: AssemblyProductAttribute(""$projectName"")]
[assembly: AssemblyCompanyAttribute("""")]
[assembly: AssemblyConfigurationAttribute(""release"")]
[assembly: AssemblyInformationalVersionAttribute(""$version"")]"  | out-file $filename -encoding "ASCII"    
}