GITHUB_USER ?= qaware
BOOTSTRAP_PATH = ./clusters/flux-managed-cluster
GITOPS_REPO = cloud-native-explab


create-kind-cluster:
	@kind create cluster

bootstrap-flux2:
	@flux bootstrap github \
	--owner=$(GITHUB_USER) \
 	--repository=$(GITOPS_REPO) \
  	--branch=main \
  	--path=$(BOOTSTRAP_PATH) \
	--components-extra=image-reflector-controller,image-automation-controller \
	--read-write-key \
	--personal

delete-kind-cluster:
	@kind delete cluster	