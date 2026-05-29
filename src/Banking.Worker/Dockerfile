FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Banking.sln ./
COPY src/Banking.Worker/Banking.Worker.csproj src/Banking.Worker/
COPY src/Banking.Application/Banking.Application.csproj src/Banking.Application/
COPY src/Banking.Domain/Banking.Domain.csproj src/Banking.Domain/
COPY src/Banking.Infrastructure/Banking.Infrastructure.csproj src/Banking.Infrastructure/
RUN dotnet restore src/Banking.Worker/Banking.Worker.csproj
COPY . .
RUN dotnet publish src/Banking.Worker/Banking.Worker.csproj -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Banking.Worker.dll"]

