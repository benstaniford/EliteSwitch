#!/bin/bash
# Build script for Elite Switch application and installer

echo "================================================"
echo "Building Elite Switch"
echo "================================================"

# Check if icon exists
if [ ! -f "icon.ico" ]; then
    echo "WARNING: icon.ico not found!"
    echo "The build may fail. Please create an icon file."
    echo "See ICON_NOTE.txt for instructions."
    read -p "Press Enter to continue anyway..."
fi

echo ""
echo "Step 1: Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to restore packages"
    exit 1
fi

echo ""
echo "Step 2: Building EliteSwitch application (Release)..."
dotnet build EliteSwitch.csproj -c Release
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build application"
    exit 1
fi

echo ""
echo "Step 3: Checking for WiX toolset..."
if ! dotnet wix --version &> /dev/null; then
    echo "WiX toolset not found. Installing..."
    dotnet tool install --global wix
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to install WiX toolset"
        exit 1
    fi
fi

echo ""
echo "Step 4: Building MSI installer..."
dotnet build EliteSwitch.Installer/EliteSwitch.Installer.wixproj -c Release
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build installer"
    exit 1
fi

echo ""
echo "================================================"
echo "Build completed successfully!"
echo "================================================"
echo ""
echo "Application: bin/Release/net8.0-windows/EliteSwitch.exe"
echo "Installer:   EliteSwitch.Installer/bin/Release/x64/en-US/EliteSwitchSetup.msi"
echo ""
