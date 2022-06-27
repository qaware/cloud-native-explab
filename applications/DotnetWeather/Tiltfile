version_settings(constraint='>=0.22.2')

docker_build(
    'dotnet-weather',
    '.',
    dockerfile='Dockerfile'
)

# k8s_yaml(['k8s/base/db-config.yaml', 'k8s/base/db-deployment.yaml', 'k8s/base/db-service.yaml'])
# k8s_yaml(['k8s/base/dotnet-weather-config.yaml', 'k8s/base/dotnet-weather-deployment.yaml', 'k8s/base/dotnet-weather-service.yaml'])
# apply files separately, or with kustomize

k8s_yaml(kustomize('k8s/overlays/dev'))

# k8s_resource('dotnet-weather', port_forwards='8080:8080')
# port is already exposed by the service LoadBalancer
