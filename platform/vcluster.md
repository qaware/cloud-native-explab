# vcluster based multi-tenant platforms on GKE

This lab will provision multi-tenant Kubernetes platforms using vcluster on GKE.
It will also show how to provision usefulinfrastructure components as well
as a demo application.

## Prerequisites

Before you dive right into this experience lab, make sure your local environment is setup properly. Alternatively, use the preconfigured `cn-explab-shell` Docker image.

- Modern Operating System (Windows 10, MacOS, ...) with terminal and shell
- IDE of your personal choice (with relevant plugins installed), e.g. VS Code or IntelliJ
- [gcloud](https://cloud.google.com/sdk/docs/install)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Flux2](https://fluxcd.io/flux/cmd/)
- [Kustomize](https://kustomize.io)
- [vcluster](https://www.vcluster.com/docs/getting-started/setup)

## GKE Cluster Setup

In this initial step we will create the GKE cluster using the official Google `gcloud` CLI. We will also
install and enable additional built-in addons.

**Lab Instructions**

1. Configure your local `gcloud` configuration to use the `cloud-native-experience-lab` project and `europe-west1-b` as compute zone.
2. Create a GKE cluster with the following settings and properties:
   - Kubernetes version 1.26
   - Regional cluster in `europe-north1`
   - 1 to 10 nodes using auto scaling and machine type `e2-standard-8`
   - Logging and monitoring enabled for SYSTEM scope
   - Workload pool identity enabled
   - Addons: HttpLoadBalancing, HorizontalPodAutoscaling, ConfigConnector
3. Create a GCP service account for the cluster and add it to the `roles/editor` IAM role
4. Add a policy binding to the service account for the workload identity member with the `roles/iam.workloadIdentityUser` IAM role

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
# or do it manually to better unstand the steps and commands
# see https://cloud.google.com/sdk/gcloud/reference/container/clusters/create
export GCP_PROJECT=cloud-native-experience-lab
export GCP_REGION=europe-north1
export GCP_ZONE=europe-north1-b
export CLUSTER_NAME=cloud-native-explab

gcloud config set project $GCP_PROJECT
gcloud config set compute/region $GCP_REGION
gcloud config set compute/zone $GCP_ZONE
gcloud config set container/use_client_certificate False

gcloud container clusters create $CLUSTER_NAME  \
        --release-channel=regular \
		--cluster-version=1.26 \
  		--region=$(GCP_REGION) \ 
        --addons HttpLoadBalancing,HorizontalPodAutoscaling,ConfigConnector \
        --workload-pool=$GCP_PROJECT.svc.id.goog \
        --enable-autoscaling \
        --autoscaling-profile=optimize-utilization \
        --num-nodes=1 \
        --min-nodes=1 --max-nodes=10 \
        --machine-type=e2-standard-8 \
        --logging=SYSTEM \
        --monitoring=SYSTEM

kubectl create clusterrolebinding cluster-admin-binding --clusterrole=cluster-admin --user=`gcloud config get-value core/account`

# for the ConfigConnector plugin we need to create a SA with correct permissions
gcloud iam service-accounts create $CLUSTER_NAME --description="$CLUSTER_NAME Service Account" --display-name="$CLUSTER_NAME Service Account"

gcloud projects add-iam-policy-binding $GCP_PROJECT  \
        --role=roles/editor  \
        --member=serviceAccount:$CLUSTER_NAME@$GCP_PROJECT.iam.gserviceaccount.com

gcloud iam service-accounts add-iam-policy-binding $CLUSTER_NAME@$GCP_PROJECT.iam.gserviceaccount.com \
        --member="serviceAccount:$GCP_PROJECT.svc.id.goog[cnrm-system/cnrm-controller-manager]" \
        --role="roles/iam.workloadIdentityUser"
```

</details>


## vCluster Tenant Setup

In this initial step we will install and bootstrap a vCluster based tenant cluster with custom configuration.

**Lab Instructions**

1. Install the vCluster CLI as described in https://www.vcluster.com/docs/getting-started/setup
2. Deploy a vCluster tenant cluster with the name `tenant-00`
    - Proxy the metrics from nodes and pods from the root server
    - Enable synchronization of all nodes
    - Enable ingress synchronization
    - Enable service account synchronation
3. Make sure the vCluster is running by listing all instances
3. Connect to created vCluster tenant instance

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
# create the vcluster tenant instance
vcluster create tenant-00 --expose=true --connect=false --values=tools/vcluster-values.yaml
vcluster list

vcluster connect tenant-00
kubectl get namespaces

vcluster connect tenant-00 --update-current=false --kube-config=kubeconfig/tenant-00.yaml
kubectl --kubeconfig kubeconfig/tenant-09.yaml get namespaces

# or export the custom kubeconfig
export KUBECONFIG=kubeconfig/tenant-00.yaml
```

</detals>


## Tenant Bootstrapping with Flux2

In this step we bootstrap Flux2 as GitOps tool to provision the tenenant cluster with its infrastracture and platform and application components.

**Lab Instructions**

1. Bootstrap Flux using this repository as source
    - Add following extra components: `image-reflector-controller` and `image-automation-controller`
    - Create a read / write key for Flux, so that Flux can make manifest changes
2. Configure additional kustomizations for infrastructure and applications components

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
# see https://fluxcd.io/docs/get-started/
# generate a personal Github token
export GITHUB_USER=qaware
export GITHUB_TOKEN=<your-token>
export VCLUSTER_NAME=tenant-01

# bootstrap the flux-system namespace and components
flux bootstrap github \
    --owner=$GITHUB_USER \
    --repository=cloud-native-explab \
    --branch=main \
    --path=./clusters/gcp/$(CLUSTER_NAME)/$(VCLUSTER_NAME) \
    --components-extra=image-reflector-controller,image-automation-controller \
    --read-write-key \
    --personal          # only for user accounts, not for org accounts

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization infrastructure
flux reconcile kustomization applications

# you may need to update and modify Flux kustomization
# - infrastructure-sync.yaml
# - applications-sync.yaml

# to automatically trigger the GitOps process 
# you also need to create or update the webhooks for the Git Repository
# Payload URL: http://<LoadBalancerAddress>/<ReceiverURL>
# Secret: the webhook-token value
$ kubectl -n flux-system get svc/receiver
$ kubectl -n flux-system get receiver/webapp
```

</details>
