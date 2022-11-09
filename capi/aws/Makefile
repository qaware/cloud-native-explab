AWS_ACCOUNT_ID := $(shell aws sts get-caller-identity --query Account --output text)
AWS_REGION ?= eu-central-1
GITHUB_USER ?= qaware

create-capi-cluster:
	@eksctl create cluster -f capi-cluster.yaml

prepare-capi-aws:
	# create an SSH key
	@export AWS_REGION=$(AWS_REGION)
	@export AWS_ACCESS_KEY_ID=<INSERT>
	@export AWS_SECRET_ACCESS_KEY=<INSERT>
	@clusterawsadm bootstrap iam create-cloudformation-stack --config bootstrap-config.yaml

bootstrap-capi-cluster:
	# required to avoid API rate limiting
	@echo "You may need to set a personal GITHUB_TOKEN to avoid API rate limiting"
	@export AWS_REGION=$(AWS_REGION)
	@export AWS_SSH_KEY_NAME=capi-default
	@export AWS_CONTROL_PLANE_MACHINE_TYPE=t3.medium
	@export AWS_NODE_MACHINE_TYPE=t3.medium
	@export AWS_B64ENCODED_CREDENTIALS=$(shell clusterawsadm bootstrap credentials encode-as-profile)
	@clusterctl init --infrastructure aws

create-tenant:
	@clusterctl generate cluster capi-tenant-demo --kubernetes-version $(KUBERNETES_VERSION) --control-plane-machine-count=1 --worker-machine-count=1 > capi-tenant-demo.yaml
	@kubectl apply -f capi-tenant-demo.yaml
	
	# kubectl get cluster 
	# clusterctl describe cluster capi-tenant-demo 
	# kubectl get kubeadmcontrolplane

	# clusterctl get kubeconfig capi-tenant-demo > capi-tenant-demo.kubeconfig
	# kubectl --kubeconfig=./capi-tenant-demo.kubeconfig apply -f https://raw.githubusercontent.com/projectcalico/calico/v3.24.1/manifests/calico.yaml
	# kubectl --kubeconfig=./capi-tenant-demo.kubeconfig get nodes

bootstrap-capi-flux2:
	# my need to add --personal if the GITHUB_USER is not an org
	@echo "You must set a personal GITHUB_TOKEN"
	@flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./clusters/aws/capi-mgmt-cluster \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key

delete-capi-cluster:
	@eksctl delete cluster -f capi-cluster.yaml
