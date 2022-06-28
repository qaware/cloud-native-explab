PROJECT ?= cloud-native-experience-lab
ZONE ?= europe-west1-b
GITHUB_USER ?= qaware

AWS_ACCOUNT_ID := $(shell aws sts get-caller-identity --query Account --output text)
AWS_REGION ?= eu-central-1

prepare-gcp:
    @gcloud config set project $(PROJECT)
	@gcloud config set compute/zone $(ZONE)
    @gcloud config set container/use_client_certificate False

create-gcp-cluster:
    @gcloud container clusters create cloud-native-explab --num-nodes=3 --enable-autoscaling --min-nodes=3 --max-nodes=10 --machine-type=e2-standard-4 --cluster-version=1.22
    @kubectl create clusterrolebinding cluster-admin-binding --clusterrole=cluster-admin --user=$$(gcloud config get-value core/account)
    @kubectl cluster-info

bootstrap-gcp-flux2:
    @flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./clusters/gcp/cloud-native-explab \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key \
		--personal

delete-gcp-cluster:
	@gcloud container clusters delete cloud-native-explab --async --quiet

create-aws-cluster:
	@eksctl create cluster -f aws-cluster.yaml

bootstrap-capi-flux2:
	@flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./clusters/aws/cloud-native-explab \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key \
  		--personal

delete-aws-cluster:
	@eksctl delete cluster -f aws-cluster.yaml

build-explab-shell:
	@docker build -t cn-explab-shell cn-explab-shell/

push-explab-shell:
	@docker tag cn-explab-shell qaware/cn-explab-shell
	@docker push qaware/cn-explab-shell
