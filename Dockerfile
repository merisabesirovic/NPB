FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RideMateAPI/RideMateAPI.csproj RideMateAPI/
RUN dotnet restore RideMateAPI/RideMateAPI.csproj

COPY RideMateAPI/ RideMateAPI/
RUN dotnet publish RideMateAPI/RideMateAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet RideMateAPI.dll --urls http://0.0.0.0:${PORT:-10000}"]
