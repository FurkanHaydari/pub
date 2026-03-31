FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Pubinno.slnx", "./"]
COPY ["Pubinno.Api/Pubinno.Api.csproj", "Pubinno.Api/"]
COPY ["Pubinno.Application/Pubinno.Application.csproj", "Pubinno.Application/"]
COPY ["Pubinno.Data/Pubinno.Data.csproj", "Pubinno.Data/"]
RUN dotnet restore "Pubinno.slnx"
COPY . .
WORKDIR "/src/Pubinno.Api"
RUN dotnet build "Pubinno.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pubinno.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pubinno.Api.dll"]
