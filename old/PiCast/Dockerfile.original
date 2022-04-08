FROM mcr.microsoft.com/dotnet/sdk:2.1

RUN apk add --update npm

COPY . .

RUN dotnet restore PiCast.csproj

RUN dotnet build PiCast.csproj

#FROM mcr.microsoft.com/dotnet/runtime:2.1-alpine

#RUN apk add --update npm

#COPY --from=0 

#WORKDIR $(pwd)/bin/debug/netcoreapp2.1/publish/

#RUN ./PiCast --urls=http://0.0.0.0:5000