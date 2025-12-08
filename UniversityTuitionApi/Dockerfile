# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Tüm repo içeriğini kopyala
COPY . .

# Sadece UniversityTuitionApi projesini publish et
RUN dotnet publish ./UniversityTuitionApi/UniversityTuitionApi.csproj -c Release -o /app/publish

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Publish çıktısını kopyala
COPY --from=build /app/publish .

# Render'da container içindeki port
#ENV ASPNETCORE_URLS=http://0.0.0.0:8080
#EXPOSE 8080

# UniversityTuitionApi'yi başlat
ENTRYPOINT ["dotnet", "UniversityTuitionApi.dll"]
