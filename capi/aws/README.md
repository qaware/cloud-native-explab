# Cluster API with AWS Lab

This lab describes how to spin up GKE clusters using Cluster API.

## Installation

You need to have `clusterctl`, `clusterawsadm`, and `awscli` installed locally for this lab. Alternativly, use the `cn-explab-shell` Docker image for a consistent local CLI tool setup.

## Create a CAPI Management Cluster

In order to use the Cluster API you need to create a dedicated management cluster that is the mother control plane for all child K8s tenant clusters. Optionally, we want the cluster to be fully GitOps integrated.

```bash
# either call make targets
# make sure AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY are set locally!!!
export AWS_REGION=eu-central-1

make prepare-capi-aws
make bootstrap-capi-cluster
make bootstrap-capi-flux2

# or do it manually
# configure the AWS account for CAPI
export AWS_REGION=$(AWS_REGION)
export AWS_ACCESS_KEY_ID=<INSERT>
export AWS_SECRET_ACCESS_KEY=<INSERT>
clusterawsadm bootstrap iam create-cloudformation-stack --config bootstrap-config.yaml

# you may need to set a personal GITHUB_TOKEN to avoid API rate limiting
export AWS_REGION=eu-central-1
export AWS_SSH_KEY_NAME=capi-default
export AWS_CONTROL_PLANE_MACHINE_TYPE=t3.medium
export AWS_NODE_MACHINE_TYPE=t3.medium
export AWS_B64ENCODED_CREDENTIALS=$(shell clusterawsadm bootstrap credentials encode-as-profile)
clusterctl init --infrastructure aws

# may need to add --personal if the GITHUB_USER is not an org
# you may need to set a personal GITHUB_TOKEN to avoid API rate limiting
flux bootstrap github \
    --owner=$GITHUB_USER \
    --repository=cloud-native-explab \
    --branch=main \
    --path=./clusters/aws/capi-mgmt-cluster \
    --components-extra=image-reflector-controller,image-automation-controller \
    --read-write-key
```

## Create a CAPI Tenant Cluster

Using on the CAPI management cluster, further tenant cluster can be spawned easily. Essentially, only the required `Cluster`, `AWSCluster`, `KubeadmControlPlane`, `AWSMachineTemplate` and `MachineDeployment` resources need to be created.

```bash
export KUBERNETES_VERSION=1.22.15
export AWS_REGION=eu-central-1
export AWS_SSH_KEY_NAME=capi-default
export AWS_CONTROL_PLANE_MACHINE_TYPE=t3.medium
export AWS_NODE_MACHINE_TYPE=t3.medium

# to get a list of variables
clusterctl generate cluster capi-tenant-demo --infrastructure=aws --list-variables

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
- https://cluster-api.sigs.k8s.io/user/quick-start.html
