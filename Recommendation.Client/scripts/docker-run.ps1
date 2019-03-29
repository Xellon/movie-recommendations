docker stop recommendation-client
docker rm recommendation-client
docker run --name recommendation-client -p 4000:4000 xellon/recommendation-client:latest