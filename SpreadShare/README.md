# Runner
Run algorithms for backtesting and on Binance.

## Setup and run
#### Setup Docker
1. Install [docker](https://docs.docker.com/install/linux/docker-ce/ubuntu/#install-using-the-repository)
1. Install docker-compose
    ```
    sudo apt-get install docker-compose
    ```
1. Create an `.env` file from the `.env.example` file. Adjust the `.env` configuration if needed.
1. Create an `appsettings.yaml` file from the `appsettings.yaml.example` file in the folder `SpreadShare`.
    ```
    cp SpreadShare/.appsettings.yaml.example SpreadShare/.appsettings
    ```
1. Open the `appsettings.yaml` configuration in an editor and adjust the configuration.

#### Run for backtest
1. Initially the database does not contain any historic backtesting candles. Refer to the [DataGathering](https://gitlab.hp1024.nl/spreadshare/datagathering) project in order to import data into the database.
1. Set preferred parameters in `appsettings.yaml`
1. Run with
    ```
    docker-compose up --build
    ```

#### Setup Not Docker
1. Install [dotnet 2.2](https://dotnet.microsoft.com/download/linux-package-manager/rhel/sdk-2.2.203)
1. Create an `appsettings.yaml` file from the `appsettings.yaml.example` file in the folder `SpreadShare`.
    ```
    cp SpreadShare/appsettings.yaml.example SpreadShare/appsettings.yaml
    ```
1. Adjust the postgresql [connection string](https://www.npgsql.org/doc/connection-string-parameters.html):
    ```
    LocalConnection: Server=[server];Database=[database];User ID=[user];Password=[password];Pooling=true;Use SSL Stream=true;SSL Mode=Require;Trust Server Certificate=true;
    ```


#### Run for trading
1. Set preferred parameters in `appsettings.yaml`
1. Ensure that the algorithm configurations are present in `./SpreadShare/`
1. Set trading flag in `./SpreadShare/Dockerfile`
    ```
    ENTRYPOINT ["dotnet", "SpreadShare.dll", "--trading"]
    ```
1. Run with
    ```
    docker-compose up --build
    ```
___
## Development
If you want to run this application without Docker, the framework [.NET Core 2.2](https://www.microsoft.com/net/download/dotnet-core/2.2) is required. Furthermore, please read the [testing documentation](TESTING.md).

#### Database migrations
Each time the database schema changes, the following command needs to be ran.
```
dotnet ef migrations add [MigrationTitle]
```

#### Database clean
```
docker container prune
```


#### External libraries used
| **Nuget library** 	| **Purpose**                    	|
|--------------------	|--------------------------------	|
| Binance.Net        	| Communication with Binance     	|
| CommandLineParser  	| Parsing command line arguments 	|
| ConsoleTables      	| /help command in TTY           	|
| Dawn.Guard         	| Method argument checking       	|
| Newtonsoft.Json    	| Render objects as json           	|
| Roslynator         	| Code style enforcement         	|
| Sentry             	| Error reporting                	|
| StyleCop           	| Code style enforcement         	|
| xunit              	| Testing framework              	|
| YamlDotNet         	| Configuration deserialization  	|

#### Code coverage report
To generate a html code coverage report using minicover, you can run the `generate-report.sh` script from within the `SpreadShare.Tests` folder. You can enter the exact name of a test class as filter as an optional first argument. When it has finished it will open `SpreadShare/coverage-html/index.html` in the default browser using python.
```
/SpreadShare.Tests/generate-report.sh
```

___
## Deployment
On the deployment server, the application is started as docker container per algorithm. This process works as follows:

1. Adjust the dockerfile to have no arguments on the entry point
2. Run `docker build -t main . ` to build the base image
3. Adjust `SpreadShare/appsettings.yaml` as required
4. Run ```
    docker run
    -d
    --network spreadshare_network
    --mount type=bind,source="$(pwd)"/SpreadShare/appsettings.yaml,target=/app/appsettings.yaml 
    main --trading --verbose```

You can repeat step 3 and 4 as many times as you want to deploy more algorithms. Do note that it takes some time for the
application to consume the yaml configuration. Either make sure you leave enough interval (few seconds should do) or create different
configuration files and bind them with appsettings.yaml as alias.

___
## Utilities
#### Removing trailing whitespace in a pre-commit hook
Run the following commands to remove trailing whitespace on each commit:
```bash
echo "find . -name '*.cs' -exec sed -i 's/\s*$//g' {} +" >> .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit
```

**In JetBrain Products:** 
```text
Settings > Editor > General > Strip trailing spaces on Save & corresponding dropdown menu
```

#### Dos2Unix entire directory
```bash
find . -type f -print0 | xargs -0 dos2unix
```
