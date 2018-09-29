$root = $PSScriptRoot;
. $root\..\Shared.ps1

$clientUrl = "http://localhost:5055";

# Authorization Server
Push-Location "$root/AuthorizationServer"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "run server.urls=http://localhost:5056" -PassThru
Pop-Location

# Angular Application
Push-Location "$root/AngularApp"
npm install -y
Start-Process npm -ArgumentList "run start" -PassThru
Pop-Location

Start-Browser $clientUrl;