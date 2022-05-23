FROM mcr.microsoft.com/dotnet/aspnet:6.0

EXPOSE 80

COPY ["DotnetHello.App/DotnetHello.App.csproj", "."]
RUN dotnet restore "./DotnetHello.App/DotnetHello.csproj"
COPY . . 
RUN dotnet build "DotnetHello.App/DotnetHello.csproj"

ENTRYPOINT ["dotnet", "DotnetHello.App.dll"]