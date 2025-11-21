#!/bin/bash

# Define the settings file name
SETTINGS_FILE="coverlet.runsettings"

echo "1. Cleaning previous results..."
rm -rf TestResults
rm -rf CoverageReport

# --- 1. Auto-install ReportGenerator if missing ---
if ! command -v reportgenerator &> /dev/null; then
    echo "⚙️ ReportGenerator tool not found. Installing globally..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
    
    # Update PATH for the current session to include the new tool
    export PATH="$PATH:$HOME/.dotnet/tools"
else
    echo "✅ ReportGenerator is already installed."
fi

# --- 2. Create temporary runsettings file ---
echo "2. Creating temporary $SETTINGS_FILE..."
cat <<EOF > "$SETTINGS_FILE"
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <DisableParallelProcessing>true</DisableParallelProcessing>
          <Include>[IAMService.Application]*,[IAMService.API]*</Include>
          <Exclude>[*.Test]*,[*]*.Program,[*]*Program*,[*]*.Startup,[*]*ErrorDetail,[*]*ErrorResponse,[*]*ValidationBehavior*,[*.Application]*.DTOs.*,[*.Application.DTOs]*,[*]*.gRPC.Protos.*,[*]*Grpc*</Exclude>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
EOF

# --- 3. Run Tests ---
echo "3. Running Tests..."
dotnet test "IAM-Service-Backend.sln" \
  --collect:"XPlat Code Coverage" \
  --settings "$SETTINGS_FILE" \
  --results-directory ./TestResults

# --- 4. Generate Report ---
echo "4. Generating Report..."
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"CoverageReport" \
  -reporttypes:Html

# --- 5. Cleanup Settings File ---
echo "5. Removing temporary configuration..."
rm "$SETTINGS_FILE"

echo "✅ Done! Report generated at ./CoverageReport/index.html"

# --- 6. Open Report (Cross-platform compatibility) ---
if [[ "$OSTYPE" == "darwin"* ]]; then
    open ./CoverageReport/index.html     # Mac
elif [[ "$OSTYPE" == "cygwin" || "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    start ./CoverageReport/index.html    # Windows (Git Bash/WSL)
else
    xdg-open ./CoverageReport/index.html # Linux
fi