FROM gcr.io/distroless/java:11

COPY --from=amd64/busybox:1.31.1 /bin/busybox /busybox/busybox
RUN ["/busybox/busybox", "--install", "/bin"]

WORKDIR /payara

COPY --from=payara/micro:5.2022.1 /opt/payara/payara-micro.jar ./

# https://blog.payara.fish/warming-up-payara-micro-container-images-in-5.201
RUN java -jar payara-micro.jar --rootdir micro-root --outputlauncher && \
    rm -rf payara-micro.jar && \
    java -XX:DumpLoadedClassList=classes.lst -jar micro-root/launch-micro.jar --warmup && \
    java -Xshare:dump -XX:SharedClassListFile=classes.lst -XX:SharedArchiveFile=payara.jsa -jar micro-root/launch-micro.jar && \
    rm -rf classes.lst

EXPOSE 8080

ENTRYPOINT ["java", "-server", "-Xshare:on", "-XX:SharedArchiveFile=payara.jsa", "-XX:+UseContainerSupport", "-XX:MaxRAMPercentage=75.0", "-XX:ThreadStackSize=256", "-XX:MaxMetaspaceSize=128m", "-XX:+UseG1GC", "-XX:MaxGCPauseMillis=250", "-XX:+UseStringDeduplication", "-Djava.security.egd=file:/dev/./urandom", "-jar", "micro-root/launch-micro.jar"]
CMD ["--nocluster", "--disablephonehome", "--deploy", "weather-service-javaee.war:/"]

COPY target/weather-service-javaee.war ./
