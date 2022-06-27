# Weather Service with JavaEE

This example implements a simple weather REST service using Payara Micro, JavaEE and Microprofile APIs.

- https://start.microprofile.io

## Build and run with JDK11

```bash
$ mvn package
$ tilt up
$ skaffold dev --no-prune=false --cache-artifacts=false
```

## Exercise the application

```bash
$ curl -X GET http://localhost:8080/api/weather\?city\=Rosenheim
{"city":"Rosenheim","weather":"Sunshine"}

$ curl -X GET http://localhost:8080/

$ curl -X GET http://localhost:8080/metrics

$ curl -X GET http://localhost:8080/health
$ curl -X GET http://localhost:8080/health/ready
$ curl -X GET http://localhost:8080/health/live
```
