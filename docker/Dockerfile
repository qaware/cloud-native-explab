# use ubuntu as base image
FROM ubuntu

# copy data
WORKDIR /experience-lab

COPY . /experience-lab/

# Install required packages
RUN apt-get update && \
        apt-get upgrade -y && \
        apt-get install -y curl apt-transport-https ca-certificates gnupg make git vim unzip

# Install kubectl
RUN curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl" &&\
        install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl &&\
        chmod +x kubectl &&\
        mv ./kubectl /usr/local/bin/kubectl 

# Install clusterctl
RUN curl -L https://github.com/kubernetes-sigs/cluster-api/releases/download/v1.1.3/clusterctl-linux-amd64 -o clusterctl &&\
        chmod +x ./clusterctl &&\ 
        mv ./clusterctl /usr/local/bin/clusterctl 

# Install gcloud cli
RUN echo "deb [signed-by=/usr/share/keyrings/cloud.google.gpg] https://packages.cloud.google.com/apt cloud-sdk main" | tee -a /etc/apt/sources.list.d/google-cloud-sdk.list &&\
        curl https://packages.cloud.google.com/apt/doc/apt-key.gpg |apt-key --keyring /usr/share/keyrings/cloud.google.gpg add - &&\
        apt-get -y update &&\
        apt-get install -y google-cloud-cli 

# Install Flux CLI
RUN curl -s https://fluxcd.io/install.sh | bash 
