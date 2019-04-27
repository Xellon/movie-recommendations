helm dependency update ./recommendation-cluster
del ./generated -Recurse -Force
mkdir ./generated
helm template --output-dir ./generated --values ./values.yaml ./recommendation-cluster