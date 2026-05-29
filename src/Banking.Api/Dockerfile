FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Banking.sln ./
COPY src/Banking.Api/Banking.Api.csproj src/Banking.Api/
COPY src/Banking.Application/Banking.Application.csproj src/Banking.Application/
COPY src/Banking.Domain/Banking.Domain.csproj src/Banking.Domain/
COPY src/Banking.Infrastructure/Banking.Infrastructure.csproj src/Banking.Infrastructure/
RUN dotnet restore src/Banking.Api/Banking.Api.csproj
COPY . .
RUN dotnet publish src/Banking.Api/Banking.Api.csproj -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Banking.Api.dll"]

