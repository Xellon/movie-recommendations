docker stop recommendation-service
docker rm recommendation-service
docker run --name recommendation-service -p 3010:3010 xellon/recommendation-service:latest