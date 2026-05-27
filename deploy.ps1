Set-PSDebug -Trace 1

docker compose build
docker save -o ./images/bonuses-app bonusesapp-app
scp ./images/bonuses-app root@213.178.155.94:~/bonuses-app/bonuses-app-image
ssh root@213.178.155.94 "cd ~/bonuses-app; docker compose down; docker load -i bonuses-app-image; docker compose up -d"  
