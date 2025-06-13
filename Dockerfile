# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project files
COPY *.csproj ./
RUN dotnet restore

# Copy source code
COPY . .
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/build .

# Copy default config
COPY config.json .

# Set entrypoint
ENTRYPOINT ["dotnet", "PriceImpactTrader.dll"]