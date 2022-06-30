# Cloud-native Experience Lab (for Platform Engineers)

This experience lab puts its focus on cloud-native platform engineers. No matter which implementation language is used, the conceptual items are very similar.

![Platform Architecture](architecture.png)

## Prerequisites

- Local Docker and Kubernetes installation
    - Docker Desktop (https://docs.docker.com/get-docker/)
    - Rancher Desktop (https://docs.rancherdesktop.io/getting-started/installation)
    - Minikube (https://minikube.sigs.k8s.io/docs/start/)
- Kustomize CLI (https://kubectl.docs.kubernetes.io/installation/kustomize/)
- Flux2 CLI (https://fluxcd.io/docs/installation/)

## Cluster API

```
$ kubectl get clusters
$ kubectl get kubeadmcontrolplane
$ clusterctl describe cluster capi-tenant-lreimer
$ kubectl --kubeconfig=capi-tenant-lreimer.kubeconfig get nodes
```


## GitOps with Flux

In this section we want to play around with FluxCD. Go to https://github.com/lreimer/hands-on-flux2

## Security

- Sealed Secrets
- Trivy Operator
- Gatekeeper


## Crossplane
_TODO:_

## Diagnosability
_TODO:_