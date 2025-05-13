#!/bin/bash

set -e

export PROJECT_ID="woa7019"
export REGION="us-central1"

install() {
    sudo apt-get update
    sudo apt-get install apt-transport-https ca-certificates gnupg curl -y
    curl https://packages.cloud.google.com/apt/doc/apt-key.gpg | sudo gpg --dearmor -o /usr/share/keyrings/cloud.google.gpg
    echo "deb [signed-by=/usr/share/keyrings/cloud.google.gpg] https://packages.cloud.google.com/apt cloud-sdk main" | sudo tee -a /etc/apt/sources.list.d/google-cloud-sdk.list
    sudo apt-get update
    sudo apt-get install google-cloud-cli -y
}

login() {
    gcloud auth login
    gcloud config set project "$PROJECT_ID"
    gcloud config set compute/region "$REGION"
}

deploy_function() {
    gcloud functions deploy process-image-text \
        --region="$REGION" \
        --runtime="python311" \
        --entry-point="main" \
        --source="./" \
        --trigger-http \
        --allow-unauthenticated

    gcloud functions describe process-image-text --region="$REGION" --format='value(httpsTrigger.url)'
}

# install
# login
deploy_function