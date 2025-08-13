FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NotificationSystem.Worker/NotificationSystem.Worker.csproj", "./NotificationSystem.Worker/"]
RUN dotnet restore "NotificationSystem.Worker/NotificationSystem.Worker.csproj"
COPY . .
RUN dotnet publish "NotificationSystem.Worker/NotificationSystem.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationSystem.Worker.dll"]
