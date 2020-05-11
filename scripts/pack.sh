#!/bin/bash
echo Executing after success scripts on branch $TRAVIS_BRANCH
echo Triggering NuGet package build

cd src
dotnet pack /p:PackageVersion=1.0.$TRAVIS_BUILD_NUMBER --no-restore -o .

echo Uploading Genocs.Core package to NuGet using branch $TRAVIS_BRANCH

case "$TRAVIS_BRANCH" in
  "master")
    dotnet nuget push bin\Release\*.nupkg -k $NUGET_API_KEY -s $NUGET_SOURCE
    ;;  
esac

