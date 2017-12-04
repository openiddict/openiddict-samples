[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

# Authorization Server
Write-Output "++++++++++++++++++++++++++++++++++++++";
Write-Output "Starting authorization server.";
Write-Output "++++++++++++++++++++++++++++++++++++++";
Push-Location "./AuthorizationServer"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:12345" -PassThru
Pop-Location

# Aurelia Application
Write-Output "++++++++++++++++++++++++++++++++++++++";
Write-Output "Starting client application.";
Write-Output "++++++++++++++++++++++++++++++++++++++";
Push-Location "./AureliaApp"
npm install -y
Start-Process npm -ArgumentList "run start" -PassThru
Pop-Location

# Resource Server 01
Write-Output "++++++++++++++++++++++++++++++++++++++";
Write-Output "Starting resource server 01.";
Write-Output "++++++++++++++++++++++++++++++++++++++";
Push-Location "./ResourceServer01"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5001" -PassThru
Pop-Location

# Resource Server 02
Write-Output "++++++++++++++++++++++++++++++++++++++";
Write-Output "Starting resource server 02.";
Write-Output "++++++++++++++++++++++++++++++++++++++";
Push-Location "./ResourceServer02"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5002" -PassThru
Pop-Location

Write-Output "++++++++++++++++++++++++++++++++++++++";
Write-Output "++++++++++++++++++++++++++++++++++++++";
Write-Output "Open a web browser at localhost:9000";
Write-Output "To stop the demo, close all the console windows.";
Write-Output "++++++++++++++++++++++++++++++++++++++";
Write-Output "++++++++++++++++++++++++++++++++++++++";

