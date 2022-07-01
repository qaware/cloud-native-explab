# Cloud Native Workshop - Spring Boot

This is the ReadMe for the Spring boot application for the cloud native workshop.

To be able to follow this workshop the best make sure you are yousing the correct setup and languages mentioned in the 
following Prerequisites. The motto of this workshop will be cloud native, a concept containerizing applications to enable 
applications to work independently of operating systems, being scalable in arbitrary amounts and many other advantages.
Keywords of this workshop will be docker, docker-compose, kubernetes, Tilt, Kustomize and GitHub Actions.

## Prerequisites 

Before you dive right into the concepts of spring boot and cloud native, first make sure all your development
environments are set up properly and to your expectations! 

- Modern Operating System (Windows 10, MacOS, ...)
- Editor of your choice
  - IntelliJ Ultimate
  - IntelliJ Enterprise (enabled docker plugin)
  - Visual Studio
  - ...
- Modern Terminal and shell
- Local Docker installation (Docker Desktop)
- Local Kubernetes installation (Docker Desktop, Minikube)
- Git Client

If all of these are set up, feel free to start right away

## 1) Your First Spring Boot Application

In this chapter of this workshop a short introduction into spring boot is given, helping you to create your first spring 
boot application, which later can be used to apply concepts of cloud native.

To generate a spring boot application simply go to [Spring Initializr](https://start.spring.io/) and customize and generate 
the project to your desired values. In case of this workshop we chose:

- Maven Project
- Spring Boot (TODO: Check Version)
- Group: de.qaware
- Packaging: Jar
- Java: 17
- Dependencies: 
  - Spring Web

All other dependencies will be added later by hand. Alternatively, if your IDE supports Spring Initializr you can create 
the spring boot project directly in your IDE itself and don't need to download and import it.

Either way as soon as this process is finished go to the `src/main/java` folder and your first spring application will 
already be able to run. Just start the Application given and watch the process to start at [Localhost:8080](localhost:8080)
you should now see a small webpage, without much of interest. To change the appearance of your application create a new 
html file in the `resources/static` folder. In case of this workshop we called it index.html, and filled the html file with:

```
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Test application</title>
</head>
<body>
    <p><a>Greetings</a></p>
</body>
</html>
```

With this the application will greet you nicely the moment you open it in your browser.

## 2) Implement further logic

In case of this workshop we create a spring boot web application using Controllers to retrieve calls from the frontend component.
To keep a good overview in this project categories (observable as folders) were chosen to create the structure of source code.

- Controller
  - is used to obtain requests from the frontend
  - uses the service to answer thos requests
- Service
  - implements the entire logic of the backend
  - retrieves information from external providers or databases
- Model
  - contains all classes used as data models in this application
- Provider
  - contains all external endpoints (in this case implemented as simple class)
- Repository
  - contains the database calls of this application

If you want to check the structure of the code, it is recommended to start in the Controller and work through single RequestHandlers
to Service and Provider.

## 3) Docker Images

Docker is a software enabling developers to containerize their application. These docker images can then be shared and can be
run on all environments supporting these images, completely independent of the operating system used.

To create a Docker image of your application a so called Dockerfile needs to be created, telling Docker what to do.
The Dockerfile of this application looks as follows:

```
# Maven build
FROM openjdk:17 as builder

WORKDIR /spring

COPY .mvn/ ./.mvn/
COPY mvnw ./
COPY pom.xml ./

RUN ./mvnw dependency:resolve dependency:resolve-plugins

COPY src/ src/
RUN ./mvnw package -DskipTests



# Base image
FROM openjdk:17

EXPOSE 8080

COPY --from=builder /spring/target/SpringBootWeather-0.0.1-SNAPSHOT.jar springbootweather.jar

ENTRYPOINT ["java", "-jar", "springbootweather.jar"]
```

In the first half, commented as "Maven build", Docker is told to use the base image of openjdk:17. With this then a working
directory is chosen and the necessary maven files are copied, telling Docker the important parts of the project 
configuration. With this, docker is capable of resolving maven dependencies and to package the .jar file, using maven.

After this .jar file is created it is used in the second half, denoted as "Base image". Here again, docker chooses the java 
jdk:17 base image to copy the newly created artifact, and starting it whenever the docker is run, clarified by the ENTRYPOINT.
EXPOSE 8080 then clarifies that your localhost:8080 will make the docker image accessible via your browser of choice at 
[Localhost:8080](localhost:8080).

