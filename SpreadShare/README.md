# Runner
Run algorithms for backtesting and on Binance.

## Setup and run
#### Setup
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
| Cron               	| Job scheduling using cron      	|
| Dawn.Guard         	| Method argument checking       	|
| NetMQ              	| *Unused*                       	|
| Newtonsoft.Json    	| *Unused*                       	|
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
