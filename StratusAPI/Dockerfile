# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file (csproj is in current build context, not in StratusAPI subfolder)
COPY ["StratusAPI.csproj", "./"]

# Restore dependencies
RUN dotnet restore "StratusAPI.csproj"

# Copy source code
COPY . ./

# Publish application
RUN dotnet publish "StratusAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

#add the following in .env file 
#ENV ConnectionStrings__DefaultConnection="supabase pooler connection"
#ENV JwtSettings__SecretKey=
#ENV JwtSettings__Issuer=StratusAPI
#ENV JwtSettings__Audience=StratusClients
#ENV JwtSettings__AccessTokenExpiryMinutes=
#ENV JwtSettings__RefreshTokenExpiryDays=
#ENV Supabase__SecretKey=
#ENV Supabase__Url=
#ENV Supabase__BucketName=STRATUS_BOLB
#ENV Cors__AllowedOrigins=

# Expose port
EXPOSE 80

# Run application
ENTRYPOINT ["dotnet", "StratusAPI.dll"]

# From C:\Users\sethu\source\repos\Stratus-Prod\Stratus (repo root)
# docker build -t stratusapi:latest -f StratusAPI/Dockerfile .
# docker run --env-file .env -p 8080:80 --rm --name stratusapi stratusapi:latest