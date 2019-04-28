docker stop recommendation-service
docker rm recommendation-service
docker run --name recommendation-service -p 3010:3010 `
    -e DatabaseConnectionString='Server=host.docker.internal,1433;Database=recommendations;User ID=application;Password=application;' `
    -e RedisConnectionString='host.docker.internal' `
    -e Logging__LogLevel__Default='Debug' `
    -e Logging__LogLevel__System='Information' `
    -e Logging__LogLevel__Microsoft='Information' `
    xellon/recommendation-service:latest