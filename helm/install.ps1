helm dependency update ./recommendation-cluster
helm install ./recommendation-cluster --values ./values.yaml --name recommendation-cluster