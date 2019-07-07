# SpreadShare
Run trading algorithms (Finite State Machines) on historical candle data and Binance.

This application is no longer maintained. We have developed visualisation, data fetching (Binance) and deployment (Ansible) software that integrates with this application. If you would like to acquire these applications, open an issue.

### Usage
The runner has two modes: Live and Backtest. In Live mode, API-credentials will be used to trade live on Binance. It is recommended to run live instances on a separate server and not locally. Backtests require historical candle data provided by a postgres database. Algorithms are mode-agnostic.

##### Algorithms
Algorithms are a collection of `State`s that return other states. States have several important properties and methods as shown in the two tables below.

| Properties |   |
|------------|---|
| AlgorithmConfiguration | Provides user settings for this algorithm. The configuration provides parameter tuning |
| FirstPair | The first pair of the configurated trading pairs. Useful when only using one trading pair |

| Methods    |   |
|------------|---|
| Run | Runs immediately after being switched to |
| OnMarketCondition | Is called every candle update |
| OnOrderUpdate | Is called on every order update reported by Binance |
| OnTimerElapsed | Is called when a set timer has elapsed |
| SetTimer | Sets an timer |
| WaitForNextCandle | Wait until the candle has elapsed |


### Setup
Migrations only have to be ran once to update the database schema. `appsettings.yaml` contains all required settings. If you are using docker `.env` is required as well.

##### Requirements
- Dotnet 2.2 or Docker
- A PostgreSQL database
- Some knowledge about trading, candles, orders, crypto and Binance

##### Linux setup (dotnet)
1. Install [Dotnet Core](https://dotnet.microsoft.com/download/dotnet-core/2.2)
2. Install [PostgreSQL](https://www.digitalocean.com/community/tutorials/how-to-install-and-use-postgresql-on-ubuntu-18-04)
3. Copy and adjust `appsettings` file
   ```
    cp SpreadShare/appsettings.json.example SpreadShare/appsettings.yaml
    nano SpreadShare/appsettings.yaml
   ```
4. Build project
   ```
   dotnet build SpreadShare.dll
   ```
5. Run migrations
   ```
   cd SpreadShare/bin/Debug/netcoreapp2.2/
   dotnet SpreadShare.dll --migrate
   ```
6. Import Candle data to database (https://github.com/spreadshare/datagathering)
7. Start runner
   ```
   dotnet SpreadShare.dll                       <-- Backtesting mode
   dotnet SpreadShare.dll --trading             <-- Trading mode
   ```
8. Run an algorithm
   ```
   run --algorithm TemplateAlgorithm -i
   ```

##### Linux setup (docker)
Use steps above, but instead of using dotnet:

1. Install Docker
    ```
    sudo apt-get install -y docker-ce && docker-compose
    ```
2. Add correct volumes and networks
    ```
    docker volume create --name postgres-data
    docker network create --name spreadshare_network
    ```
3. Copy and adjust .env
    ```
    cp .env.example .env
    nano .env
    ```
4. Copy and adjust `appsettings` file
5. Run migrations (adjust dockerfile to match correct entrypoint)
6. Import Candle data to database (https://github.com/spreadshare/datagathering)
7. Run docker-compose
    ```
    docker-compose up --build
    ```


