FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NotificationSystem.Api/NotificationSystem.Api.csproj", "./NotificationSystem.Api/"]
COPY ["../NotificationSystem.Shared/NotificationSystem.Shared.csproj", "./NotificationSystem.Shared/"]
RUN dotnet restore "NotificationSystem.Api/NotificationSystem.Api.csproj"
COPY . .
RUN dotnet publish "NotificationSystem.Api/NotificationSystem.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationSystem.Api.dll"]
