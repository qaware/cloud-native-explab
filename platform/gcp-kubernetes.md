# GKE based Kubernetes native Platform

This lab is builds an opinionated K8s-native platform based on Google Kubernetes Engine.

- gcloud CLI
- Google Kubernetes Engine (GKE)
- Flux2 CLI

## GKE Cluster Setup

In this initial step we will create the GKE cluster using the official Google `gcloud` CLI. We will also
install and enabler the ConfigConnector add-on to be able to provision GCP infrastructure via K8s resources.

```bash
# for the unpatient, use the provided Make targets
make create-gcp-cluster
make create-gcp-sa

# or do it manually to better unstand the steps and commands
# see https://cloud.google.com/sdk/gcloud/reference/container/clusters/create
export GCP_PROJECT=cloud-native-experience-lab
export GCP_ZONE=europe-west1-b
export CLUSTER_NAME=cloud-native-explab

gcloud config set project $GCP_PROJECT
gcloud config set compute/zone $GCP_ZONE
gcloud config set container/use_client_certificate False

gcloud container clusters create $CLUSTER_NAME  \
        --addons HttpLoadBalancing,HorizontalPodAutoscaling,ConfigConnector \
        --workload-pool=$GCP_PROJECT.svc.id.goog \
        --num-nodes=3 \
        --enable-autoscaling \
        --min-nodes=3 --max-nodes=10 \
        --machine-type=e2-standard-4 \
        --logging=SYSTEM \
        --monitoring=SYSTEM \
        --cluster-version=1.22
kubectl create clusterrolebinding cluster-admin-binding --clusterrole=cluster-admin --user=`gcloud config get-value core/account`

# for the ConfigConnector plugin we need to create a SA with correct permissions
gcloud iam service-accounts create $CLUSTER_NAME --description="$CLUSTER_NAME Service Account" --display-name="$CLUSTER_NAME Service Account"
gcloud projects add-iam-policy-binding $GCP_PROJECT  \
        --role=roles/editor  \
        --member=serviceAccount:$CLUSTER_NAME@$GCP_PROJECT.iam.gserviceaccount.com
gcloud iam service-accounts add-iam-policy-binding $CLUSTER_NAME@$GCP_PROJECT.iam.gserviceaccount.com \
        --member="serviceAccount:$GCP_PROJECT.svc.id.goog[cnrm-system/cnrm-controller-manager]" \
        --role="roles/iam.workloadIdentityUser"
gcloud iam service-accounts keys create gke-sa-key.json --iam-account=$CLUSTER_NAME@$GCP_PROJECT.iam.gserviceaccount.com
```

## Platform Bootstrapping with Flux2

In this step we bootstrap Flux2 as GitOps tool to provision further infrastructure and platform components as
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
    --path=./clusters/gcp/$CLUSTER_NAME \
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

## Config Connector

_TODO_ 

## External Secrets Management

_TODO_ 

## References

- https://cloud.google.com/sdk/gcloud/reference/container/clusters/create
- https://github.com/GoogleCloudPlatform/k8s-config-connector
- https://cloud.google.com/config-connector/docs/how-to/install-upgrade-uninstall
- https://cloud.google.com/config-connector/docs/reference/overview