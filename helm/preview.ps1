helm dependency update ./recommendation-cluster
helm template --output-dir ./generated ./recommendation-cluster