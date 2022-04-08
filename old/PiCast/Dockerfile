#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS restore
WORKDIR /src
COPY ["PiCast/PiCast.csproj", "PiCast/"]
RUN dotnet restore "PiCast/PiCast.csproj"
COPY . .

FROM restore as build
WORKDIR "/src/PiCast"
RUN dotnet build "PiCast.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "PiCast.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PiCast.dll"]
