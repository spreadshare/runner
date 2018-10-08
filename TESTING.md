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

### Adding More Test Projects in VSCode
1) Navigate to *unit-tests/*
2) Run `mkdir <New Test Project>`
3) Navigate to *New Test Project*
4) Run `dot new xunit`
5) Run `dotnet add reference ../../SpreadShare/SpreadShare.csproj`
6) Navigate back to *unit-tests/*
7) Run `dotnet sln add ./<New Test Project>/<New Test Project>.csproj` 
8) Remove *UnitTest1.cs*
 Make a new *.cs* file using the example below.

```csharp
using Xunit;

namespace UnitTests
{
	public class Test
  {
		//You are encouraged you to add objects from
		//the SpreadShare.* namespace as private members 
		//and initialize them in the constructor.
		
		public Test() { }

		[Fact]
		public void SuccessGuaranteed()
		{
			bool AreWeRichYet = true;
			//Test passes if the first argument is true, 
			//if not it will echo the second argument to the test logs.
			Assert.True(AreWeRichYet, "Dude! You're poor!");
		}
  }
}
```
