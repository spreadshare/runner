FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY SpreadShare/SpreadShare.csproj SpreadShare/
RUN dotnet restore SpreadShare/SpreadShare.csproj
COPY . .
WORKDIR /src/SpreadShare
RUN dotnet build SpreadShare.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish SpreadShare.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
#ENTRYPOINT ["dotnet", "SpreadShare.dll", "--verbose"]
# Run the container like a deamon
ENTRYPOINT ["tail", "-f", "/dev/null"]
