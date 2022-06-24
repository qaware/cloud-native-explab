# To get feedback in the tilt UI
print('Hello Tiltfile')

# Deployment of the database. Replaces following commands in kubectl:
# kubectl apply -f weather_db_deployment.yaml
# kubectl apply -f weather_db_service.yaml
#k8s_yaml(['kubernetes/base/db_config.yaml','kubernetes/base/weather_db_deployment.yaml', 'kubernetes/base/weather_db_service.yaml'])

# Deployment of the application. Replaces following commands in kubectl:
# kubectl apply -f weather_app_deployment.yaml
# kubectl apply -f weather_app_service.yaml
#k8s_yaml(['kubernetes/base/app_config.yaml','kubernetes/base/weather_app_deployment.yaml', 'kubernetes/base/weather_app_service.yaml'])

k8s_yaml(kustomize('kubernetes/base/'))

# builds the docker image of the local application. Replaces following commands of docker:
# docker build -t springbootweather_app .
docker_build('springbootweather_app', '.')

# maps the port of the application to localhost:8081
k8s_resource('app', port_forwards='8081:8080')
