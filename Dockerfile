FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# List files to debug - remove after it works
RUN ls -la

# Restore the main project directly
RUN dotnet restore SmartStorage/SmartStorage.csproj

# Publish the main project
RUN dotnet publish SmartStorage/SmartStorage.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "SmartStorage.dll"]
