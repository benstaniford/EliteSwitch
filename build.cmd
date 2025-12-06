@echo off
REM Build script for Elite Switch application and installer

echo ================================================
echo Building Elite Switch
echo ================================================

REM Check if icon exists
if not exist "icon.ico" (
    echo WARNING: icon.ico not found!
    echo The build may fail. Please create an icon file.
    echo See ICON_NOTE.txt for instructions.
    pause
)

echo.
echo Step 1: Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore packages
    exit /b %ERRORLEVEL%
)

echo.
echo Step 2: Building EliteSwitch application (Release)...
dotnet build EliteSwitch.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build application
    exit /b %ERRORLEVEL%
)

echo.
echo Step 3: Checking for WiX toolset...
dotnet wix --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo WiX toolset not found. Installing...
    dotnet tool install --global wix
    if %ERRORLEVEL% NEQ 0 (
        echo ERROR: Failed to install WiX toolset
        exit /b %ERRORLEVEL%
    )
)

echo.
echo Step 4: Building MSI installer...
dotnet build EliteSwitch.Installer\EliteSwitch.Installer.wixproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build installer
    exit /b %ERRORLEVEL%
)

echo.
echo ================================================
echo Build completed successfully!
echo ================================================
echo.
echo Application: bin\Release\net8.0-windows\EliteSwitch.exe
echo Installer:   EliteSwitch.Installer\bin\Release\x64\en-US\EliteSwitchSetup.msi
echo.
pause
