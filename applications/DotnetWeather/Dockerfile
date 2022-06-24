FROM mcr.microsoft.com/dotnet/sdk:6.0

COPY DotnetWeather/DotnetWeather.csproj DotnetWeather/
RUN dotnet restore DotnetWeather/DotnetWeather.csproj

COPY . . 
RUN dotnet build "DotnetWeather/DotnetWeather.csproj"

WORKDIR DotnetWeather
ENTRYPOINT ["dotnet", "run"]
