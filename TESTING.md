# Testing SpreadShare
This document describes how to run tests using the command line and using VSCode.

## Running the tests
Tests can be run using the following commands. Please note that [Travis CI](https://travis-ci.com/HugoPeters1024/SpreadShare2) will not accept any warnings while _dotnet_ does. Tests are run on every commit in [Travis CI](https://travis-ci.com/HugoPeters1024/SpreadShare2).
```
dotnet build SpreadShare.Tests/SpreadShare.Tests.csproj
dotnet test SpreadShare.Tests/SpreadShare.Tests.csproj
```

### Testing VSCode from solution 
1) Install the _.NET Core Test Explorer_ `formulahendry.dotnet-test-explorer` from the extension menu.
2) In VSCode navigate to `File > Preferences > Settings`, then select the workspace tab, click on the `...` logo and click `Open settings.json`
The content of settings.json should then contain at least the following:

```
{
    "dotnet-test-explorer.testProjectPath" : "./unit-testing/<Test Project Folder>"
}
```

If _.NET Core Test Explorer_ does not show tests, check if the test(s) compile by running `dotnet test` in the folder of the test project. Furthermore, try to open VSCode in the scope of the folder of the test project to eliminate the possibility of a faulty `settings.json`