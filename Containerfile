FROM node:24 AS node-build

WORKDIR /src/SKP.OS.Frontend
COPY SKP.OS.Frontend/package*.json ./
RUN npm ci
COPY SKP.OS.Frontend/. .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
COPY --from=node-build /src/SKP.OS.Frontend/dist /src/SKP.OS.Backend/wwwroot/dist
RUN dotnet restore SKP.OS.Backend/SKP.OS.Backend.csproj
RUN dotnet publish SKP.OS.Backend/SKP.OS.Backend.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "SKP.OS.Backend.dll"]
COPY --from=build /app/publish ./
