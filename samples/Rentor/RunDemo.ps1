$root = $PSScriptRoot;
. $root\..\Shared.ps1

$clientUrl = "https://localhost:44238";

# Authorization Server
Push-Location "$root/Rentor.Server"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run urls=https://localhost:44213" -PassThru 
Pop-Location

# Client Application
Push-Location "$root/Rentor.Client"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run urls=$clientUrl" -PassThru
Pop-Location

Start-Browser $clientUrl;