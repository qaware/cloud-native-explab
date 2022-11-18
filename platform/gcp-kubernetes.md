# GKE based Kubernetes native Platform

This lab will provision a GKE based platform with several useful infrastructure components as well
as a demo application.

## Prerequisites

Before you dive right into this experience lab, make sure your local environment is setup properly.
Alternatively, use the preconfigured `cn-explab-shell` Docker image.

- Modern Operating System (Windows 10, MacOS, ...) with terminal and shell
- IDE of your personal choice (with relevant plugins installed)
  - IntelliJ Ultimate
  - VS Code
- [gcloud](https://cloud.google.com/sdk/docs/install)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Flux2](https://fluxcd.io/flux/cmd/)
- [Kustomize](https://kustomize.io)

## GKE Cluster Setup

In this initial step we will create the GKE cluster using the official Google `gcloud` CLI. We will also
install and enable additional built-in addons.

**Lab Instructions**

1. Configure your local `gcloud` configuration to use the `cloud-native-experience-lab` project and `europe-west1-b` as compute zone.
2. Create a GKE cluster with the following settings and properties:
   - Kubernetes version 1.22
   - 3 to 10 nodes using auto scaling and machine type e2-standard-4
   - Logging and monitoring enabled for SYSTEM scope
   - Network policies enabled
   - Workload pool identity enabled
   - Addons: HttpLoadBalancing, HorizontalPodAutoscaling, ConfigConnector
3. Create a GCP service account for the cluster with the following permissions
   - Add the service account to the `roles/editor` IAM role
   - Add the workload identity service account as member using the `roles/iam.workloadIdentityUser` role

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
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

</details>

## Platform Bootstrapping with Flux2

In this step we bootstrap Flux2 as GitOps tool to provision further infrastructure and platform and application components.

**Lab Instructions**

1. Bootstrap Flux using this repository as source
    - Add following extra components: `image-reflector-controller` and `image-automation-controller`
    - Create a read / write key for Flux, so that Flux can make manifest changes
2. Configure additional kustomizations for infrastructure and applications components
3. (_optional_) Configure webhook notification and image update automation

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
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
    # --personal         # only for user accounts, not for org accounts

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization infrastructure
flux reconcile kustomization applications

# you may need to update and modify Flux kustomization
# - infrastructure-sync.yaml
# - notification-receiver.yaml
# - receiver-service.yaml
# - webhook-token.yaml
# - applications-sync.yaml
# - image-update-automation.yaml

# to automatically trigger the GitOps process 
# you also need to create or update the webhooks for the Git Repository
# Payload URL: http://<LoadBalancerAddress>/<ReceiverURL>
# Secret: the webhook-token value
$ kubectl -n flux-system get svc/receiver
$ kubectl -n flux-system get receiver/webapp
```

</details>

## Config Connector

The ConfigConnector add-on from GKE allows the declarative management of other GCP cloud resources such as SQL instances or storage bucket. However, after the installation it needs to be configured for it to work correctly.

**Lab Instructions**

1. Create a dedicated namespace `config-connector` and add `cnrm.cloud.google.com/project-id` label
2. Create ConfigConnector resource and configure Google Service Account

<details>
  <summary markdown="span">Click to expand solution ...</summary>

_TODO_

</details>

## Kubernetes Dashboard

The Kubernetes dashboard has not been installed as a GKE addon. Instead, we install the dashboard manually in the current version. Since RBAC is enabled we also need to make a few additional steps are required.

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

</details>

## External Secrets Management

The External Secrets Operator is a component to synchronize secrets from external APIs such
as the Google Secrets Manager. In this step we will install the component using Helm and then configure it to synchronize some secrets.

**Lab Instructions**

1. Install the External Secrets operator as Helm chart using Flux. Use the pod-based workload identity feature
2. Create a `SecretStore` and a `ExternalSecret` to obtain a secret from the GCP secret manager

<details>
  <summary markdown="span">Click to expand solution ...</summary>

_TODO_

</details>

## Addon and Alternative Labs

### Cluster Setup and Flux Bootstrapping with Infrastructure as Code

Instead of using the CLI tools to bootstrap the GKE cluster and Flux, use a proper
Infrastructure as Code tool like Terraform or Pulumi to achieve the same.

## References

- https://cloud.google.com/sdk/gcloud/reference/container/clusters/create
- https://github.com/GoogleCloudPlatform/k8s-config-connector
- https://cloud.google.com/config-connector/docs/how-to/install-upgrade-uninstall
- https://cloud.google.com/config-connector/docs/reference/overview
