# Stage 1: Build the application
# We use the heavy SDK image to compile your code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the project file first to restore dependencies (caches this layer)
COPY ["CreditRisk.API.csproj", "./"]
RUN dotnet restore "CreditRisk.API.csproj"

# Copy the rest of the backend source code and build it
COPY . .
RUN dotnet publish "CreditRisk.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run the application
# We use the lightweight ASP.NET runtime image for the final deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render requires web services to bind to a port, usually 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Tell Docker how to start your API
ENTRYPOINT ["dotnet", "CreditRisk.API.dll"]