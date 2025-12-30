# Use .NET 8.0 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY --from=build /app/out .
COPY --from=build /app/*.csproj ./
COPY --from=build /app/Migrations ./Migrations

# Expose port (Railway will set PORT env variable)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Install EF Core tools in runtime
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Create a startup script to handle PORT variable and run migrations
RUN echo '#!/bin/sh\n\
if [ ! -z "$PORT" ]; then\n\
  export ASPNETCORE_URLS="http://+:$PORT"\n\
fi\n\
echo "Running database migrations..."\n\
dotnet ef database update --no-build || echo "Migration failed, continuing..."\n\
echo "Starting application..."\n\
dotnet KLDShop.dll' > /app/start.sh && chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]
