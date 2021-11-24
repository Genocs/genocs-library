#!/bin/bash
echo Executing after success scripts on branch $TRAVIS_BRANCH
echo Triggering NuGet package build

dotnet pack -p:NuspecFile=Genocs.Core.nuspec --no-restore -o .

echo Uploading Genocs.Core package to NuGet using branch $TRAVIS_BRANCH

case "$TRAVIS_BRANCH" in
  "master")
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s $NUGET_SOURCE
    ;;  
esac
