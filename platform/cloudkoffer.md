# Bare Metal Kubernetes Cloudkoffer

This lab is really hands on with real hardware to give you that good old computer science feeling.

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



## Alternative Labs

### Fedora CoreOS with K3s Cluster

### ArgoCD instead of Flux2