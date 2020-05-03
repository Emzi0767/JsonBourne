#!/usr/bin/env pwsh
#
# -----------------------------------------------------------------------------
#
# This file is a part of Clyde.NET project.
# 
# Copyright 2019-2020 Emzi0767
# 
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# 
#   http://www.apache.org/licenses/LICENSE-2.0
#   
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
# -----------------------------------------------------------------------------
#
# Rebuild-lib
#
# Rebuilds the entire Clyde.NET project, and places artifacts in specified directory.
# 
# Author:       Emzi0767
# Version:      2019-01-21T17:39+02:00
#
# Arguments:
#   .\build\rebuild-lib.ps1 <output path> <configuration> [version suffix] [build number]
#
# Run as:
#   .\build\rebuild-lib.ps1 .\path\to\artifact\location Debug/Release version-suffix build-number
#
# or
#   .\build\rebuild-lib.ps1 .\path\to\artifact\location Debug/Release

param
(
    [parameter(Mandatory = $true)]
    [string] $ArtifactLocation,

    [parameter(Mandatory = $true)]
    [string] $Configuration,

    [parameter(Mandatory = $false)]
    [string] $VersionSuffix,

    [parameter(Mandatory = $false)]
    [int] $BuildNumber = -1
)

# Check if configuration is valid
if ($Configuration -ne "Debug" -and $Configuration -ne "Release")
{
    Write-Host "Invalid configuration specified. Must be Release or Debug."
    Exit 1
}

# Restores the environment
function Restore-Environment()
{
    Write-Host "Restoring environment"
    if (Test-Path ./NuGet.config)
    {
        Remove-Item ./NuGet.config
    }
}

# Prepares the environment
function Prepare-Environment([string] $target_dir_path)
{
    # Prepare the environment
    if (Test-Path ./build/NuGet.config)
    {
        Write-Host "Detected NuGet configuration"
        Copy-Item ./build/NuGet.config ./
    }
    else
    {
        Write-Host "No NuGet configuration was detected"
    }
    
    # Check if the target directory exists
    # If it does, remove it
    if (Test-Path "$target_dir_path")
    {
        Write-Host "Target directory exists, deleting"
        Remove-Item -recurse -force "$target_dir_path"
    }
    
    # Create target directory
    $dir = New-Item -type directory "$target_dir_path"
}

# Builds everything
function Build-All([string] $target_dir_path, [string] $version_suffix, [string] $build_number, [string] $bcfg)
{
    # Form target path
    $dir = Get-Item "$target_dir_path"
    $target_dir = $dir.FullName
    Write-Host "Will place packages in $target_dir"
    
    # Clean previous build results
    Write-Host "Cleaning previous build"
    & dotnet clean -v minimal -c "$bcfg" | Out-Host
    if ($LastExitCode -ne 0)
    {
        Write-Host "Cleanup failed"
        Return $LastExitCode
    }
    
    # Restore nuget packages
    Write-Host "Restoring NuGet packages"
    & dotnet restore -v minimal | Out-Host
    if ($LastExitCode -ne 0)
    {
        Write-Host "Restoring packages failed"
        Return $LastExitCode
    }

    # Create build number string
    if (-not $build_number)
    {
        $build_number_string = ""
    }
    else
    {
        $build_number_string = [int]::Parse($build_number).ToString("00000")
    }
    
    if ($Env:OS -eq $null)
    {
        # Build and package (if Release) in specified configuration but Linux
        Write-Host "Building everything"
        if (-not $version_suffix -or -not $build_number_string)
        {
            & .\build\rebuild-linux.ps1 "$target_dir" -Configuration "$bcfg" | Out-Host
        }
        else
        {
            & .\build\rebuild-linux.ps1 "$target_dir" -Configuration "$bcfg" -VersionSuffix "$version_suffix" -BuildNumber "$build_number_string" | Out-Host
        }
        if ($LastExitCode -ne 0)
        {
            Write-Host "Packaging failed"
            Return $LastExitCode
        }
    }
    else 
    {
        # Build in specified configuration
        Write-Host "Building everything"
        if (-not $version_suffix -or -not $build_number_string)
        {
            & dotnet build -v minimal -c "$bcfg" | Out-Host
        }
        else
        {
            & dotnet build -v minimal -c "$bcfg" --version-suffix "$version_suffix" -p:BuildNumber="$build_number_string" | Out-Host
        }
        if ($LastExitCode -ne 0)
        {
            Write-Host "Build failed"
            Return $LastExitCode
        }
        
        # Package for NuGet
        Write-Host "Creating NuGet packages"
        if (-not $version_suffix -or -not $build_number_string)
        {
            & dotnet pack -v minimal -c "$bcfg" --no-build -o "$target_dir" --include-symbols | Out-Host
        }
        else
        {
            & dotnet pack -v minimal -c "$bcfg" --version-suffix "$version_suffix" -p:BuildNumber="$build_number_string" --no-build -o "$target_dir" --include-symbols | Out-Host
        }
        if ($LastExitCode -ne 0)
        {
            Write-Host "Packaging failed"
            Return $LastExitCode
        }
    }
    
    Return 0
}

# Check if building a nightly package
if ($VersionSuffix -and $BuildNumber -and $BuildNumber -ne -1)
{
    Write-Host "Building nightly package with version suffix of `"$VersionSuffix-$($BuildNumber.ToString("00000"))`""
}

# Prepare environment
Prepare-Environment "$ArtifactLocation"

# Build everything
$BuildResult = Build-All "$ArtifactLocation" "$VersionSuffix" "$BuildNumber" "$Configuration"

# Restore environment
Restore-Environment

# Check if there were any errors
if ($BuildResult -ne 0)
{
    Write-Host "Build failed with code $BuildResult"
    $host.SetShouldExit($BuildResult)
    Exit $BuildResult
}
else
{
    Exit 0
}
