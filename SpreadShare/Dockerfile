FROM microsoft/dotnet:2.2-sdk AS publish
WORKDIR /src

COPY SpreadShare.csproj SpreadShare/
RUN dotnet restore SpreadShare.csproj
COPY . .
WORKDIR /src/SpreadShare
RUN dotnet publish SpreadShare.csproj -c Release -o /app

FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SpreadShare.dll", "--backtest"]
