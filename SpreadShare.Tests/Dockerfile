FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY SpreadShare.Tests/SpreadShare.Tests.csproj SpreadShare.Tests/
RUN dotnet restore SpreadShare.Tests/SpreadShare.Tests.csproj
COPY . .
WORKDIR /src/SpreadShare.Tests
RUN dotnet build SpreadShare.Tests.csproj -c Release -o /app
RUN dotnet test SpreadShare.Tests.csproj