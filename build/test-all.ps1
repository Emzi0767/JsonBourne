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
# Test-all
#
# Runs all the Clyde.NET unit tests.
# 
# Author:       Emzi0767
# Version:      2019-06-08T22:08+02:00
#
# Arguments:
#   .\build\test-all.ps1 <configuration>
#
# Run as:
#   .\build\test-all.ps1 Debug/Release

param
(
    [parameter(Mandatory = $true)]
    [string] $Configuration
)

# Check if configuration is valid
if ($Configuration -ne "Debug" -and $Configuration -ne "Release")
{
    Write-Host "Invalid configuration specified. Must be Release or Debug."
    Exit 1
}

# Run tests for supplied configuration
Write-Host "Running all tests for $Configuration configuration"
& dotnet test -v d -c "$Configuration" --no-build

# Check if run was successful
if ($LastExitCode -ne 0)
{
    Write-Host "Test run failed"
    Exit $LastExitCode
}

# It was, so return success
Exit 0
