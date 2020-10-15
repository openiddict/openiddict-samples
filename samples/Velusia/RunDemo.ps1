$root = $PSScriptRoot;
. $root\..\Shared.ps1

$clientUrl = "http://localhost:53507";

# Authorization Server
Push-Location "$root/Velusia.Server"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run urls=http://localhost:54540" -PassThru 
Pop-Location

# Client Application
Push-Location "$root/Velusia.Client"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run urls=$clientUrl" -PassThru
Pop-Location

Start-Browser $clientUrl;