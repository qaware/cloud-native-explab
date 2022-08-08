# Bare Metal Kubernetes Cloudkoffer

This lab is really hands on with real hardware to give you that good old computer science feeling.
Potential use cases are e.g. a private cloud Kubernetes setup in an on-premise data center on either
bare metal or virtualized hardware.

![Conceptual Architecture](cloudkoffer.png)

## Ubuntu Server 22.04 Installation

The case consists of 5 NUCs. All need to be pre-installed with the latest Ubuntu Server linux distribution. 
- Download the latest ISO image from https://ubuntu.com/download/server
- Use [Balena Etcher](https://www.balena.io/etcher/) or a similar tool to put the ISO on a USB stick
- Boot each NUC from the USB stick and perform basic installation.

Make sure to setup the same root password and user account with same password on all machines. This makes 
the microk8s setup on all machines a lot easier later on.

| Node          | IP (static)    | DNS | Gateway | Packages  |
| ------------- |----------------| --- | ------- | --------- |
| k8s-master.cloudkoffer   | 192.168.178.10 | 8.8.8.8 | 192.168.178.1 | OpenSSH, microk8s Snap |
| k8s-node-1.cloudkoffer   | 192.168.178.20 | 8.8.8.8 | 192.168.178.1 | OpenSSH, microk8s Snap |
| k8s-node-2.cloudkoffer   | 192.168.178.30 | 8.8.8.8 | 192.168.178.1 | OpenSSH, microk8s Snap |
| k8s-node-3.cloudkoffer   | 192.168.178.40 | 8.8.8.8 | 192.168.178.1 | OpenSSH, microk8s Snap |
| k8s-node-4.cloudkoffer   | 192.168.178.50 | 8.8.8.8 | 192.168.178.1 | OpenSSH, microk8s Snap |

Once the initial setup of each nodes is done, it is recommended to perform a `sudo apt update && apt upgrade`.
On the master node you may additionally install a GUI as described here 

On the master node edit the `/etc/hosts` file and add the IPs and hostnames of all cluster nodes.
```
192.168.178.10  k8s-master.cloudkoffer k8s-master
192.168.178.20  k8s-node-1.cloudkoffer k8s-node-1
192.168.178.30  k8s-node-2.cloudkoffer k8s-node-2
192.168.178.40  k8s-node-2.cloudkoffer k8s-node-3
192.168.178.50  k8s-node-4.cloudkoffer k8s-node-4
```

On the master node, create a keypair and copy the SSH ID to all the nodes. Basically, follow the instructions given here: http://www.thegeekstuff.com/2008/11/3-steps-to-perform-ssh-login-without-password-using-ssh-keygen-ssh-copy-id

## microk8s Cluster Setup

After the initial server installation all nodes run as individual microk8s nodes. Now we need to join all nodes
with the master to automatically for a HA cluster. Also, we need to enable useful and required microk8s addons.

**Lab Instructions**
1. Enable at least the following standard microk8s addons: 
    - DNS
    - RBAC
    - hostpath-storage
    - dashboard
    - metallb
2. Add and join all 4 nodes to the master node
3. _(Optional)_ Enable additional community addons, e.g.
    - linkerd
    - starboard
    - traefik

<details>
  <summary markdown="span">Click to expand solution ...</summary>

  ```bash
# prepare the master node
microk8s status

microk8s enable dns
microk8s enable rbac
microk8s enable hostpath-storage
microk8s enable dashboard
microk8s enable metallb
# when asked, use the following IP range: 192.168.178.60-192.168.178.100

# start joining the other nodes
# see https://microk8s.io/docs/clustering

# repeat the following steps for each of the nodes
# and use the output from the following command
microk8s add-node
ssh k8s-node-1
microk8s join 192.168.178.10:25000/92b2db237428470dc4fcfc4ebbd9dc81/2c0cb3284b05
...

# once all four nodes have joined check their status
microk8s status
microk8s kubectl get nodes

# enable additional community addons
microk8s enable community
microk8s enable linkerd
microk8s enable starboard
microk8s enable traefik
  ```
</details>

## Platform Bootstrapping with Flux2

Next, we will bootstrap Flux2 as GitOps tool to provision further infrastructure and platform components. as well as the applications.

**Lab Instructions**
1. Install the Flux2 CLI on either the microk8s master or your developer machine
2. Create personal Github token and export as ENV variable
3. Bootstrap the flux-system namespace and components
    - use this repository as GitOps repository
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
export GITHUB_TOKEN=<your-token>

# bootstrap the flux-system namespace and components
flux bootstrap github \
	--owner=$GITHUB_USER \
  --repository=cloud-native-explab \
  --branch=main \
  --path=./clusters/bare/microk8s-cloudkoffer \
	--components-extra=image-reflector-controller,image-automation-controller \
	--read-write-key
  # --token-auth       # instead of SSH key access, use the Github token instead
  # --personal         # only for user accounts, not for org accounts

# you may need to update and modify Flux kustomization
# - infrastructure-sync.yaml

flux create kustomization infrastructure \
  --source=GitRepository/flux-system \
  --path="./infrastructure/bare/microk8s-cloudkoffer"
  --prune=true \
  --interval=5m0s \
  --export > ./clusters/bare/microk8s-cloudkoffer/infrastructure-sync.yaml

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization infrastructure
flux get all
  ```
</details>

### Kubernetes Dashboard

The Kubernetes dashboard has already been installed as microk8s addon as part of the cluster setup.
However, since RBAC has been enabled for the cluster a few additional steps are required.

**Lab Instructions**
1. Create service account and cluster role binding using Flux2
2. Expose the dashboard UI as _LoadBalancer_ service or using an _Ingress_ resource
3. Generate user token and access dashboard UI

<details>
  <summary markdown="span">Click to expand solution ...</summary>

  ```yaml
# see https://github.com/kubernetes/dashboard/blob/master/docs/user/access-control/creating-sample-user.md
# the microk8s dashboard is installed in the kube-system namespace
# create dashboard-rbac.yaml in the GitOps infrastructure directory
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

Now you can open and access the dashboard in your preferred browser. If you prefer to expose the dashboard UI as
LoadBalancer or Ingress, add the K8s resource definitions to the configured GitOps repository.

  ```bash
# either use port forwarding
microk8s kubectl port-forward -n kube-system service/kubernetes-dashboard 10443:443
# or (manually) use a LoadBalancer to access the dashboard
microk8s kubectl patch service kubernetes-dashboard -n kube-system -p '{"spec": {"type": "LoadBalancer"}}'
microk8s kubectl get services -n kube-system

# create an access token to login to the dashboard
microk8s kubectl -n kube-system create token admin-user
  ```
</details>

### Observability with Grafana, Loki and Tempo

For good observability we will use a Grafana-based stack, which is completely free software:
- [Prometheus](https://prometheus.io/) to collect metrics
- [Promtail](https://grafana.com/docs/loki/latest/clients/promtail/) to forward logs to [Loki](https://grafana.com/docs/loki/latest/)
- [Tempo](https://grafana.com/docs/tempo/latest/) to receive traces

Read the blog post [Cloud Observability With Grafana And Spring Boot](https://blog.qaware.de/posts/cloud-observability-grafana-spring-boot/) for more details.

```bash
# we can use the Flux CLI to create the GitOps manifests for the observability stack
cd infrastructure/bare/microk8s-cloudkoffer

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
```

## Applications Deployment with Flux2

Now, we will finally setup Flux2 as GitOps tool to provision cloud-native applications.

**Lab Instructions**
1. Add another Flux2 Kustomization for the `applications/` folder, that depends on the _infrastructure_ Kustomization
2. Add support for image update automation for the `applications/` folder

<details>
  <summary markdown="span">Click to expand solution ...</summary>
  
  ```bash
# you may need to update and modify Flux kustomization
# - applications-sync.yaml
# - image-update-automation.yaml

flux create kustomization applications \
  --depends-on=infrastructure
  --source=GitRepository/flux-system \
  --path="./applications/bare/microk8s-cloudkoffer"
  --prune=true \
  --interval=5m0s \
  --export > ./clusters/bare/microk8s-cloudkoffer/applications-sync.yaml

# see https://fluxcd.io/docs/guides/image-update/

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization applications
flux get all
  ```
</details>

### 

## Addon and Alternative Labs

### PXE Boot Server for NUC Setup

Instead of manually provision the hardware nodes with the operating system and software, we could use the PXE boot
mechanism to provision the individual nodes automatically on boot.

- https://linuxhint.com/pxe_boot_ubuntu_server/
- https://www.tecmint.com/install-ubuntu-via-pxe-server-using-local-dvd-sources/


### Fedora CoreOS with K3s Cluster

An alternative base setup at the IaaS and CaaS layer is the combination of CoreOS and K3s.

- https://devopstales.github.io/kubernetes/k3s-fcos/
- https://stevex0r.medium.com/setting-up-a-lightweight-kubernetes-cluster-with-k3s-and-fedora-coreos-12d504160366
- https://www.murillodigital.com/tech_talk/k3s_in_coreos/

### ArgoCD instead of Flux2

Instead of Flux2 you could use ArgoCD as GitOps tool. Luckily, in case of microk8s there is a suitable addon.

```bash
# enable community addons
microk8s enable community
microk8s enable argocd

# see https://argo-cd.readthedocs.io/en/stable/getting_started/
# install argo CLI
kubectl patch svc argocd-server -n argocd -p '{"spec": {"type": "LoadBalancer"}}'

# get the initial password
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d; echo
argocd login <ARGOCD_SERVER>
```

### k3OS with K3s Cluster

see https://github.com/rancher/k3os#installation
