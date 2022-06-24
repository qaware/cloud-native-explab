# Improve your containerized App

If you read this, you already containerized a simple spring web application, so congratulation.

In this chapter now an additional database should be added, structure will be given to the code,
and more steps towards a 12 factor app are done.

### Structure of the code

To achieve good readability as well as a good overview over your code, you should cluster it into different
classes. A possible structure should be given here:

* Application: Here only the start of the application happens, marked by the @SpringBootApplication Annotation

* Controller: As a @RestController they are able to obtain and define http requests from frontend components

* Service: Inside the service all data is requested from the storage, analysed and modified to pass it back to the controller.

* Model: Models sums up all classes, which are important and defined for our code.

* Repository: Repositories are used for communication with the used databases.

Of course all of these are just modeling recommendations and can differ from case to case.

### Include a database 

To include a database into your spring boot application, first of all add following dependencies to your pom.xml:

```
<dependency>
     <groupId>org.springframework.boot</groupId>
     <artifactId>spring-boot-starter-data-jpa</artifactId>
</dependency>

<dependency>
     <groupId>org.postgresql</groupId>
      <artifactId>postgresql</artifactId>
</dependency>
```

Hereby, jpa is used as communication between your code and the database. An example of this communication can be seen inside 
the Weather model of this application:

```
@Entity
public class Weather {

    @Id
    @GeneratedValue(strategy = GenerationType.AUTO)
    private Integer id;
    private String city;
    private String weather;
    private Date date;

    public Integer getId() {
        return id;
    }

    ...
}
```

Annotated with the @Entity tag jpa recognizes this class as table inside a chosen database. Values marked with @Id
in this case the id of our weather data, are keys of this data table and therefore ideal for relational database models.
As database in this application postgreSQL is used, but of course other databases like mysql are implementable in the same way.

Adding a Repository based on the CrudRepository:

```
public interface WeatherRepository extends CrudRepository<Weather, Integer> {

    Weather findWeatherById(Integer Id);

}
```

, which implements the fundamental requests: create, read, update, delete. And using
this data in service calls in the backend the database is usable already. Still, a problem occurs!
We have not defined in any way, where our database is located.

For this reason we need to create an application.properties file inside our src/main/resources folder.
In this we define location as well as passwords and type of our database.

```
spring.datasource.url=jdbc:postgresql://localhost:5432/weather_db
spring.datasource.username=springuser
spring.datasource.password=password
spring.jpa.hibernate.ddl-auto=update
spring.jpa.properties.hibernate.dialect = org.hibernate.dialect.PostgreSQLDialect

spring.datasource.driver-class-name=org.postgresql.Driver
```

* spring.datasource.url: describes the url of our database in this case localhost:5432 (default port for postgres)
* spring.datasource.username: username of postgres database
* spring.datasource.password: password of user database
* spring.jpa.hibernate.ddl-auto: jpa updates the database, when started and written (alternatively: create, none, ...)
* spring.jpa.properties.hibernate.dialect: defines in this case postgres as database language
* spring.datasource.driverclassname: tells jpa to interpret the url as url pointing at a postgres database

### Creating a postgres database

If you don't know how to create a postgres database don't worry! I would personally recommend using docker, cause you
have this already installed in this step. Create a docker-compose.yml file, containing the following:

```
version: '3.5'

services:
  db:
    image: postgres:12
    container_name: db
    restart: always
    environment:
      POSTGRES_USER: springuser
      POSTGRES_PASSWORD: password
      POSTGRES_DB: weather_db
    ports:
      - "5432:5432"
```

Open a command line at the folder, where this file is located and type ``` docker-compose up ``` and you already created
a postgres database fitting our application properties. With this, now your application should run
and your database (as long it exists) contains the data, even when u restart the application itself.

#### Containerize the database combined with your application

But now there is a problem! If you define localhost as your database, obviously this will work at your pc
but not on another one or a kubernetes cluster. Therefore, we are not done now. To achieve a docker image of your new application
to work, we include it in the given docker-compose.yml. With this we create a multi-container docker image working on an
arbitrary machine.

For this the dev prod parity concept should be kept. This means we only change in the docker image,
what really cannot stay the same as on our local devices. Therefore, we add to our existing docker-compose file:

```
app:
    depends_on:
     - db
    build: .
    ports:
     - "8081:8080"
    environment:
      SPRING_DATASOURCE_URL : jdbc:postgresql://db:5432/weather_db
volumes:
  postgre_db:
    driver: local
```

Here now as a second service the written application is mentioned. As dependency the database is given
leading to the app being started after the database is implemented. The build attribute refers to the Dockerfile given
in the same folder, building your application as before into a dockerimage. Ports can be again chosen in the schema:
``` Your Desired Port : Docker Port ```

The most important line is here ``` SPRING_DATASOURCE_URL : jdbc:postgresql://db:5432/weather_db ```. As an environment variable
this line changes the url of the database to a new one positioned not on your localhost, but on the database created inside this
docker container and therefore the database designed above.

With this and the ``` docker-compose up ``` command, now a containerized version of your application can be created.

To enable a better overview in case of this application an additional .env file was added containing values of our
docker-compose file, meaning an easier change of all variables inside this file. For a better understanding look into the .env
file as well as the docker-compose.yml of this application.

Congrats!!!

At this point we can also add additional docker-compose files, like
`docker-compose.override.yml`, which will be read automatically by the docker-compose command, while additional
files like docker-compose.prod.yml would be possible too, to generate different environments depending on your needs. These
files then can be started by the

```
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

command.


### Kubernetes

Up to this point you now achieved to create a multi container application capable of running your app inside docker without a local 
database. To further improve application to a 12 factor app, including cloud native concepts, the next step will be a deployment
on kubernetes clusters. 

While Docker enables Developers to run their apps in container independently, kubernetes organizes containers, services and deployments
in a way they can be analysed and work properly. Kubernetes cares about restarting containers as well as smart management of
working ones.


For this, again we can use docker as it offers the deployment to a local kubernetes cluster. For this
go to your docker desktop application -> Settings -> Kubernetes and enable the local kubernetes cluster option. After a restart
in your task bar at the docker icon you should see that Kubernetes is running.

To make sure, that all of your configurations are the same as in this tutorial, open a command line and type 

```
kubectl config get-contexts
```

With this you should now see possible kubectl contexts on your device. As we use docker for our local kubernetes deployment,
type:

```
kubectl config use-context docker-desktop
```

to choose docker as your context, if it's not already set. To check if all works properly you can check your output, when typing:

```
kubectl get nodes
```

With this you are now able to deploy to a local kubernetes cluster inside docker. But how?

### Deploy your app to a local kubernetes cluster

To achieve a deployment you will need obviously files telling what to deploy. In this case our docker-compose.yml gives us
a good overview, but is sadly not compatible with kubectl. Kubectl needs .yaml-files of the structure given in this project,
which distinguishes between pods, deployments, containers and further information needed to define your wanted cluster setup.

Therefore, in this case for a good overview four yaml files where created. Two of these affect the database and two the application.
Each of them are generated by one deployment and one service file leading to the total number of 4.

At this point, you probably wonder why we need a service and a deployment and can't combine them or one is redundant.
At least I did so let's discuss it. A deployment is necessary as it is responsible to set up a so called pod. A pod is
necessary to give the framework for your application to run in. It decides how many of your applications run in parallel
and how it should behave in certain cases. It declares dependencies, like in this case the database for the application
and cares about the context around your app. A service does not care about these points. It uses a pod to run and grants users network access to the application from
the outside.

With this out of the way it should be clear now, that services and deployments work nicely together. To set up a deployment
a yaml file could look something like:

```
apiVersion: apps/v1
kind: Deployment
metadata:
  creationTimestamp: null
  name: app
spec:
  replicas: 1
  selector:
    matchLabels:
      springbootweather: web
  template:
    metadata:
      labels:
        springbootweather: web
      creationTimestamp: null
    spec:
      containers:
        - env:
            - name: SPRING_DATASOURCE_URL
              value: jdbc:postgresql://db:5432/weather_db
          image: springbootweather_app
          imagePullPolicy: Never
          name: app
          ports:
            - containerPort: 8080
          resources: {}
      restartPolicy: Always
status: {}
```

As explained before, here general information is given. Next to the apiVersion of your file, the kind of this file is 
clarified as well as name of the desired pod, labels, environment variables and much more. An important line in this code
is ```ImagePullPolicy: Never```. This prevents your kubernetes of trying to pull a docker image of docker hub as you want to use 
your local Docker image. Defining now a service of this app:

```
# Service
apiVersion: v1
kind: Service
metadata:
  labels:
    springbootweather: web
  creationTimestamp: null
  name: app
spec:
  ports:
    - name: "8081"
      port: 8081
      targetPort: 8080
  selector:
    springbootweather: web
status:
  loadBalancer: {}
```

your app will be able to be accessed. Analog files for the database are of course necessary. To check them out, feel free
to look in the yaml files of this application in the `kubernetes` folder and scroll through the code and comments to understand it better.
An additional configmap is given there as well, meaning a .yaml file containing necessary
environment variables, which can be used in the other .yaml files.

After you fully understood the .yaml-files, you are now capable of deploying to your local cluster. For this open your 
command line at the folder your yaml files are in. After that use these to create your application with the commands:

```
kubectl apply -f <yaml-file>
```

To create the app of this example open the `SpringBootWeather/kubernetes` folder and type the commands:

```
kubectl apply -f db_config.yaml
kubectl apply -f weather_db_deployment.yaml
kubectl apply -f weather_db_service.yaml
kubectl apply -f app_config.yaml
kubectl apply -f weather_app_deployment.yaml
kubectl apply -f weather_app_service.yaml
```

After retrieving information of running pods, with the line

```
kubectl get pods
```

or specific logs of a single pod running:

```
kubectl logs <podname>
```

you should see two running pods in your local kubernetes cluster. If that is the case, congrats, you just deployed your
application to a local kubernetes cluster. To now stop your kubernetes cluster from working, type:

```
kubectl delete -f <yaml-file>
```

and your pods will shut down.

As you probably noticed already, these are a lot of commands to do, so can we simplify that?
Of course, we can.

### Simplify your deployment using Tilt

At this point you now created a working spring boot application and were able to containerize it, deploy it to a container
inside docker as well as a local kubernetes cluster on your device. Now, we want to simplify this process and make your 
life as a developer easier. For this reason we want to take a closer look onto tilt. For this go to the page 
https://docs.tilt.dev/install.html and install tilt for your operating system.

Tilt is a microservice not only deploying to the kubernetes cluster as you wish, it also enables a graphic overview of
your running pods and immediate feedback if something goes wrong or crashes. Furthermore, it updates your kubernetes cluster
on any code change automatically and reduces your command line input to ```tilt up```. To get a feeling for all this, I
recommend you to shut down all running kubernetes pods and docker container for now. And go to your command line into the
SpringBootWeather folder.

To check your tilt is set up correctly and is working, you can type:

```
tilt version
```

an additional time. Now if all is set up correctly, you should be able to run ```tilt up``` and a short output gives u the
option to press spacebar. After u pressed spacebar your standard browser should open and tilt should start your local kubernetes
pods automatically. At this points check the outputs by taking a closer look onto all given pods and deployments.

To add tilt to your application, again, you need to add a setup file for tilt, a so called Tiltfile. This Tiltfile contains
information of your kubernetes yaml files and Dockerfile as well as further information of port forwarding. Looking at the
Tiltfile of this project all will become a little clearer, hopefully.

```
print('Hello Tiltfile')

k8s_yaml(['kubernetes/db_config.yaml','kubernetes/weather_db_deployment.yaml', 'kubernetes/weather_db_service.yaml'])
k8s_yaml(['kubernetes/app_config.yaml','kubernetes/weather_app_deployment.yaml', 'kubernetes/weather_app_service.yaml'])

docker_build('springbootweather_app', '.')

k8s_resource('app', port_forwards='8081:8080')
```

The first line creates an output in your tilt webview, which should you have noticed already using the tilt up command.
It is just to create feedback and has no further use for us. Feel free to delete it at any time you want. The second and 
third line use our yaml files to create kubernetes deployments and services, followed by the fourth line necessary in case
of an update of sourcecode, which creates a new docker image of your application in case its necessary. The last line
puts the application onto port 8000 of your localhost, which can directly be accessed over tilt as well.

If you add a Tiltfile as easy as this to your application, you now added Tilt to your project and hopefully need to use 
a lot less commands in your workflow. 