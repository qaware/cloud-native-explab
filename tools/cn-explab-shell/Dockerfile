# use ubuntu as base image
FROM ubuntu:22.10

WORKDIR /cloud-native-explab

RUN apt-get update && \
        apt-get upgrade -y && \
        apt-get install -y curl apt-transport-https ca-certificates gnupg make git vim unzip

# kubectl
RUN curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl" && \
        install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl && \
        chmod +x kubectl && \
        mv ./kubectl /usr/local/bin/kubectl 

# clusterctl
RUN curl -L https://github.com/kubernetes-sigs/cluster-api/releases/download/v1.1.3/clusterctl-linux-amd64 -o clusterctl && \
        chmod +x ./clusterctl && \ 
        mv ./clusterctl /usr/local/bin/clusterctl 

# gcloud CLI
RUN echo "deb [signed-by=/usr/share/keyrings/cloud.google.gpg] https://packages.cloud.google.com/apt cloud-sdk main" | tee -a /etc/apt/sources.list.d/google-cloud-sdk.list &&\
        curl https://packages.cloud.google.com/apt/doc/apt-key.gpg |apt-key --keyring /usr/share/keyrings/cloud.google.gpg add - &&\
        apt-get -y update && \
        apt-get install -y google-cloud-cli 

# Flux CLI
RUN curl -s https://fluxcd.io/install.sh | bash 

# AWS CLI
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "/tmp/awscliv2.zip" && \
        unzip /tmp/awscliv2.zip -d /tmp && \
        /tmp/aws/install && \
        aws --version

# eksctl
RUN curl --silent --location "https://github.com/weaveworks/eksctl/releases/latest/download/eksctl_$(uname -s)_amd64.tar.gz" | tar xz -C /tmp && \
        mv /tmp/eksctl /usr/local/bin
