# ----- Build Stage -----

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY ["src/Gym.Api/Gym.Api.csproj", "src/Gym.Api/"]
COPY ["src/Gym.Application/Gym.Application.csproj", "src/Gym.Application/"]
COPY ["src/Gym.Domain/Gym.Domain.csproj", "src/Gym.Domain/"]
COPY ["src/Gym.Infrastructure/Gym.Infrastructure.csproj", "src/Gym.Infrastructure/"]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]

RUN dotnet restore "src/Gym.Api/Gym.Api.csproj"

COPY . .

RUN dotnet publish "src/Gym.Api/Gym.Api.csproj" -c Release -o /app --no-restore

# ----- Final Stage -----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# Install timezone data for TimeZoneInfo support
RUN apt-get update && apt-get install -y tzdata && \
    ln -fs /usr/share/zoneinfo/America/Montreal /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    rm -rf /var/lib/apt/lists/*

ENV TZ=America/Montreal
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "Gym.Api.dll"]