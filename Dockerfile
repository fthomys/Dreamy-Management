FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Dreamy-Management.csproj .
RUN dotnet restore -p:TargetFramework=net10.0 -r linux-x64
COPY . .
RUN dotnet publish Dreamy-Management.csproj -c Release -r linux-x64 --self-contained false \
    -p:TargetFramework=net10.0 --no-restore -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /out .
# config.ini is read from the working directory, mount it to /app/config.ini
ENTRYPOINT ["./Dreamy-Management"]
