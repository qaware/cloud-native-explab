FROM mcr.microsoft.com/dotnet/sdk:6.0

EXPOSE 80

COPY DotnetHello.App/DotnetHello.App.csproj DotnetHello.App/
RUN dotnet restore DotnetHello.App/DotnetHello.App.csproj

COPY . . 
RUN dotnet build "DotnetHello.App/DotnetHello.App.csproj"

WORKDIR DotnetHello.App
ENTRYPOINT ["dotnet", "run"]
