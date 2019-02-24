docker build . -f ./RecommendationService/Dockerfile -t recommendation-service

# Remove images
# $images = docker images -q

# foreach ($image in $images) {
#  docker rmi $image -f
# }