FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HinduPanchang.Api/HinduPanchang.Api.csproj", "HinduPanchang.Api/"]
RUN dotnet restore "HinduPanchang.Api/HinduPanchang.Api.csproj"
COPY . .
WORKDIR "/src/HinduPanchang.Api"
RUN dotnet publish "HinduPanchang.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5090
EXPOSE 5090
ENTRYPOINT ["dotnet", "HinduPanchang.Api.dll"]
