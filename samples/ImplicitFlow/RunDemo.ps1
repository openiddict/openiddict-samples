[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

# Authorization Server
Push-Location "./AuthorizationServer"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:12345" -PassThru
Pop-Location

# Aurelia Application
Push-Location "./AureliaApp"
npm install -y
Start-Process npm -ArgumentList "run start" -PassThru
Pop-Location

# Resource Server 01
Push-Location "./ResourceServer01"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5001" -PassThru
Pop-Location

# Resource Server 02
Push-Location "./ResourceServer02"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5002" -PassThru
Pop-Location


Write-Out "++++++++++++++++++++++++++++++++++++++";
Write-Out "Open a web browser at localhost:9000";
Write-Out "To stop the demo, close all the console windows.";
Write-Out "++++++++++++++++++++++++++++++++++++++";

