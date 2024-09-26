@echo off
setlocal enabledelayedexpansion

:: Ouvre une première cmd pour le serveur
start cmd /k "cd Server && dotnet run --launch-profile https"

:: Ouvre le nombre de cmd pour les clients spécifié par l'utilisateur
start cmd /k "cd Client && dotnet run"

start cmd /k "cd Client && dotnet run"

exit
