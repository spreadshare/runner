ARG API_READ_KEY="key"
ARG API_READ_SECRET="secret"

FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY SpreadShare.Tests/SpreadShare.Tests.csproj SpreadShare.Tests/
RUN dotnet restore SpreadShare.Tests/SpreadShare.Tests.csproj

COPY . .
RUN cp SpreadShare/appsettings.json.example SpreadShare/appsettings.json
RUN sed -i "s/api_key/${API_READ_KEY}/g" SpreadShare/appsettings.json
RUN sed -i "s/api_secret/${API_READ_SECRET}/g" SpreadShare/appsettings.json


WORKDIR /src/SpreadShare.Tests
RUN dotnet build SpreadShare.Tests.csproj -c Release -o /app
RUN dotnet test SpreadShare.Tests.csproj