# Cluster API with GCP Lab

This lab describes how to spin up GKE clusters using Cluster API.

## Installation

You need to have `clusterctl`, `gcloud`, `Packer` and `Ansible` installed locally for this lab. Alternativly, use the `cn-explab-shell` Docker image for a consistent local CLI tool setup.

## Setup a Network and Cloud NAT

If the Google project is freshly setup than you need to configure the `default` network appropriately.

```bash
# either call make target
make prepare-cloud-nat

# or do it manually
export GCP_PROJECT=cloud-native-experience-lab
export GCP_REGION=europe-west1
export GCP_NETWORK_NAME=default
export CLUSTER_NAME=capi-mgmt-cluster

gcloud compute routers create $CLUSTER_NAME-router --project=$GCP_PROJECT --region=$GCP_REGION --network=$GCP_NETWORK_NAME
gcloud compute routers nats create $CLUSTER_NAME-nat --project=$GCP_PROJECT --router-region=$GCP_REGION --router=$CLUSTER_NAME-router --nat-all-subnet-ip-ranges --auto-allocate-nat-external-ips
```

## Create a Service Account

```bash
# either call make target
make create-sa

# or do it manually
export GCP_PROJECT=cloud-native-experience-lab
export CLUSTER_NAME=capi-mgmt-cluster

gcloud iam service-accounts create $CLUSTER_NAME --description="$CLUSTER_NAME Service Account" --display-name="$CLUSTER_NAME Service Account"
gcloud projects add-iam-policy-binding $GCP_PROJECT --role=roles/editor --member=serviceAccount:$CLUSTER_NAME@$GCP_PROJECT.iam.gserviceaccount.com
gcloud iam service-accounts keys create gke-sa-key.json --iam-account=$CLUSTER_NAME@$GCP_PROJECT.iam.gserviceaccount.com
```

## Build Worker Node Images

In order to use Cluster API with GCP, specific GCE machines images are required. These need to be built and published for your Google project. You need to have `Packer` and `Ansible` installed locally for this step, or use the `cn-explab-shell` Docker image.

```bash
# either call make target
make create-images

# or do it manually
export GCP_PROJECT=cloud-native-experience-lab
export GOOGLE_APPLICATION_CREDENTIALS=gke-sa-key.json

# checkout and build all images
git clone https://github.com/kubernetes-sigs/image-builder.git image-builder
cd image-builder/images/capi && make build-gce-all

# Check that you can see the published images
gcloud compute images list --project $GCP_PROJECT_ID --no-standard-images
```

The output of the last command should list all the available and published GCE images.

```markdown
NAME                                         PROJECT                      FAMILY                      DEPRECATED  STATUS
cluster-api-ubuntu-1804-v1-23-10-1666653376  cloud-native-experience-lab  capi-ubuntu-1804-k8s-v1-23              READY
cluster-api-ubuntu-2004-v1-22-8-1653399314   cloud-native-experience-lab  capi-ubuntu-2004-k8s-v1-22              READY
cluster-api-ubuntu-2004-v1-22-9-1653382327   cloud-native-experience-lab  capi-ubuntu-2004-k8s-v1-22              READY
cluster-api-ubuntu-2004-v1-23-10-1666654232  cloud-native-experience-lab  capi-ubuntu-2004-k8s-v1-23              READY
```

Pick an image of your choice, depending the the Kubernetes version you want to support. Attention: the Kubernetes version you
choose here, needs to be consistent with the CAPI all tenant clusters versions.

```bash
# Export the IMAGE_ID using one of the GCE names from above
# The IMAGE_ID format needs to be exactly as specified, otherwise machine creation will fail later on!!!
export IMAGE_ID=projects/$GCP_PROJECT_ID/global/images/cluster-api-ubuntu-2004-v1-22-9-1653382327
```

## Create a CAPI Management Cluster

In order to use the Cluster API you need to create a dedicated management cluster that is the mother control plane for
all child K8s tenant clusters. Optionally, we want the cluster to be fully GitOps integrated.

```bash
export KUBERNETES_VERSION=1.22.15
export CLUSTER_NAME=capi-mgmt-cluster

# either call make targets
make create-capi-cluster
make bootstrap-capi-cluster
make bootstrap-capi-flux2

# or do it manually
gcloud container clusters create $CLUSTER_NAME --num-nodes=3 --enable-autoscaling --min-nodes=3 --max-nodes=5 --cluster-version=$KUBERNETES_VERSION
kubectl create clusterrolebinding cluster-admin-binding --clusterrole=cluster-admin --user=$$(gcloud config get-value core/account)

# you may need to set a personal GITHUB_TOKEN to avoid API rate limiting
export GCP_B64ENCODED_CREDENTIALS=$(cat gke-sa-key.json | base64 | tr -d '\n' )
clusterctl init --infrastructure gcp

# may need to add --personal if the GITHUB_USER is not an org
# you may need to set a personal GITHUB_TOKEN to avoid API rate limiting
flux bootstrap github \
    --owner=$GITHUB_USER \
    --repository=cloud-native-explab \
    --branch=main \
    --path=./clusters/gcp/capi-mgmt-cluster \
    --components-extra=image-reflector-controller,image-automation-controller \
    --read-write-key
```

## Create a CAPI Tenant Cluster

Using on the CAPI management cluster, further tenant cluster can be spawned easily. Essentially, only the required `Cluster`, `GCPCluster`, `KubeadmControlPlane`, `GCPMachineTemplate` and `MachineDeployment` resources need to be created.

```bash
export KUBERNETES_VERSION=1.22.15

# to get a list of variables
clusterctl generate cluster capi-tenant-demo --infrastructure=gcp --list-variables

# create and apply the CAPI tenant manifests
clusterctl generate cluster capi-tenant-demo --kubernetes-version $KUBERNETES_VERSION --control-plane-machine-count=1 --worker-machine-count=1 > capi-tenant-demo.yaml
kubectl apply -f capi-tenant-demo.yaml
kubectl get cluster 

# monitor the cluster deployment, until the control plane is ready
clusterctl describe cluster capi-tenant-demo 
kubectl get kubeadmcontrolplane

# obtain kube.config for tenant cluster and install CNI
clusterctl get kubeconfig capi-tenant-demo > capi-tenant-demo.kubeconfig
kubectl --kubeconfig=./capi-tenant-demo.kubeconfig apply -f https://raw.githubusercontent.com/projectcalico/calico/v3.24.1/manifests/calico.yaml
kubectl --kubeconfig=./capi-tenant-demo.kubeconfig get nodes
```

## Delete a CAPI Tenant Cluster

> :warning: In order to ensure a proper cleanup of your infrastructure you must always delete the cluster object. Deleting the entire cluster template with `kubectl delete -f capi-tenant-demo.yaml` might lead to pending resources to be cleaned up manually.

```bash
# delete the root CAPI cluster resource
kubectl delete cluster capi-tenant-demo
```

## Further References

All the steps are also kind of documented in the following resources:
- https://github.com/kubernetes-sigs/cluster-api-provider-gcp/blob/main/docs/book/src/topics/prerequisites.md
- https://cluster-api.sigs.k8s.io/user/quick-start.html
