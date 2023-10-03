set LANG=ja_JP.UTF-8
git config --global core.quotepath false

git add .
printf "\n"

git status
printf "\n"

read -p "Commit."

git commit -a -m'Update'
printf "\n"

#read -p "Sync."

#git pull -v --progress "origin"
#printf "\n"

#git push -v --progress "origin" main:main
#printf "\n"

read -p "Complete."
