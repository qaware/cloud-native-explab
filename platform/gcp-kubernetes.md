# GKE based Kubernetes native Platform

This lab is builds an opinionated K8s-native platform based on Google Kubernetes Engine.

- gcloud CLI
- Google Kubernetes Engine (GKE)
- Flux2 CLI

## GKE Cluster Setup

## Platform Bootstrapping with Flux2

In this lab we use Flux2 as GitOps tool to provision further infrastructure and platform components as
well as a simple weather microservice.

```bash
# install the Flux2 CLI on the master node
# see https://fluxcd.io/docs/installation/
curl -s https://fluxcd.io/install.sh | sudo bash

# see https://fluxcd.io/docs/get-started/
# generate a personal Github token
export GITHUB_USER=qaware
export GITHUB_TOKEN=<your-token>

# bootstrap the flux-system namespace and components
flux bootstrap github \
	--owner=$GITHUB_USER \
    --repository=cloud-native-explab \
    --branch=main \
    --path=./clusters/gcp/cloud-native-explab \
	--components-extra=image-reflector-controller,image-automation-controller \
	--read-write-key
    # --token-auth       # instead of SSH key access, use the Github token instead
  	# --personal         # only for user accounts, not for org accounts

# you may need to update and modify Flux kustomization
# - infrastructure-sync.yaml
# - notification-receiver.yaml
# - receiver-service.yaml
# - webhook-token.yaml
# - applications-sync.yaml
# - image-update-automation.yaml

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization infrastructure
flux reconcile kustomization applications

# to automatically trigger the GitOps process 
# you also need to create or update the webhooks for the Git Repository
# Payload URL: http://<LoadBalancerAddress>/<ReceiverURL>
# Secret: the webhook-token value
$ kubectl -n flux-system get svc/receiver
$ kubectl -n flux-system get receiver/webapp
```
