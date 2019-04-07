$password='debugPASSWORD123'
# $mountLocation='C:\Users\Elonas\Documents\GitHub\movie-recommendations\MSSQLData';
#-v ($mountLocation':/var/opt/mssql') `

docker run -e 'ACCEPT_EULA=Y' -e ('SA_PASSWORD=' + $password) `
   -p 1433:1433 --name sql1 `
   -d mcr.microsoft.com/mssql/server:2017-latest

docker run --name redis -p 6379:6379 -d redis