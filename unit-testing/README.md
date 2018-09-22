# Unit Testing Research
This folder contains a basic unit test setup using the tutorial at https://docs.microsoft.com/nl-nl/dotnet/core/testing/unit-testing-with-dotnet-test

# Running the tests
To run the test simply run `dotnet test` in this folder.

# Running the tests from VSCode
install the .Net Core Test Explorer `formulahendry.dotnet-test-explorer` from the extension menu.

In VSCode navigate to `File > Preferences > Settings`, then select the workspace tab, click on the `...` logo and click `Open settings.json`

The content of settings.json should then contain the following:

```
{
    "dotnet-test-explorer.testProjectPath" : "./unit-testing/PrimeService.Tests"
}
```

###### *This only applies if you are opening VSCode in the scope of upper project folder (SpreadShare2)
