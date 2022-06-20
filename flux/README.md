# K8 cluster configuration with flux

This guide shows how to deploy several infrastructure and monitoring tools with `fluxCD`

## Pre requirements
 - flux CLI
 - kind 
 - git
 - github access token 

 ## Bootstrap a Kind cluster with FluxCD
Make targets in [kind/Makefile](./kind/Makefile) can be used to create a new kind cluster and bootstrap flux.
```bash
# Create a new kind cluster
make -C ./kind create-kind-cluster 
# bootstrap flux2
make -C ./kind bootstrap-flux2     
```
This will install all resources defined in [clusters/flux-managed-cluster](./clusters/flux-managed-cluster)

## Resource definitions
Targets in [Makefile](./Makefile) can be used to create or recreate resource definitions.

```bash
# Create all resource definitions
make all
# Create monitoring resource definitions only
make monitoring
# Create infrastructure resource definitions only
make infrastructure

# Apply resource definitions with git
git add --all && git commit -m "Add new resources"
git push
```

