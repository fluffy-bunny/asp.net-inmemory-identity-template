docker images
az acr login --name acrchaosbunny

docker tag fluffybunny/inmemoryidentityapptemplate:latest acrchaosbunny.azurecr.io/fluffybunny/inmemoryidentityapptemplate:latest
docker push acrchaosbunny.azurecr.io/fluffybunny/inmemoryidentityapptemplate:latest

