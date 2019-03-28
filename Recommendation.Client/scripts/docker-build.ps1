cd ../..
copy .dockerignore-client .dockerignore
docker login
docker build -t xellon/recommendation-client:latest . -f Dockerfile-Client

cd Recommendation.Client/scripts
#del .dockerignore
