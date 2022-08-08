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

```bash
# prepare the master node
microk8s status

microk8s enable dns
microk8s enable rbac
microk8s enable hostpath-storage
microk8s enable dashboard
microk8s enable community

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
```

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
    --path=./clusters/bare/microk8s-cloudkoffer \
	--components-extra=image-reflector-controller,image-automation-controller \
	--read-write-key
    # --token-auth       # instead of SSH key access, use the Github token instead
  	# --personal         # only for user accounts, not for org accounts

# you may need to update and modify Flux kustomization
# - infrastructure-sync.yaml
# - applications-sync.yaml
# - image-update-automation.yaml

# to manually trigger the GitOps process use the following commands
flux reconcile source git flux-system
flux reconcile kustomization infrastructure
flux reconcile kustomization applications
```

## Kubernetes Dashboard

The Kubernetes dashboard has already been installed as microk8s addon as part of the cluster setup.
However, since RBAC has been enabled for the cluster a few additional steps are required, such as creating
a service account and cluster role binding.

Use the configure Flux2 GitOps repository to create and/or update the RBAC service account and cluster role
binding the dashboard.
```yaml
# see https://github.com/kubernetes/dashboard/blob/master/docs/user/access-control/creating-sample-user.md
# the microk8s dashboard is installed in the kube-system namespace
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

## Observability with Grafana, Loki and Tempo

```bash

````

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
