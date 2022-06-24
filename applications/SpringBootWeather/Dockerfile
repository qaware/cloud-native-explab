FROM openjdk:17 as builder

WORKDIR /spring

COPY .mvn/ ./.mvn/
COPY mvnw ./
COPY pom.xml ./

RUN ./mvnw dependency:resolve dependency:resolve-plugins

COPY src/ src/
RUN ./mvnw package -DskipTests


# base image
FROM openjdk:17

# port
EXPOSE 8080

# jar file
COPY --from=builder /spring/target/SpringBootWeather-0.0.1-SNAPSHOT.jar springbootweather.jar

# entry
ENTRYPOINT ["java", "-jar", "springbootweather.jar"]