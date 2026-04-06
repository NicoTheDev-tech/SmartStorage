FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Restore Core project
RUN dotnet restore SmartStorage.Core/SmartStorage.Core.csproj

# Restore Infrastructure project
RUN dotnet restore SmartStorage.Infrastructure/SmartStorage.Infrastructure.csproj

# Restore main project
RUN dotnet restore SmartStorage/SmartStorage.csproj

# Publish the main project
RUN dotnet publish SmartStorage/SmartStorage.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "SmartStorage.dll"]
