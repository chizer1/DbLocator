#!/bin/bash

# Build the project to generate XML documentation
dotnet build ../DbLocator/DbLocator.csproj

# Install DocFX if not already installed
if ! command -v docfx &> /dev/null; then
    dotnet tool install -g docfx
fi

# Generate documentation
docfx init -q
docfx build

echo "Documentation has been generated in the _site directory" 