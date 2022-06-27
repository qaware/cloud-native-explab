# Cloud-native Experience Lab (for Software Engineers)

This experience lab puts its focus on cloud-native software engineers. No matter which implementation language is used, the conceptual items are very similar.

![Weather Service Architecture](architecture.png)

Currently, several implementations of the lab and the Weather Service are available:
- JavaEE 8: https://github.com/qaware/cloud-native-weather-javaee
- Spring Framework: https://github.com/qaware/cloud-native-weather-spring
- Golang: https://github.com/qaware/cloud-native-weather-golang
- .NET Core: https://github.com/qaware/cloud-native-weather-dotnet

## Cloud-native Development

### 12-factor App Principles

There are several principles a good cloud-native application must and should adhere to.

_TODO:_ Implement some of the 12-factor app principles within the given application. Think about external configuration via ENV variables. Circuit breakers. Metrics. Logs. et.al

### Containerization

In this stage, the microservice and all required resources need to be containerized, so that the runtime environment is the same overall stages of the software.

_TODO:_ Implement a Dockerfile to build and containerized the given application.

### Deployment (using Kustomize)




### Deployment (using Helm)
_TODO_

## Local Developer Experience

### Skaffold

_TODO:_ Implement the required Skaffold manifest to continously build and deploy the artifact to the local development environment.

### Tilt

_TODO:_ Implement the required Tilt manifest to continously build and deploy the artifact to the local development environment.

## Continuous Integration

The software under development must be built on every (significat) change to ensure that no regressions have been introduced by that latest changes.

### GitHub Actions

_TODO:_ Implement GitHub Actions to build and publish the final Docker artifacts for deployment.


## Continous Deployment

### Flux

## Continuous Testing

### Testkube
