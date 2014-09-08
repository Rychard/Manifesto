SET NUGET=..\..\..\..\Nuget\NuGet.exe

REM Symbol Package Disabled
REM %NUGET% pack -Symbols ..\..\..\..\Manifesto\Manifesto.csproj
REM SET PACKAGEPATH=%CD%\Manifesto.%1.symbols.nupkg
REM ECHO Pushing Symbol Package: %PACKAGEPATH%
REM %NUGET% push %PACKAGEPATH%

%NUGET% pack ..\..\..\..\Manifesto\Manifesto.csproj
SET PACKAGEPATH=%CD%\Manifesto.%1.nupkg
ECHO Pushing Package: %PACKAGEPATH%
%NUGET% push %PACKAGEPATH%


