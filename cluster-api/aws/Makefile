AWS_ACCOUNT_ID := $(shell aws sts get-caller-identity --query Account --output text)
AWS_REGION ?= eu-central-1
GITHUB_USER ?= qaware

create-capi-cluster:
	@eksctl create cluster -f capi-cluster.yaml

bootstrap-capi-flux2:
	@flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./cluster-api/aws/clusters/capi-mgmt-cluster \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key \
  		--personal

delete-capi-cluster:
	@eksctl delete cluster -f capi-cluster.yaml