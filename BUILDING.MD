# Building JsonBourne
These are detailed instructions on how to build the JsonBourne library under various environmnets.

It is recommended you have prior experience with .NET Core/Standard projects, as well as the `dotnet` CLI utility, and 
MSBuild.

## Requirements
In order to build the library, you will first need to install some software.

### Windows
On Windows, only Visual Studio 2019 16.8 or newer is supported. Visual Studio Code and other IDEs might work, but will 
receive no support from me.

* **Windows 10** - The instructions below were only ever tested with Windows 10.
* [**Git for Windows**](https://git-scm.com/download/win) - required to clone the repository.
* [**Visual Studio 2019**](https://visualstudio.com/downloads/) - community edition or better. Visual Studio 2017 and 
  older are not supported and will not work. Note that to build the library, you need Visual Studio 2019 version 16.8 
  or newer.
   * **Workloads**:
      * **.NET Core Cross-Platform Development** - required to build .NET Core and .NET Standard projects.
* [**.NET 5 SDK**](https://www.microsoft.com/net/download) - required to build the project.
* **Windows PowerShell** - required to run the build scripts. You need to make sure your script execution policy 
  allows execution of unsigned scripts.

### GNU/Linux
On GNU/Linux, building via Visual Studio Code and .NET Core SDK is supported. Other IDEs might work, but are not 
guaranteed to.

While these should apply to any modern distribution, testing only happens against Debian 10. Your mileage may vary.

When installing the below, make sure you install all the dependencies properly. A build environmnent might be shipped 
as a Docker container in the future.

* **Any modern GNU/Linux distribution** - like Debian 10.
* **Git** - to clone the repository.
* [**Visual Studio Code**](https://code.visualstudio.com/Download) - a recent version is required.
   * **C# for Visual Studio Code (powered by OmniSharp)** - required for syntax highlighting and basic Intellisense
* [**.NET 5 SDK**](https://www.microsoft.com/net/download) - required to build the project.
* [**Mono 5.x or better**](http://www.mono-project.com/download/#download-lin) - required to build the docs.
* [**PowerShell Core**](https://docs.microsoft.com/en-us/powershell/scripting/setup/Installing-PowerShell-Core-on-macOS-and-Linux?view=powershell-6) - 
  required to execute the build scripts.
* **p7zip-full** - required to package docs.

## Instructions
Once you install all the necessary prerequisites, you can proceed to building. These instructions assume you have 
already cloned the repository.

### Windows
Building on Windows is relatively easy. There's 2 ways to build the project:

#### Building through Visual Studio
Building through Visual Studio yields just binaries you can use in your projects.

1. Open the solution in Visual Studio.
2. Set the configuration to Release.
3. Select Build > Build Solution to build the project.
4. Select JsonBourne project.
4. Select Build > Pack JsonBourne to create NuGet packages.

#### Building with one-click build script
Building this way always outputs NuGet packages, and a documentation package. Ensure you have an internet connection 
available, as the script will install programs necessary to build the documentation.

#### Building with the build script
Building this way outputs NuGet packages. It can also generate a documentation package. When choosing to do so, ensure 
you have an internet connection available, as the script will install programs necessary to build the documentation.

1. Open PowerShell and navigate to the directory which you cloned JsonBourne to.
2. Execute `.\build\rebuild-all.ps1 .\artifacts Release` and wait for the script to finish execution.
3. Once it's done, the artifacts will be available in *artifacts* directory, located in your working directory. 

If you want to build documentation packages as well, you need to run the `rebuild-all.ps1` script like so:

`.\build\rebuild-all.ps1 .\artifacts Release -DocsPath .\docs -DocsPackageName jsonbourne-docs`

Doing so will generate a file called *jsonbourne-docs.tar.xz* in addition to NuGet packages.

### GNU/Linux
When all necessary prerequisites are installed, you can proceed to building. There are technically 2 ways to build the 
library, though both of them perform the same steps, they are just invoked slightly differently.

#### Through Visual Studio Code
1. Open Visual Studio Code and open the folder to which you cloned JsonBourne as your workspace.
2. Select Build > Run Task...
3. Select `buildRelease` task and wait for it to finish.
4. The artifacts will be placed in *artifacts* directory, located in the same place JsonBourne is.

#### Through PowerShell
1. Open PowerShell (`pwsh`) and navigate to the directory which you cloned JsonBourne to.
2. Execute `.\build\rebuild-all.ps1 -configuration Release` and wait for the script to finish execution.
3. Once it's done, the artifacts will be available in *artifacts* directory, located in the same place JsonBourne is.
