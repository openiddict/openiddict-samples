$root = $PSScriptRoot;
. $root\..\Shared.ps1

$clientUrl = "http://localhost:9000";

# Authorization Server
Push-Location "$root/Zirku.Server"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:12345" -PassThru 
Pop-Location

# Aurelia Application
Push-Location "$root/Zirku.Client"
npm install -y
Start-Process yarn -ArgumentList "run start" -PassThru
Pop-Location

# Resource Server 01
Push-Location "$root/Zirku.Api1"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5001" -PassThru
Pop-Location

# Resource Server 02
Push-Location "$root/Zirku.Api2"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5002" -PassThru
Pop-Location

Start-Browser $clientUrl;