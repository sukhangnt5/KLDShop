# Use .NET 8.0 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port (Railway will set PORT env variable)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Create a startup script to handle PORT variable
RUN echo '#!/bin/sh\n\
if [ ! -z "$PORT" ]; then\n\
  export ASPNETCORE_URLS="http://+:$PORT"\n\
fi\n\
dotnet KLDShop.dll' > /app/start.sh && chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]
