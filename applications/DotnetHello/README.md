# Dotnet Weather

This is a simple weather application that can run in a container.


## Run with Docker-compose

```
docker-compose build
docker-compose up
```

You can then access the application on http://localhost:8080/.

## Run with Kubernetes

First, make sure you have build the docker image (`docker-compose build`). Then, run:

```
kubectl apply -f k8s
```

You can then access the application on http://localhost:8080/.


## Run with Tilt

Run `tilt up`.

The tilt server with info and status of all jobs should open. You can access the application on http://localhost:8080/.