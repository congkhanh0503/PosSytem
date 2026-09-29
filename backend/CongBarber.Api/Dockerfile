# ==========================================
# Stage 1: Build .NET API Application
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy file csproj và restore dependencies trước để tận dụng Docker cache
COPY ["CongBarber.Api.csproj", "./"]
RUN dotnet restore "CongBarber.Api.csproj"

# Copy toàn bộ mã nguồn và build release
COPY . .
RUN dotnet publish "CongBarber.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Stage 2: Runtime Image (Siêu nhẹ)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Thiết lập biến môi trường chạy trên port 5012
ENV ASPNETCORE_URLS=http://+:5012
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy kết quả build từ stage trước
COPY --from=build /app/publish .

# Expose cổng 5012
EXPOSE 5012

# Khởi chạy API
ENTRYPOINT ["dotnet", "DiroPos.dll"]
