#!/usr/bin/env bash
dotnet restore ../SpreadShare.sln
dotnet build ../SpreadShare.sln

# Instrument assemblies inside 'test' folder to detect hits for source files inside 'src' folder
dotnet minicover instrument --workdir ../SpreadShare #--assemblies /bin/Debug/netcoreapp2.1/SpreadShare.dll --sources /**/*.cs

# Reset hits count in case minicover was run for this project
dotnet minicover reset --workdir ../SpreadShare


# Run the actual test suite
if [ -z $1 ]; then
  dotnet test ../SpreadShare.Tests/
else
  dotnet test --filter $1 ../SpreadShare.Tests/
fi

# Uninstrument assemblies, it's important if you're going to publish or deploy build outputs
dotnet minicover uninstrument --workdir ../SpreadShare

# Create html reports inside folder coverage-html
dotnet minicover htmlreport --workdir ../SpreadShare --threshold 50

# Print console report
# This command returns failure if the coverage is lower than the threshold
# dotnet minicover report --workdir ../SpreadShare --threshold 90 

# Open html report in browser
python -mwebbrowser ../SpreadShare/coverage-html/index.html > /dev/null
