# SpreadShare2 [![Build Status](https://travis-ci.com/HugoPeters1024/SpreadShare2.svg?token=EzhgBYg4bqpNUB6Jq5aw&branch=master)](https://travis-ci.com/HugoPeters1024/SpreadShare2)
Using a simple bandwagon algorithm for cryptocurrency trading on Binance

## Setup
1. Install [Docker](https://docs.docker.com/install/)
2. _(Windows)_ [Share C drive with Docker](https://medium.com/travis-on-docker/why-and-how-to-use-docker-for-development-a156c1de3b24)
3. Create volume postgres-data 
```docker volume create --name postgres-data```
4. Create an `.env` file from the `.env.example` file. Replace the default passwords with randomly generated passwords.
5. Create an `appsettings.json` file from the `appsettings.json.example` file. Replace the API credentials with actual credentials.
5. Run Visual Studio with Docker Compose configuration or run
```docker-compose up```

### Linux setup
```
sudo apt-get install -y docker-ce && docker-compose
docker volume create --name postgres-data
cp .env.example .env
cp SpreadShare/appsettings.json.example SpreadShare/appsettings.json
docker-compose up
```

## Development
If you want to run this application without Docker, the framework [.NET Core 2.1](https://www.microsoft.com/net/download/dotnet-core/2.1) is required. Furthermore, please read the [testing documentation](TESTING.md).

### Database migrations
Each time the database schema changes, the following command needs to be ran.
```
dotnet ef migrations add [MigrationTitle]
```

### Dawn's Guard plugin
For argument checking, the project consistenly uses the Guard plugin from Dawn. This plugin requires at least C#7.2. You can review the documentation here https://github.com/safakgur/guard

### Architecture
This is a console application written in [ASP.Net Core 2.1](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-2.1). [Dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1) is used for injecting common dependencies (such as database context and logger factories). Different modules are separated in services which are registered in `Startup.cs` and can be accessed via a `ServiceCollection`. A [PostgreSQL](https://www.postgresql.org/) database is connected with a database context. This database runs in a separate Docker container. There are three Docker containers in total: SpreadShare, the database and a ZeroMQ-enabled python application.

### Using Docker in development
We have chosen to use Docker in development rather than only using Docker for deployment. This approach allows for a consistent development environment that only requires Docker and easy deployment [[1](https://medium.com/travis-on-docker/why-and-how-to-use-docker-for-development-a156c1de3b24)]. To enable debugging, we required the integration with Docker of Visual Studio. This is configured by adding support for Docker [[2](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/visual-studio-tools-for-docker?view=aspnetcore-2.1)]. This results in a Docker Compose run configuration instead of a Spreadshare configuration.

**Disadvantage 1: Unclear error output in Docker configuration**
However, this has a disadvantage. Errors in the `docker-compose.yml` file and the `Dockerfile`s are not displayed clearly in the output. This can be resolved by running `docker-compose build & docker-compose up` in a terminal window. 

**Disadvantage 2: No terminal color support in debug output**
Furthermore, the Visual Studio debug output window does not support special characters and by extensions colors in output ([Github issue](https://github.com/aspnet/Logging/issues/428)). We have disabled the color output using `DisableColors` in `ConsoleLoggerOptions`.

### Splitting debug logs into default output and program output
The debug output contains default output and program output. If you would like to split this output, change the following setting in Visual Studio:
> _Tools -> Visual Studio Options Dialog -> Debugging -> Check the "Redirect All Output Window Text to the Immediate Window"_
