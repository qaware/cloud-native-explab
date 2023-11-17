# EKS based Kubernetes native Platform

This lab will provision a EKS based platform with several useful infrastructure components as well
as a demo application.

## Prerequisites

Before you dive right into this experience lab, make sure your local environment is setup properly.
Alternatively, use the preconfigured `cn-explab-shell` Docker image.

- Modern Operating System (Windows 10, MacOS, ...) with terminal and shell
- IDE of your personal choice (with relevant plugins installed)
  - IntelliJ Ultimate
  - VS Code
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- [eksctl](https://docs.aws.amazon.com/eks/latest/userguide/eksctl.html)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Flux2](https://fluxcd.io/flux/cmd/)
- [Kustomize](https://kustomize.io)

## Amazon EKS Cluster Setup

In this initial step we will create the ELS cluster using the `eksctl` CLI.

**Lab Instructions**

1. Create an EKS cluster in the `eu-central-1`region with the following settings and properties:
   - Kubernetes version 1.22
   - Cluster endpoints are reachable via private and public endpoints
   - 5 to 10 managed nodes using Karpenter auto scaling and instance type `t3.xlarge`
   - CloudWatch cluster logging enabled for all components
   - OIDC and IRSA enabled
   - Service accounts and policies enabled: Cert Manager, AWS LoadBalancer Controller, EBS CSI, EFS CSI, AutoScaler, Cloudwatch, Image Builder, AutoScaler

<details>
  <summary markdown="span">Click to expand solution ...</summary>

Create a new YAML file with the following manifest to configure and create the EKS cluster:

```yaml
apiVersion: eksctl.io/v1alpha5
kind: ClusterConfig

metadata:
  name: cloud-native-explab
  region: eu-central-1
  version: '1.22'

iam:
  withOIDC: true
  serviceAccounts:
  - metadata:
      name: aws-load-balancer-controller
      namespace: kube-system
    wellKnownPolicies:
      awsLoadBalancerController: true
  - metadata:
      name: ebs-csi-controller-sa
      namespace: kube-system
    wellKnownPolicies:
      ebsCSIController: true
  - metadata:
      name: efs-csi-controller-sa
      namespace: kube-system
    wellKnownPolicies:
      efsCSIController: true
  - metadata:
      name: cert-manager
      namespace: cert-manager
    wellKnownPolicies:
      certManager: true
  - metadata:
      name: build-service
      namespace: ci-cd
    wellKnownPolicies:
      imageBuilder: true

vpc:
  clusterEndpoints:
    privateAccess: true
    publicAccess: true

cloudWatch:
  clusterLogging:
    enableTypes: ["*"]

managedNodeGroups:
  - name: managed-explab-cluster-ng-1
    instanceType: t3.xlarge
    minSize: 5
    maxSize: 10
    desiredCapacity: 5
    volumeSize: 20
    ssh:
      allow: false
    labels: {role: worker}
    tags:
      nodegroup-role: worker
    iam:
      withAddonPolicies:
        certManager: true
        albIngress: true
        awsLoadBalancerController: true
        imageBuilder: true
        autoScaler: true        
        ebs: true
        efs: true
        cloudWatch: true
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

## AWS Controllers for Kubernetes

The Amazon controllers for Kubernetes are a lightweight AWS only option to provision cloud infrastructure the K8s-native way.

**Lab Instructions**

1. Deploy and configure the ACK S3 Controller as Helm Chart from the public AWS ECR
    - Create a IAM role for the controller and the S3 controller service account
    - Attach the `AmazonS3FullAccess` policy to the role
    - Add the `eks.amazonaws.com/role-arn` annotation to the controller service account
    - Restart the S3 controller deployment
2. (_optional_) Deploy and configure the ACK ECR Controller as Helm Chart from the public AWS ECR
3. (_optional_) Deploy and configure the ACK RDS Controller as Helm Chart from the public AWS ECR

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
export ACK_SYSTEM_NAMESPACE=ack-system
export AWS_REGION=eu-central-1
export AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query "Account" --output text)
export OIDC_PROVIDER=$(aws eks describe-cluster --name cloud-native-explab --region $AWS_REGION --query "cluster.identity.oidc.issuer" --output text | sed -e "s/^https:\/\///")

# we need to login to the public chart ECR
aws ecr-public get-login-password --region $AWS_REGION | helm registry login --username AWS --password-stdin public.ecr.aws

# install the S3 controller
helm install -n $ACK_SYSTEM_NAMESPACE ack-s3-controller \
    oci://public.ecr.aws/aws-controllers-k8s/s3-chart --version=v0.1.3 --set=aws.region=$AWS_REGION

# setup IAM permissions and IRSA
envsubst < ack-s3-controller-trust.tpl > ack-s3-controller-trust.json
aws iam create-role \
    --role-name ack-s3-controller \
    --assume-role-policy-document file://ack-s3-controller-trust.json \
    --description "IRSA role for ACK S3 controller"
aws iam attach-role-policy \
    --role-name ack-s3-controller \
    --policy-arn arn:aws:iam::aws:policy/AmazonS3FullAccess

export ACK_CONTROLLER_IAM_ROLE_ARN=$(aws iam get-role --role-name=ack-s3-controller --query Role.Arn --output text)
export IRSA_ROLE_ARN=eks.amazonaws.com/role-arn=$ACK_CONTROLLER_IAM_ROLE_ARN
kubectl annotate serviceaccount -n ack-system ack-s3-controller $IRSA_ROLE_ARN
kubectl -n ack-system rollout restart deployment ack-s3-controller-s3-chart
```

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

Finally, create the access token for the admin user.
```bash
kubectl -n kubernetes-dashboard create token admin-user
```

</details>

## External Secrets Management

The External Secrets Operator is a component to synchronize secrets from external APIs such as the AWS Secrets Manager or Parameter Store. In this step we will install the component using Helm and then configure it to synchronize some secrets.

**Lab Instructions**

1. Install the External Secrets operator as Helm chart using Flux
2. Create a `SecretStore` and a `ExternalSecret` to obtain a secret from the AWS secrets manager

<details>
  <summary markdown="span">Click to expand solution ...</summary>

_TODO_

</details>

## Pod Info Application Deployment

In this step we will deploy [Podinfo](https://github.com/stefanprodan/podinfo).
Podinfo is a tiny web application made with Go that showcases best practices of running microservices in Kubernetes. Podinfo is used by CNCF projects like Flux and Flagger for end-to-end testing and workshops.

**Lab Instructions**

1. Read the installation instructions at https://github.com/stefanprodan/podinfo
2. Install the Podinfo application into the default namespace either as Helm chart or Kustomize
    - Patch the Podinfo deployment and set `replicas: 3`
    - Patch the PodInfo HPA and set `minReplicas: 3`
    - Patch the PodInfo Service and set `type: LoadBalancer`
3. (_optional_) Setup the image update automation workflow with suitable image repository and policy

<details>
  <summary markdown="span">Click to expand solution ...</summary>

```bash
cd applications/gcp/cloud-native-explab
kustomize create

flux create source git podinfo \
    --url=https://github.com/stefanprodan/podinfo \
    --tag="6.1.8" \
    --interval=30s \
    --export > podinfo/podinfo-source.yaml

flux create kustomization podinfo \
    --source=GitRepository/podinfo \
    --path="./kustomize" \
    --prune=true \
    --interval=5m0s \
    --target-namespace=default \
    --export > podinfo/podinfo-kustomization.yaml
```

The Kustomize patches need to be added manually to the `podinfo-kustomization.yaml`.

```yaml
  images:
    - name: ghcr.io/stefanprodan/podinfo
      newName: ghcr.io/stefanprodan/podinfo # {"$imagepolicy": "flux-system:podinfo:name"}
      newTag: 6.1.8 # {"$imagepolicy": "flux-system:podinfo:tag"}
  patchesStrategicMerge:
    - apiVersion: autoscaling/v2beta2
      kind: HorizontalPodAutoscaler
      metadata:
        name: podinfo
      spec:
        minReplicas: 3
    - apiVersion: apps/v1
      kind: Deployment
      metadata:
        name: podinfo
        labels:
          lab: cloud-native-explab
      spec:
        replicas: 3
        template:
          metadata:
            labels:
              lab: cloud-native-explab
    - apiVersion: v1
      kind: Service
      metadata:
        name: podinfo
      spec:
        type: LoadBalancer
```

Then add and configure image repository and policy for the image update automation to work.

```bash
flux create image repository podinfo \
    --image=ghcr.io/stefanprodan/podinfo \
    --interval 1m0s \
    --export > podinfo/podinfo-registry.yaml

flux create image policy podinfo \
    --image-ref=podinfo \
    --select-semver="6.1.x" \
    --export > podinfo/podinfo-policy.yaml
```

</details>

## Addon and Alternative Labs

### Cluster Setup and Flux Bootstrapping with Infrastructure as Code

Instead of using the CLI tools to bootstrap the EKS cluster and Flux, use a proper
Infrastructure as Code tool like Terraform or Pulumi to achieve the same.

## References

- https://eksctl.io/
- https://aws-controllers-k8s.github.io/community/
