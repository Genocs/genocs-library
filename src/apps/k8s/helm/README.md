# Voucher - Kubernetes Cluster Setup

This folder contains the necessary files to setup a Kubernetes cluster for the Generic service.

The application is composed by a WebApi and a Worker handled by as couple of Docker images.

The voucher-secret.yaml file contains the secrets for the application. The secrets are stored in a Kubernetes Secret object.

The Production secrets are stored in a Azure Key Vault and are retrieved by the application using the Azure Key Vault Provider for Secrets Store CSI Driver.

``` sh
# Create a namespace for the application i
kubectl create namespace utu-taxfree

# Deploy the all the all the components components along with it's secrets
kubectl apply R -f ./voucher-webapi.yaml
kubectl apply R -f ./voucher-secret.yaml
# patch the deployment to use the load balancer
# kubectl patch svc -n utu-taxfree jaeger-simple-query -p '{"spec": {"type": "LoadBalancer"}}'
# az login --tenant 3e106da8-8aaa-441e-8e17-9752767e6585
```
