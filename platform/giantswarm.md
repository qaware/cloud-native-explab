# Giantswarm Managed Kubernetes Platform (on AWS)

This lab will provision a GiantSwarm based platform with several useful infrastructure components as well
as a demo application.

## Prerequisites

Before you dive right into this experience lab, make sure your local development environment is setup properly! 

- Modern Operating System (Windows 10, MacOS, ...) with terminal and shell
- IDE of your personal choice (with relevant plugins installed)
  - IntelliJ Ultimate
  - VS Code
- (_optional_) Local Docker / Kubernetes installation (Docker Desktop, Rancher Desktop, Minikube)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Flux2](https://tilt.dev)
- [Kustomize](https://kustomize.io)


## GiantSwarm Cluster Setup

_TODO_

## Platform Bootstrapping with Flux2

Next, we will bootstrap Flux2 as GitOps tool to provision further infrastructure and platform components.

**Lab Instructions**
1. Install the Flux2 CLI on your developer machine
2. Create personal Github token and export as ENV variable
3. Bootstrap the flux-system namespace and components
    - use a personal repository as GitOps repository
    - enable extra components: _image-reflector-controller_ and _image-automation-controller_
    - create a read-write SSH key
4. Add Flux2 Kustomization for platform `infrastructure/` folder

<details>
  <summary markdown="span">Click to expand solution ...</summary>
  
  ```bash
# install the Flux2 CLI on the master node
# see https://fluxcd.io/docs/installation/
curl -s https://fluxcd.io/install.sh | sudo bash

# see https://fluxcd.io/docs/get-started/
# generate a personal Github token
export GITHUB_USER=qaware
export GITHUB_TOKEN=<your-github-token>

# bootstrap the flux-system namespace and components
flux bootstrap github \
    --owner=$GITHUB_USER \
    --repository=cloud-native-explab \
    --branch=main \
    --path=./clusters/gorilla/cne01 \
    --components-extra=image-reflector-controller,image-automation-controller \
    --read-write-key
    --personal         # only for user accounts, not for org accounts

# you may need to update and modify Flux kustomization
# - infrastructure-sync.yaml

flux create kustomization infrastructure \
    --source=GitRepository/flux-system \
    --path="./infrastructure/gorilla/cne01"
    --prune=true \
    --interval=5m0s \
    --export > ./clusters/gorilla/cne01/infrastructure-sync.yaml

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization infrastructure
flux get all
  ```
</details>

### Observability with Grafana, Loki and Tempo

For good observability we will use a Grafana-based stack, which is completely free software:
- [Prometheus](https://prometheus.io/) to collect metrics
- [Promtail](https://grafana.com/docs/loki/latest/clients/promtail/) to forward logs to [Loki](https://grafana.com/docs/loki/latest/)
- [Tempo](https://grafana.com/docs/tempo/latest/) to receive traces

**Lab Instructions**
1. Add Helm repository for Prometheus community charts using Flux CLI and GitOps repository
2. Add `observability` namespace via GitOps repository
3. Install `kube-prometheus-stack` Helm chart (39.5.0 or later) using Flux CLI and GitOps repository
4. Add Helm repository for Grfana chars using Flux CLI and GitOps repository
5. Install `tempo` Helm chart (0.15.0 or later) or later using Flux CLI and GitOps repository
5. Install `promtail` Helm chart (2.6.0 or later) using Flux CLI and GitOps repository
5. Install `loki` Helm chart (2.13.0 or later) using Flux CLI and GitOps repository

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
# we can use the Flux CLI to create the GitOps manifests for the observability stack
cd infrastructure/gorilla/cne01

# create a Helm source and release for a the kube-prometheus-stack
flux create source helm prometheus-community \
    --url=https://prometheus-community.github.io/helm-charts \
    --interval=10m0s \
    --export > observability/prometheus-community-source.yaml

flux create hr kube-prometheus-stack \
    --source=HelmRepository/prometheus-community \
    --chart=kube-prometheus-stack \
    --chart-version="39.5.0" \
    --target-namespace=observability \
    --create-target-namespace=false \
    --export > observability/kube-prometheus-stack.yaml

# create a Helm source for Grafana charts and for the individual releases
flux create source helm grafana-charts \
    --url=https://grafana.github.io/helm-charts \
    --interval=10m0s \
    --export > observability/grafana-charts-source.yaml

flux create hr tempo \
    --source=HelmRepository/grafana-charts \
    --chart=tempo \
    --chart-version=">=0.15.0 <0.16.0" \
    --target-namespace=observability \
    --create-target-namespace=false \
    --export > observability/tempo-release.yaml

flux create hr promtail \
    --source=HelmRepository/grafana-charts \
    --chart=promtail \
    --chart-version=">=2.6.0 <2.7.0" \
    --target-namespace=observability \
    --create-target-namespace=false \
    --export > observability/promtail-release.yaml

flux create hr loki \
    --source=HelmRepository/grafana-charts \
    --chart=loki \
    --chart-version=">=2.13.0 <2.13.0" \
    --target-namespace=observability \
    --create-target-namespace=false \
    --export > observability/loki-release.yaml

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization infrastructure
flux get all
```
</details>

## Applications Deployment with Flux2

Now, we will finally setup Flux2 as GitOps tool to provision cloud-native applications.

**Lab Instructions**
1. Add another Flux2 Kustomization for the `applications/gorilla/cne01` folder, that depends on the _infrastructure_ Kustomization

<details>
  <summary markdown="span">Click to expand solution ...</summary>
  
  ```bash
# you may need to update and modify Flux kustomization
# - applications-sync.yaml
# - image-update-automation.yaml

flux create kustomization applications \
    --depends-on=infrastructure
    --source=GitRepository/flux-system \
    --path="./applications/gorilla/cne01"
    --prune=true \
    --interval=5m0s \
    --export > ./clusters/gorilla/cne01/applications-sync.yaml

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization applications
flux get all
  ```
</details>

### Pod Info Application Deployment

Podinfo is a tiny web application made with Go that showcases best practices of running microservices in Kubernetes. Podinfo is used by CNCF projects like Flux and Flagger for end-to-end testing and workshops. 
In this lab we will deploy [Podinfo](https://github.com/stefanprodan/podinfo) using the Cloudkoffer Gitops workflow.

**Lab Instructions**
1. Read the installation instructions at https://github.com/stefanprodan/podinfo
2. Install the Podinfo application into the default namespace either as Helm chart or Kustomize
    - Patch the Podinfo deployment and set `replicas: 3`
    - Patch the PodInfo HPA and set `minReplicas: 3`
    - Patch the PodInfo Service and set `type: LoadBalancer`
3. (_optional_) Setup the image update automation workflow with suitable image repository and policy

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
cd applications/gorilla/cne01

flux create source git podinfo \
    --url=https://github.com/stefanprodan/podinfo \
    --tag="6.1.8" \
    --interval=30s \
    --export > podinfo/podinfo-source.yaml

flux create kustomization podinfo \
    --source=GitRepository/podinfo \
    --path="./kustomize" \
    --prune=true \
    --interval=5m0s \
    --target-namespace=default \
    --export > podinfo/podinfo-kustomization.yaml
```

The Kustomize patches need to be added manually to the `podinfo-kustomization.yaml`.
```yaml
  images:
    - name: ghcr.io/stefanprodan/podinfo
      newName: ghcr.io/stefanprodan/podinfo # {"$imagepolicy": "flux-system:podinfo:name"}
      newTag: 6.1.8 # {"$imagepolicy": "flux-system:podinfo:tag"}
  patchesStrategicMerge:
    - apiVersion: autoscaling/v2beta2
      kind: HorizontalPodAutoscaler
      metadata:
        name: podinfo
      spec:
        minReplicas: 3
    - apiVersion: apps/v1
      kind: Deployment
      metadata:
        name: podinfo
        labels:
          lab: cloud-native-explab
      spec:
        replicas: 3
        template:
          metadata:
            labels:
              lab: cloud-native-explab
    - apiVersion: v1
      kind: Service
      metadata:
        name: podinfo
      spec:
        type: LoadBalancer
```

Then add and configure image repository and policy for the image update automation to work.
```bash
flux create image repository podinfo \
    --image=ghcr.io/stefanprodan/podinfo \
    --interval 1m0s \
    --export > podinfo/podinfo-registry.yaml

flux create image policy podinfo \
    --image-ref=podinfo \
    --select-semver="6.1.x" \
    --export > podinfo/podinfo-policy.yaml
```

### Cloud-native Weather Showcase Deployment

Currently, several implementations of the Cloud-native weather service implementation are available, including 
a SPA that serves as a frontend. Installation instructions can be found in the individual repositories:

- [Cloud-native Weather Service with Golang](https://github.com/qaware/cloud-native-weather-golang/blob/main/docs/README.md)
- [Cloud-native Weather UI with Vue.js](https://github.com/qaware/cloud-native-weather-vue3/blob/main/docs/README.md)
- [Cloud-native Weather Service with .NET Core](https://github.com/qaware/cloud-native-weather-dotnet/blob/main/docs/README.md)
- [Cloud-native Weather Service with JavaEE](https://github.com/qaware/cloud-native-weather-javaee/blob/main/docs/README.md)
- [Cloud-native Weather Service with Node.js](https://github.com/qaware/cloud-native-weather-nodejs/blob/main/docs/README.md)
- [Cloud-native Weather Service with Spring](https://github.com/qaware/cloud-native-weather-spring/blob/main/docs/README.md)
