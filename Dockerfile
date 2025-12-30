# Use .NET 8.0 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
COPY .config/dotnet-tools.json .config/
RUN dotnet restore
RUN dotnet tool restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Generate migration bundle
RUN dotnet ef migrations bundle -o out/efbundle --self-contained

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port (Railway will set PORT env variable)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Create a startup script to handle PORT variable and run migrations
RUN echo '#!/bin/sh\n\
if [ ! -z "$PORT" ]; then\n\
  export ASPNETCORE_URLS="http://+:$PORT"\n\
fi\n\
echo "Running database migrations..."\n\
if [ ! -z "$DATABASE_URL" ]; then\n\
  ./efbundle --connection "$DATABASE_URL" || echo "Migration failed, continuing..."\n\
else\n\
  echo "DATABASE_URL not set, skipping migrations"\n\
fi\n\
echo "Starting application..."\n\
dotnet KLDShop.dll' > /app/start.sh && chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]
