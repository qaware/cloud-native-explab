# Giant Swarm Managed Kubernetes Platform (on AWS)

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


## Giant Swarm Workload Cluster Setup

First, we need to obtain access to our personal Giant Swarm workload cluster. The cluster is GitOps managed by this
repository. Scale your cluster to a desired minimum size of 3.

**Lab Instructions**

1. Install the Flux2 CLI on your developer machine, if not already done
2. Fork the GitHub repository `https://github.com/qaware/cloud-native-explab` if not already done
3. Modify the `AWSMachineDeployment` to have a minimum node pool size of 3
4. Issue a pull request with your changes (or manually apply the changes?)

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```yaml
apiVersion: infrastructure.giantswarm.io/v1alpha3
kind: AWSMachineDeployment
metadata:
  annotations:
    giantswarm.io/docs: https://docs.giantswarm.io/ui-api/management-api/crd/awsmachinedeployments.infrastructure.giantswarm.io/
  labels:
    cluster.x-k8s.io/cluster-name: ${cluster_id}
    giantswarm.io/cluster: ${cluster_id}
    giantswarm.io/machine-deployment: ${machine_deployment_id}
    giantswarm.io/organization: ${organization}
    release.giantswarm.io/version: ${release}
  name: ${machine_deployment_id}
  namespace: org-${organization}
spec:
  nodePool:
    description: Default node pool
    scaling:
      max: 5
      # set minimum size
      min: 3
```

</details>

## Platform Bootstrapping with Flux2

Next, we will bootstrap Flux2 as GitOps tool to provision further infrastructure and platform components.

**Lab Instructions**

1. Install the Flux2 CLI on your developer machine, if not already done
2. Create personal Github token and export as ENV variable
3. Add Flux2 Kustomization for platform `infrastructure/` folder
4. (_Bonus_) Bootstrap the flux-system namespace and components locally on your machine
    - use a personal repository as GitOps repository
    - (_optional_) enable extra components: _image-reflector-controller_ and _image-automation-controller_
    - create a read-write SSH key



<details>
  <summary markdown="span">Click to expand solution ...</summary>
  
  ```bash
# install the Flux2 CLI on the master node
# see https://fluxcd.io/docs/installation/
curl -s https://fluxcd.io/install.sh | sudo bash

# see https://fluxcd.io/docs/get-started/
# generate a personal Github token
export GITHUB_USER=<your-github-user>
export GITHUB_REPO=cloud-native-explab
export GITHUB_TOKEN=<your-github-token>

# bootstrap the flux-system namespace and components
# only do this if Flux is not installed on the cluster yet
flux bootstrap github \
    --owner=$GITHUB_USER \
    --repository=$GITHUB_REPO \
    --branch=main \
    --path=./clusters/gorilla/cne01 \
    --components-extra=image-reflector-controller,image-automation-controller \
    --read-write-key
    --personal         # only for user accounts, not for org accounts

# you may register a dedicated 
flux create source git $GIT_REPO \
    --url=https://github.com/$GIT_USER/$GIT_REPO \
    --branch=main \
    --interval=5m \
    --export > ./clusters/gorilla/cne01/$GIT_REPO-source.yaml

# you may need to update and modify Flux kustomization
# - infrastructure-sync.yaml

flux create kustomization infrastructure \
    --source=GitRepository/flux-system \
    --path="./infrastructure/gorilla/cne01"
    --prune=true \
    --interval=5m0s \
    --export > ./clusters/gorilla/cne01/infrastructure-sync.yaml

# manually apply the source and infrastructure manifests
kubectl apply ./clusters/gorilla/cne01/$GIT_REPO-source.yaml
kubectl apply ./clusters/gorilla/cne01/infrastructure-sync.yaml

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

### Kubernetes Dashboard

The Kubernetes dashboard has not been installed as a GKE addon. Instead, we install the dashboard manually in the
current version. Since RBAC is enabled we also need to make a few additional steps are required.

**Lab Instructions**

1. Deploy the Kubernetes Dashboard as YAML from the upstream repository
2. Create service account and cluster role binding using Flux2
3. Expose the dashboard UI as _LoadBalancer_ service or using an _Ingress_ resource
4. Generate user token and access dashboard UI

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
# change into infrastructure/ directory
take kubernetes-dashboard
kustomize create
```

Now you can add the latest recommend dashboard manifest YAML to the resources section of the kustomization.yaml
e.g. https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml

```yaml
# see https://github.com/kubernetes/dashboard/blob/master/docs/user/access-control/creating-sample-user.md
# create admin-service-account.yaml in the GitOps infrastructure directory
apiVersion: v1
kind: ServiceAccount
metadata:
    name: admin-user
    namespace: kube-system
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
    name: admin-user
roleRef:
    apiGroup: rbac.authorization.k8s.io
    kind: ClusterRole
    name: cluster-admin
subjects:
    - kind: ServiceAccount
      name: admin-user
      namespace: kube-system
```

Now you can open and access the dashboard in your preferred browser. You could either use port-forwarding or the proxy
functionality of kubectl.

```bash
# using the proxy
kubectl proxy
open http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/

# or use port forward
kubectl port-forward -n kube-system service/kubernetes-dashboard 10443:443
```

Even better is to patch the `kubernetes-dashboard` service using type `LoadBalancer` and apply it as strategic
merge patch using Kustomize.

```yaml
# create loadbalancer.yaml in the GitOps repository
apiVersion: v1
kind: Service
metadata:
  name: kubernetes-dashboard
  namespace: kubernetes-dashboard
spec:
  type: LoadBalancer

# add this to the kustomize.yaml
patchesStrategicMerge:
  - loadbalancer.yaml
```

</details>

### Bitnami Sealed Secrets

In this step we want to provision the [Sealed Secrets](https://github.com/bitnami-labs/sealed-secrets) controller as 
a Helm release using Flux.

**Lab Instructions**

1. Create a `HelmRepository` resource for the official Sealed Secrets chart repository 
2. Create a `HelmRelease` resource for the actual chart to install
3. Add resource to your GitOps repo and synchronize the cluster state

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
# create a dedicated directory with initial Kustomization
take sealed-secrets
kustomize create

flux create source helm sealed-secrets \
    --url=https://bitnami-labs.github.io/sealed-secrets \
    --interval=10m0s \
    --export > repository.yaml

flux create hr sealed-secrets \
    --source=HelmRepository/sealed-secrets \
    --chart=sealed-secrets \
    --release-name=sealed-secrets-controller \
    --chart-version=">=1.18.1" \
    --target-namespace=kube-system \
    --create-target-namespace=false \
    --crds=CreateReplace \
    --export > release.yaml
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
</details>

### Cloud-native Weather Showcase Deployment

Currently, several implementations of the Cloud-native weather service implementation are available, including 
a SPA that serves as a frontend. Installation instructions can be found in the individual repositories:

- [Cloud-native Weather Service with Golang](https://github.com/qaware/cloud-native-weather-golang/blob/main/docs/README.md)
- [Cloud-native Weather UI with Vue.js](https://github.com/qaware/cloud-native-weather-vue3/blob/main/docs/README.md)
- [Cloud-native Weather Service with .NET Core](https://github.com/qaware/cloud-native-weather-dotnet/blob/main/docs/README.md)
- [Cloud-native Weather Service with JavaEE](https://github.com/qaware/cloud-native-weather-javaee/blob/main/docs/README.md)
- [Cloud-native Weather Service with Node.js](https://github.com/qaware/cloud-native-weather-nodejs/blob/main/docs/README.md)
- [Cloud-native Weather Service with Spring](https://github.com/qaware/cloud-native-weather-spring/blob/main/docs/README.md)
