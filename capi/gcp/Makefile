GCP_PROJECT ?= cloud-native-experience-lab
GCP_REGION ?= europe-west1
GCP_ZONE ?= europe-west1-b
KUBERNETES_VERSION ?= 1.22.15
GCP_NETWORK_NAME ?= default
IMAGE_ID ?= projects/$(GCP_PROJECT)/global/images/cluster-api-ubuntu-2004-v1-22-9-1653382327
CLUSTER_NAME ?= capi-mgmt-cluster
GCP_CONTROL_PLANE_MACHINE_TYPE ?= e2-medium
GCP_NODE_MACHINE_TYPE ?= e2-medium
CONTROL_PLANE_MACHINE_COUNT ?= 1
WORKER_MACHINE_COUNT ?= 1
GITHUB_USER ?= qaware

prepare-gcp:
	@gcloud config set project $(GCP_PROJECT)
	@gcloud config set compute/zone $(GCP_ZONE)
	@gcloud config set container/use_client_certificate False

prepare-cloud-nat:
	@gcloud compute routers create $(CLUSTER_NAME)-router --project=$(GCP_PROJECT) --region=$(GCP_REGION) --network=$(GCP_NETWORK_NAME)
	@gcloud compute routers nats create $(CLUSTER_NAME)-nat --project=$(GCP_PROJECT) --router-region=$(GCP_REGION) --router=$(CLUSTER_NAME)-router --nat-all-subnet-ip-ranges --auto-allocate-nat-external-ips

create-capi-cluster:
	@gcloud container clusters create $(CLUSTER_NAME) --num-nodes=3 --enable-autoscaling --min-nodes=3 --max-nodes=5 --cluster-version=$(KUBERNETES_VERSION)
	@kubectl create clusterrolebinding cluster-admin-binding --clusterrole=cluster-admin --user=$$(gcloud config get-value core/account)
	@kubectl cluster-info	

create-sa:
	@gcloud iam service-accounts create $(CLUSTER_NAME) --description="$(CLUSTER_NAME) Service Account" --display-name="$(CLUSTER_NAME) Service Account"
	@gcloud projects add-iam-policy-binding $(GCP_PROJECT) --role=roles/editor --member=serviceAccount:$(CLUSTER_NAME)@$(GCP_PROJECT).iam.gserviceaccount.com
	@gcloud iam service-accounts keys create gke-sa-key.json --iam-account=$(CLUSTER_NAME)@$(GCP_PROJECT).iam.gserviceaccount.com

create-images:
	@export GCP_GCP_PROJECT_ID=$(GCP_PROJECT)
	@export GOOGLE_APPLICATION_CREDENTIALS=$PWD/gke-sa-key.json
	@git clone https://github.com/kubernetes-sigs/image-builder.git image-builder
	@cd image-builder/images/capi && make build-gce-all
	@gcloud compute images list --project $(GCP_PROJECT_ID) --no-standard-images 

create-tenant:
	@clusterctl generate cluster capi-tenant-demo --kubernetes-version $(KUBERNETES_VERSION) --control-plane-machine-count=3 --worker-machine-count=3 > capi-tenant-demo.yaml
	@kubectl apply -f capi-tenant-demo.yaml
	
	# kubectl get cluster 
	# clusterctl describe cluster capi-tenant-demo 
	# kubectl get kubeadmcontrolplane

	# clusterctl get kubeconfig capi-tenant-demo > capi-tenant-demo.kubeconfig
	# kubectl --kubeconfig=./capi-tenant-demo.kubeconfig apply -f https://raw.githubusercontent.com/projectcalico/calico/v3.24.1/manifests/calico.yaml
	# kubectl --kubeconfig=./capi-tenant-demo.kubeconfig get nodes

bootstrap-capi-cluster:
	# required to avoid API rate limiting
	@echo "You may need to set a personal GITHUB_TOKEN to avoid API rate limiting"
	@export GCP_B64ENCODED_CREDENTIALS=$(cat gke-sa-key.json | base64 | tr -d '\n' )
	@clusterctl init --infrastructure gcp

bootstrap-capi-flux2:
	# my need to add --personal if the GITHUB_USER is not an org
	@echo "You must set a personal GITHUB_TOKEN"
	@flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./clusters/gcp/capi-mgmt-cluster \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key

delete-capi-cluster:
	@gcloud container clusters delete capi-mgmt-cluster --async --quiet
