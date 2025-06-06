#!/bin/bash

# Ensure we're in the docs directory
cd "$(dirname "$0")"

# Build the project to generate XML documentation
dotnet build ../DbLocator/DbLocator.csproj

# Install DocFX if not already installed
if ! command -v docfx &> /dev/null; then
    dotnet tool install -g docfx
fi

# Clean previous build
rm -rf _site
rm -rf api

# Generate documentation
docfx init
docfx metadata
docfx build

# Verify the _site directory exists
if [ ! -d "_site" ]; then
    echo "Error: _site directory was not created"
    exit 1
fi

echo "Documentation has been generated in the _site directory" 