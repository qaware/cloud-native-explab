# Dotnet Weather

This is a simple weather application that can run in a container.

There are multiple ways how to run the application.

## Local deployment

Deployment on a local machine is not recommended – we show it here only for completeness.

First, you need to have an instance of [Microsoft SQL server](https://docs.microsoft.com/en-us/sql/database-engine/install-windows/install-sql-server)
installed and running.

Then go to the `DotnetWeather` directory and run:

```
dotnet restore
dotnet build
dotnet run
```

This is the standard way to build a .NET app from the command line. You can find more about `dotnet` commands
in the [docs](https://docs.microsoft.com/en-us/dotnet/core/tools/).

The app will be available on http://localhost:8080/. To change the app configuration, change entries
in [appsettings.json](DotnetWeather/appsettings.json) and [launchSettings.json](DotnetWeather/Properties/launchSettings.json).

## Run with Docker-compose

First, make sure you have [Docker](https://docs.docker.com/desktop/windows/install/) installed. Then, run in this folder:

```
docker-compose build
docker-compose up
```

You can then access the application on http://localhost:8080/.

### Dockerfile
[Dockerfile](Dockerfile) is like a recipe for building a container with our app.
The command `FROM` specifies a base image for this container. The image `mcr.microsoft.com/dotnet/sdk:6.0` already has
installed everything we need (.NET SDK) to build our app, so we use it as base.

We copy the files to our docker container with the command `COPY` and run the same commands `docker restore` and `docker build` as 
when building on a local machine.

The final command `ENTRYPOINT` specifies the command that should be run when the container is started (`docker run`).

You can build the image with the command `docker build . -t dotnet-weather:latest` and run the container
with `docker run -d -p 8080:8080 dotnet-weather:latest`. We need the flag `-p 8080:8080` to forward the port from the container
to a local port, to be able to access the app from outside of the container.
Further reference is in the [docker documentation](https://docs.docker.com/engine/reference/commandline/docker/).

### Docker-compose

`docker-compose` simplifies deploying and configuring multiple containers. In this example, we need two services to make
this app running: the app itself, and the database. Both can be run in their own container and we need to set up the containers
as well as communication between them.

The file [docker-compose.yml](docker-compose.yml) specifies these two services. We pull the database image from the docker repository,
and build the dotnet-weather image locally.

The command `docker-compose` looks in the current directory for a file named [docker-compose.yml](docker-compose.yml), and also for
[docker-compose.override.yml](docker-compose.override.yml). It first applies the configuration from the first file, and then adds or overrides
the configuration from the second file. We use the latter to set environment variables. An override can also be used to
create different docker-compose configuration in developement and production environment.

### Configure the environment

TODO:
- docker-compose.override.yml overrides docker-compose.yml
- there are also environment variables in the .env file
- how can it override appsettings.json and launchSettings.json?
- what can we change and where
- dev vs. prod?

## Run with Kubernetes

First, make sure you have build the docker image (`docker-compose build`). Then, run:

```
kubectl apply -f k8s
```

You can then access the application on http://localhost:8080/.


## Run with Tilt

Run `tilt up`.

The tilt server with info and status of all jobs should open. You can access the application on http://localhost:8080/.
