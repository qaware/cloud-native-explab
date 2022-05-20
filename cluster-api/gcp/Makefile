PROJECT ?= cloud-native-experience-lab
ZONE ?= europe-west1-b
GITHUB_USER ?= qaware

prepare-gke-cluster:
    @gcloud config set project $(PROJECT)
	@gcloud config set compute/zone $(ZONE)
    @gcloud config set container/use_client_certificate False

create-gke-cluster:
    @gcloud container clusters create capi-mgmt-cluster --num-nodes=3 --enable-autoscaling --min-nodes=3 --max-nodes=5 --cluster-version=1.21.6-gke.1503
    @kubectl create clusterrolebinding cluster-admin-binding --clusterrole=cluster-admin --user=$$(gcloud config get-value core/account)
    @kubectl cluster-info

bootstrap-gke-flux2:
    @flux bootstrap github \
		--owner=$(GITHUB_USER) \
        --repository=cloud-native-explab \
        --branch=main \
        --path=./cluster-api/gcp/clusters/capi-mgmt-cluster \
		--components-extra=image-reflector-controller,image-automation-controller \
		--read-write-key \
		--personal

delete-gke-cluster:
	@gcloud container clusters delete capi-mgmt-cluster --async --quiet