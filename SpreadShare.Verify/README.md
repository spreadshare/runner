# Description
A simple command line tool to verify configurations of the main applications within a shell.

# Installation

If a previous version is already installed.
```
dotnet tool uninstall -g spreadshare.verify
```

Build and install
```
cd SpreadShare.Verify
dotnet pack
dotnet tool install -g --add-source ./bin/Debug/ SpreadShare.Verify
```

From this point on the command `dotnet verify <filename>` is available on the entire machine.

