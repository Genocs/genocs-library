#!/bin/bash


for service in apigateway identities products orders notifications
do
    microk8s kubectl apply -f ./$service.yml
done