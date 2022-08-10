# Cluster API with GCP Lab

This guide is based on the following tutorials:

- https://cluster-api.sigs.k8s.io/user/quick-start.html
- https://github.com/kubernetes-sigs/cluster-api-provider-gcp/blob/main/docs/book/src/topics/prerequisites.md
- https://kube.academy/courses/cluster-api/lessons/cluster-api-introduction

The guide is still work in progress.

## Prerequisites
- gcloud CLI
- Flux CLI
- Cluster API CLI
- Kubectl
- make


## Use pre configured docker image shell

The repository contains a preconfigured docker image containing all tools and files needed.

To build the docker image navigate to `cloud-native-explab/cluster-api/gcp` and run:
```bash
cloud-native-explab/cluster-api/gcp$ bash ./buildDockerContainer.sh`
````
This build the docker image `explab-gcp-shell`

Start the container:
```bash
docker run -it explab-gcp-shell
````

## Setup 

### Environment variables

The `.env` file in `cloud-native-explab/cluster-api/gcp` contains all necessary configuration properties:
```bash
GCP_REGION=europe-west1
GCP_PROJECT=cloud-native-experience-lab
GCP_ZONE=europe-west1-b
KUBERNETES_VERSION=1.22.8
GCP_CONTROL_PLANE_MACHINE_TYPE=n1-standard-2
GCP_NODE_MACHINE_TYPE=n1-standard-2
GCP_NETWORK_NAME=default
CLUSTER_NAME=<cluster-name>
IMAGE_ID=<gke-image>
GCP_PROJECT_ID=cloud-native-experience-lab
GITHUB_TOKEN=<token>
```
Set all arguments:
```bash
export $(cat .env | xargs)
```

### Login with the google cli

```bash
gcloud auth login
```

If you use the docker shell, follow the prompted instructions to complete the login with the browser on the host machine. 

### Create Router and NAT

```bash
# Create a router
gcloud compute routers create "${CLUSTER_NAME}-router" --project="${GCP_PROJECT}" \
    --region="${GCP_REGION}" --network="default"

# Create a nat
gcloud compute routers nats create "${CLUSTER_NAME}-nat" --project="${GCP_PROJECT}" \
    --router-region="${GCP_REGION}" --router="${CLUSTER_NAME}-router" \
    --nat-all-subnet-ip-ranges --auto-allocate-nat-external-ips
```

### Create a Service Account

To create new workload clusters a Service account with `Editor` permissions is needed.
```bash
# Create a new service account:
gcloud iam service-accounts create <service-acc> --display-name="Service Account"

# Add the service account to the project with `Editor` permission:
gcloud projects add-iam-policy-binding <project-id> \
  --member="serviceAccount:<service-acc>@<project-id>.iam.gserviceaccount.com" --role="roles/editor"
  
#Create and download credentials:
gcloud iam service-accounts keys create </path/to/serviceaccount-key.json>  \
  --iam-account=<service-acc>@${GCP_PROJECT_ID}.iam.gserviceaccount.co

# Set the path to the file containing the key:
export GOOGLE_APPLICATION_CREDENTIALS=</path/to/serviceaccount-key.json>

# Set the credentials Base64 encoded
export GCP_B64ENCODED_CREDENTIALS=$( cat /path/to/gcp-credentials.json | base64 | tr -d '\n' )
```

### Create the management Cluster

There are two make targets to create new gke clusters:
```bash
# prepare cluster settings
make prepare-gke-cluster
# provision the management cluster
make create-gke-cluster
```

Install DNI
```bash
# apply calico
kubectl apply -f https://docs.projectcalico.org/v3.21/manifests/calico.yaml
```

Finally, initialize the management cluster with the gcp provider:
```bash
clusterctl init --infrastructure gcp
 ```

Important commands 
```bash
# Get infos for the management cluster
kubectl cluster-info 

# List the namespaces
kubectl get ns    

# List of nodes in the management cluster
kubectl get nodes 

# List of workload clusters
kubectl get clusters

# List of machines in the workload clusters
kubectl get machines

# Get infos for a workload cluster
kubectl describe cluster <workload-cluster>

 ```

## Generate a sample workload cluster
The management cluster should now be ready to create new workload clusters.

TODO: This step does not succeed!

```bash 
# Option 1: create a new workload cluster definition via cmd options
clusterctl generate cluster capi-quickstart \ 
    --kubernetes-version v1.22.8   --control-plane-machine-count=1  \
    --worker-machine-count=1  > capi-quickstart.yaml
    
# Option 2: create a new workload cluster definition via config file
clusterctl config cluster capi-quickstart --config ./clusterctl.yaml > capi-quickstart.yaml

# create the cluster object
kubectl apply -f capi-quickstart.yaml
```

The cluster now gets provisioned:

```bash 
kubectl get clusters
 ```

But the workload clusters are not successfully initialized:

```bash 
kubectl get machines 

NAME                                    CLUSTER            NODENAME   PROVIDERID   PHASE          AGE     VERSION
capi-quickstart-control-plane-6g8s4     capi-quickstart                            Provisioning   3h45m   v1.22.8
capi-quickstart-md-0-66cbb56b9c-w67k4   capi-quickstart                            Pending        3h45m   v1.22.8
```

## Next steps
The next step would be to deploy a CNI solution on the workload cluster otherwise it will not become ready:

```bash 
# get the kubeconfig for the workload cluster
clusterctl get kubeconfig capi-quickstart > capi-quickstart.kubeconfig

# use the workload cluster and apply calcio
kubectl --kubeconfig=./capi-quickstart.kubeconfig \
    apply -f https://docs.projectcalico.org/v3.21/manifests/calico.yaml

# get the list of workload cluster nodes
kubectl --kubeconfig=./capi-quickstart.kubeconfig get nodes
```

## Clean Up
```bash 
# Delete the workload cluster object
kubectl delete cluster capi-quickstart

# Delete the mangament cluster
make delete-gke-cluster

# Delete nat
gcloud compute routers nats delete "${CLUSTER_NAME}-nat" --project="${GCP_PROJECT}" \
 --router-region="${GCP_REGION}" --router="${CLUSTER_NAME}-router" --quiet || true 
 
# Delete router
gcloud compute routers delete "${CLUSTER_NAME}-router" --project="${GCP_PROJECT}" \
--region="${GCP_REGION}" --quiet || true
```

## Installing GKE image
There is no official image for worker cluster nodes yet, hence it must be build and pushed manually.

IMPORTANT: This does not work within docker! TODO: find out why

### Prerequisites
 - Python
 - Ansible 
 - Packer
 - an existing service account

### Setup environment
Make sure
```bash 
export GOOGLE_APPLICATION_CREDENTIALS=</path/to/serviceaccount-key.json>
export GCP_PROJECT_ID=<project-id>
```
are set 

### Prepare the image builder
Download the Image Builder repository
```bash 
git clone https://github.com/kubernetes-sigs/image-builder.git image-builder

cd image-builder/images/capi
```

The image will be build for Zone `us` by default with a static kubernetes version. 

_Note: There might be an existing configuration option._

To change the zone or kubernetes version modify `packer/gce/ubuntu-2004.json` like below:
```json
{
  "build_name": "ubuntu-2004",
  "distribution_release": "focal",
  "distribution_version": "2004",
  "zone": "europe-west1-b",
  "kubernetes_deb_version": "1.22.8-00",
  "kubernetes_rpm_version": "1.22.8-0",
  "kubernetes_semver": "v1.22.8",
  "kubernetes_series": "v1.22"
}
```

### Build and publish the image:
```bash 
# Build and push GCE imapes to the google repository
make build-gce-ubuntu-1804
make build-gce-ubuntu-2004` 

# Check whether the image is availabe
gcloud compute images list --project ${GCP_PROJECT_ID} --no-standard-images \
--filter="family:capi-ubuntu-1804-k8s"

# The output will look like below: 
NAME                                        PROJECT                      FAMILY                      DEPRECATED  STATUS
cluster-api-ubuntu-2004-v1-22-8-1653399314  cloud-native-experience-lab  capi-ubuntu-2004-k8s-v1-22              READY
cluster-api-ubuntu-2004-v1-22-9-1653382327  cloud-native-experience-lab  capi-ubuntu-2004-k8s-v1-22              READY

# Set the Image id
export IMAGE_ID=cluster-api-ubuntu-2004-v1-22-8-1653399314
```