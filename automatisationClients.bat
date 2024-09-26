@echo off
setlocal enabledelayedexpansion

:: Demande combien de clients l'utilisateur veut
set /p clients=Combien de clients voulez-vous ? 

:: Ouvre une première cmd pour le serveur
start cmd /k "cd Server && dotnet run --launch-profile https"

:: Ouvre le nombre de cmd pour les clients spécifié par l'utilisateur
for /l %%i in (1, 1, %clients%) do (
    start cmd /k "cd Client && dotnet run"
    :: Faire une pause de 0.1 seconde
    timeout /t 1 > nul
)

exit
