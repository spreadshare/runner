FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY SpreadShare/SpreadShare.csproj SpreadShare/
RUN dotnet restore SpreadShare/SpreadShare.csproj
COPY SpreadShare.Tests/SpreadShare.Tests.csproj SpreadShare.Tests/
RUN dotnet restore SpreadShare.Tests/SpreadShare.Tests.csproj

ARG API_READ_KEY="key"
ARG API_READ_SECRET="secret"

COPY . .
RUN cp SpreadShare/appsettings.json.example SpreadShare/appsettings.json
RUN sed -i "s/api_key/${API_READ_KEY}/g" SpreadShare/appsettings.json
RUN sed -i "s/api_secret/${API_READ_SECRET}/g" SpreadShare/appsettings.json

WORKDIR /src/SpreadShare
RUN dotnet build SpreadShare.csproj -p:Configuration=Release /warnaserror

WORKDIR /src/SpreadShare.Tests
RUN dotnet build SpreadShare.Tests.csproj -p:Configuration=Release /warnaserror
RUN dotnet test SpreadShare.Tests.csproj