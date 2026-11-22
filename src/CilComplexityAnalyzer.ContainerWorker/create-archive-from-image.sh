docker create --name temp-container container-worker
docker export temp-container -o container-worker.tar
docker rm temp-container