#!/bin/bash
MYGET_ENV=""
case "$TRAVIS_BRANCH" in
  "master")
    MYGET_ENV="-dev"
    ;;
esac

dotnet build -c release