$root = $PSScriptRoot;
. $root\..\Shared.ps1

$clientUrl = "http://localhost:53507";

# Authorization Server
Push-Location "$root/AuthorizationServer"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run -f netcoreapp2.0 server.urls=http://localhost:54540" -PassThru 
Pop-Location

# Client Application
Push-Location "$root/ClientApp"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run -f netcoreapp2.0 server.urls=$clientUrl" -PassThru
Pop-Location

Start-Browser $clientUrl;