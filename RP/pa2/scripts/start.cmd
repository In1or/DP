@echo off

cd "../Valuator"

start dotnet run --urls "http://0.0.0.0:5001"
start dotnet run --urls "http://0.0.0.0:5002"

cd "../../nginx-1.26.0"
start "" "nginx.exe"

