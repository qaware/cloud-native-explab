AWS_ACCOUNT_ID := $(shell aws sts get-caller-identity --query Account --output text)
AWS_REGION ?= eu-central-1
GITHUB_USER ?= qaware

create-capi-cluster:
	@eksctl create cluster -f capi-cluster.yaml

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

prepare-capi-aws:
	# create an SSH key
	@export AWS_REGION=$(AWS_REGION)
	@export AWS_ACCESS_KEY_ID=<INSERT>
	@export AWS_SECRET_ACCESS_KEY=<INSERT>
	@clusterawsadm bootstrap iam create-cloudformation-stack --config bootstrap-config.yaml

install-capi-aws:
	# required to avoid API rate limiting
	@echo "You may need to set a personal GITHUB_TOKEN to avoid API rate limiting"
	@export AWS_REGION=$(AWS_REGION)
	@export AWS_SSH_KEY_NAME=capi-default
	@export AWS_CONTROL_PLANE_MACHINE_TYPE=t3.medium
	@export AWS_NODE_MACHINE_TYPE=t3.medium
	@export AWS_B64ENCODED_CREDENTIALS=$(shell clusterawsadm bootstrap credentials encode-as-profile)
	@clusterctl init --infrastructure aws

delete-capi-cluster:
	@eksctl delete cluster -f capi-cluster.yaml