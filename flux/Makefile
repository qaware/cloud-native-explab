
CLUSTER_PATH = ./clusters/flux-managed-cluster
INFRA_PATH = $(CLUSTER_PATH)/infrastructure
MONITORING_PATH = $(CLUSTER_PATH)/monitoring

all: monitoring infrastructure

monitoring: setup-monitoring create-prometheus create-loki create-flux-dashboard

infrastructure: add-gitops-linkerd add-alpine-source add-falcosecurity-source add-kyverno-source create-linkerd create-flagger create-alpine create-falco create-falcosidekick create-kyverno create-kyverno-policies add-kedacore create-keda

setup-monitoring:
	@flux create source git flux-monitoring --interval=30m --url=https://github.com/fluxcd/flux2 \
		--branch=main \
		--export > $(MONITORING_PATH)/monitoring-source.yaml

create-prometheus:
	@flux create kustomization kube-prometheus-stack \
	--interval=1h --prune --source=flux-monitoring \
	--path="./manifests/monitoring/kube-prometheus-stack" \
	--health-check-timeout=5m \
	--wait \
	--export > $(MONITORING_PATH)/prometheus-kustomization.yaml

create-loki:
	@flux create kustomization loki-stack --depends-on=kube-prometheus-stack \
	--interval=1h --prune --source=flux-monitoring \
	--path="./manifests/monitoring/loki-stack" --health-check-timeout=5m \
	--wait \
	--export > $(MONITORING_PATH)/loki-kustomization.yaml

create-flux-dashboard:
	@flux create kustomization monitoring-config \
	--depends-on=kube-prometheus-stack \
  	--interval=1h \
  	--prune=true \
	--source=flux-monitoring \
	--path="./manifests/monitoring/monitoring-config" \
	--health-check-timeout=1m \
	--wait \
	--export > $(MONITORING_PATH)/monitoring-kustomization.yaml

add-gitops-linkerd:
	@flux create source git gitops-linkerd \
	--url=https://github.com/stefanprodan/gitops-linkerd \
	--branch=main \
	--export > $(INFRA_PATH)/linkerd-source.yaml

create-linkerd:
	@flux create kustomization linkerd \
	--path="./infrastructure/linkerd" \
	--source=gitops-linkerd \
	--prune=true \
	--interval=1m \
	--health-check="Deployment/linkerd-proxy-injector.linkerd" \
	--export > $(INFRA_PATH)/linkerd-kustomization.yaml

create-flagger:
	@flux create kustomization flagger \
	--depends-on=linkerd \
	--source=gitops-linkerd \
	--path="./infrastructure/flagger" \
	--prune=true \
	--interval=1m \
	--health-check="Deployment/flagger.linkerd" \
	--export > $(INFRA_PATH)/flagger-kustomization.yaml

add-alpine-source:
	@flux create source git alpine \
    --url https://github.com/developer-guy/desired-state-repository \
    --branch master \
    --interval 30s \
	--export > $(INFRA_PATH)/alpine-source.yaml

create-alpine:
	@flux create kustomization alpine \
    --source alpine \
    --path "./" \
    --prune true \
    --interval 5m \
	--export > $(INFRA_PATH)/alpine-kustomization.yaml

add-falcosecurity-source:
	@flux create source helm falcosecurity \
    --url=https://falcosecurity.github.io/charts \
    --interval=10m \
    --export > $(INFRA_PATH)/falcosecurity-source.yaml

create-falco:
	@flux create helmrelease falco \
    --interval=10m \
    --source=HelmRepository/falcosecurity \
    --chart=falco \
    --export > $(INFRA_PATH)/falco-kustomization.yaml

create-falcosidekick:
	@flux create helmrelease falcosidekick \
    --interval=10m \
    --source=HelmRepository/falcosecurity \
    --chart=falcosidekick \
    --export > $(INFRA_PATH)/falcosidekick-kustomization.yaml

add-kyverno-source:
	@flux create source helm kyverno \
    --url=https://kyverno.github.io/kyverno/ \
	--interval=10m \
    --export > $(INFRA_PATH)/kyverno-source.yaml

create-kyverno:
	@flux create helmrelease kyverno \
    --interval=10m \
	--source=HelmRepository/kyverno \
    --chart=kyverno \
    --export > $(INFRA_PATH)/kyverno-kustomization.yaml

create-kyverno-policies:
	@flux create helmrelease kyverno-policies \
	--depends-on=kyverno \
    --interval=10m \
    --source=HelmRepository/kyverno \
    --chart=kyverno-policies \
    --export > $(INFRA_PATH)/kyverno-policies-kustomization.yaml

add-kedacore: 
	@flux create source helm kedacore \
    --url=https://kedacore.github.io/charts \
	--interval=10m \
    --export > $(INFRA_PATH)/kedacore-source.yaml

create-keda:
	@flux create helmrelease keda \
    --interval=10m \
    --source=HelmRepository/kedacore \
    --chart=keda \
    --export > $(INFRA_PATH)/keda-kustomization.yaml