# ========================================================
# HỆ THỐNG QUẢN LÝ PHÁT HÀNH BÁO CHÍ (MICROSERVICES)
# DOCKERFILE MULTI-STAGE TỐI ƯU HÓA THEO GIÁO TRÌNH MỤC 6.5.1
# ========================================================

# --- GIAI ĐOẠN RUNTIME CHUNG (Dung lượng siêu nhẹ ~200MB) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# --- GIAI ĐOẠN BUILD DÙNG .NET SDK 8.0 ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copy tất cả các file .csproj để tối ưu Docker Cache khi restore
COPY ["API_GateWay/API_GateWay.csproj", "API_GateWay/"]
COPY ["API_Login/API_Login.csproj", "API_Login/"]
COPY ["API_Admin/API_Admin.csproj", "API_Admin/"]
COPY ["API_BienTap/API_BienTap.csproj", "API_BienTap/"]
COPY ["API_DocGia/API_DocGia.csproj", "API_DocGia/"]
COPY ["BLL/BLL.csproj", "BLL/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["Models/Models.csproj", "Models/"]

# 2. Restore dependencies
RUN dotnet restore "API_GateWay/API_GateWay.csproj" && \
    dotnet restore "API_Login/API_Login.csproj" && \
    dotnet restore "API_Admin/API_Admin.csproj" && \
    dotnet restore "API_BienTap/API_BienTap.csproj" && \
    dotnet restore "API_DocGia/API_DocGia.csproj"

# 3. Copy toàn bộ mã nguồn
COPY . .

# --- 4. PUBLISH TỪNG DỊCH VỤ Ở CHẾ ĐỘ RELEASE ---
FROM build AS publish-gateway
WORKDIR "/src/API_GateWay"
RUN dotnet publish "API_GateWay.csproj" -c Release -o /app/publish/gateway /p:UseAppHost=false

FROM build AS publish-login
WORKDIR "/src/API_Login"
RUN dotnet publish "API_Login.csproj" -c Release -o /app/publish/login /p:UseAppHost=false

FROM build AS publish-admin
WORKDIR "/src/API_Admin"
RUN dotnet publish "API_Admin.csproj" -c Release -o /app/publish/admin /p:UseAppHost=false

FROM build AS publish-bientap
WORKDIR "/src/API_BienTap"
RUN dotnet publish "API_BienTap.csproj" -c Release -o /app/publish/bientap /p:UseAppHost=false

FROM build AS publish-docgia
WORKDIR "/src/API_DocGia"
RUN dotnet publish "API_DocGia.csproj" -c Release -o /app/publish/docgia /p:UseAppHost=false

# ========================================================
# GIAI ĐOẠN CUỐI: CÁC TARGET RUNTIME CHO TỪNG CONTAINER
# ========================================================
FROM base AS gateway
WORKDIR /app
COPY --from=publish-gateway /app/publish/gateway .
ENTRYPOINT ["dotnet", "API_GateWay.dll"]

FROM base AS login
WORKDIR /app
COPY --from=publish-login /app/publish/login .
ENTRYPOINT ["dotnet", "API_Login.dll"]

FROM base AS admin
WORKDIR /app
COPY --from=publish-admin /app/publish/admin .
ENTRYPOINT ["dotnet", "API_Admin.dll"]

FROM base AS bientap
WORKDIR /app
COPY --from=publish-bientap /app/publish/bientap .
ENTRYPOINT ["dotnet", "API_BienTap.dll"]

FROM base AS docgia
WORKDIR /app
COPY --from=publish-docgia /app/publish/docgia .
ENTRYPOINT ["dotnet", "API_DocGia.dll"]
