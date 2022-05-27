# Dotnet Weather

This is a simple weather application that can be run from a container.

```
docker build . -t dotnet-weather
docker run -d -p 8080:8080 dotnet-weather
```