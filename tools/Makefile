GCP_PROJECT ?= cloud-native-experience-lab
GCP_REGION ?= europe-north1
GCP_ZONE ?= europe-north1-b

GITHUB_USER ?= qaware
CLUSTER_NAME ?= cloud-native-explab
VCLUSTER_NAME ?= tenant-01

AWS_ACCOUNT_ID := $(shell aws sts get-caller-identity --query Account --output text)
AWS_REGION ?= eu-central-1

prepare-gcp:
	@gcloud config set project $(GCP_PROJECT)
	@gcloud config set compute/zone $(GCP_ZONE)
	@gcloud config set container/use_client_certificate False

create-gcp-cluster:
	@gcloud container clusters create $(CLUSTER_NAME)  \
	  	--release-channel=regular \
		--cluster-version=1.26 \
  		--region=$(GCP_REGION) \
		--addons HttpLoadBalancing,HorizontalPodAutoscaling,ConfigConnector \
		--workload-pool=$(GCP_PROJECT).svc.id.goog \
  		--enable-vertical-pod-autoscaling \
		--enable-autoscaling \
		--autoscaling-profile=optimize-utilization \
		--num-nodes=1 \
		--min-nodes=1 --max-nodes=10 \
		--machine-type=e2-standard-8 \
		--logging=SYSTEM \
    	--monitoring=SYSTEM
	@kubectl create clusterrolebinding cluster-admin-binding --clusterrole=cluster-admin --user=$$(gcloud config get-value core/account)
	@kubectl cluster-info

create-gcp-sa:
	@gcloud iam service-accounts create $(CLUSTER_NAME) --description="$(CLUSTER_NAME) Service Account" --display-name="$(CLUSTER_NAME) Service Account"
	@gcloud projects add-iam-policy-binding $(GCP_PROJECT) --role=roles/editor --member=serviceAccount:$(CLUSTER_NAME)@$(GCP_PROJECT).iam.gserviceaccount.com
	@gcloud iam service-accounts add-iam-policy-binding $(CLUSTER_NAME)@$(GCP_PROJECT).iam.gserviceaccount.com --member="serviceAccount:$(GCP_PROJECT).svc.id.goog[cnrm-system/cnrm-controller-manager]" --role="roles/iam.workloadIdentityUser"
	@gcloud iam service-accounts keys create gke-sa-key.json --iam-account=$(CLUSTER_NAME)@$(GCP_PROJECT).iam.gserviceaccount.com

bootstrap-gcp-flux2:
	@flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./clusters/gcp/$(CLUSTER_NAME) \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key

bootstrap-vcluster-flux2:
	@flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./clusters/gcp/$(CLUSTER_NAME)/$(VCLUSTER_NAME) \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key

delete-gcp-cluster:
	@gcloud container clusters delete $(CLUSTER_NAME) --async --quiet

create-aws-cluster:
	@eksctl create cluster -f aws-cluster.yaml

bootstrap-aws-flux2:
	@flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./clusters/aws/$(CLUSTER_NAME) \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key

delete-aws-cluster:
	@eksctl delete cluster -f aws-cluster.yaml

build-explab-shell:
	@docker build -t cn-explab-shell cn-explab-shell/

push-explab-shell:
	@docker tag cn-explab-shell qaware/cn-explab-shell
	@docker push qaware/cn-explab-shell
