cd ../..
copy .dockerignore-service .dockerignore
docker login
docker build -t xellon/recommendation-service:latest . -f Dockerfile-Service

cd Recommendation.Service/scripts
#del .dockerignore
