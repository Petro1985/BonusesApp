Set-PSDebug -Trace 1

docker compose build
docker save -o ./images/bonuses-app bonusesapp-app
scp ./images/bonuses-app peter@petr-prozorov.ru:~/bonuses-app/bonuses-app-image
ssh peter@petr-prozorov.ru "cd ~/bonuses-app; docker compose down; docker load -i bonuses-app-image; docker compose up -d"  
  
  