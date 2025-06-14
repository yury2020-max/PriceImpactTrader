# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY *.csproj ./
RUN dotnet restore

# Copy source code
COPY . .
RUN dotnet build -c Release -o /app/build

# Runtime stage with Python
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

# Install Python and pip
RUN apt-get update && \
    apt-get install -y python3 python3-pip python3-venv && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Create Python virtual environment
RUN python3 -m venv /app/.venv

# Copy requirements first for better caching
COPY requirements.txt .
RUN /app/.venv/bin/pip install --no-cache-dir -r requirements.txt

# Copy published app
COPY --from=build /app/build .

# Copy default config
COPY config.json .

# Make sure Python venv is in PATH
ENV PATH="/app/.venv/bin:$PATH"

# Set entrypoint
ENTRYPOINT ["dotnet", "PriceImpactTrader.dll"]