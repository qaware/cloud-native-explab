FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /DotnetWeather

COPY DotnetWeather/DotnetWeather.csproj ./
RUN dotnet restore DotnetWeather.csproj

COPY DotnetWeather/ .
RUN dotnet build DotnetWeather.csproj

ENTRYPOINT ["dotnet", "run"]
