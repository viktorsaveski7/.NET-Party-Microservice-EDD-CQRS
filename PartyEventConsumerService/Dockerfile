FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["PartyEventConsumerService/PartyEventConsumerService.csproj", "PartyEventConsumerService/"]
COPY ["GuestService/GuestService.Domain/GuestService.Domain.csproj", "GuestService/GuestService.Domain/"]
COPY ["GuestService/GuestService.Application/GuestService.Application.csproj", "GuestService/GuestService.Application/"]
COPY ["GuestService/GuestService.Infrastructure/GuestService.Infrastructure.csproj", "GuestService/GuestService.Infrastructure/"]

RUN dotnet restore "PartyEventConsumerService/PartyEventConsumerService.csproj"

COPY . .

WORKDIR /src/PartyEventConsumerService
RUN dotnet publish "PartyEventConsumerService.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PartyEventConsumerService.dll"]
