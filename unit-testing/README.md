# Unit Testing Research
This folder contains a basic unit test setup using the tutorial at https://docs.microsoft.com/nl-nl/dotnet/core/testing/unit-testing-with-dotnet-test

# Running the tests
To run the test simply run `dotnet test` in this folder.

# Running the tests from VSCode
install the .Net Core Test Explorer `formulahendry.dotnet-test-explorer` from the extension menu.

In VSCode navigate to `File > Preferences > Settings`, then select the workspace tab, click on the `...` logo and click `Open settings.json`

The content of settings.json should then contain at least the following:

```
{
    "dotnet-test-explorer.testProjectPath" : "./unit-testing/<Test Project Folder>"
}
```

###### *This only applies if you are opening VSCode in the scope of upper project folder (SpreadShare2)

# Troubleshooting
## .Net Core Test Explorer does not show tests
* Check if the test(s) compile by running `dotnet test` in the folder of the test project.
* Open VSCode in the scope of the folder of the test project to eliminate the possibility of a faulty settings.json
