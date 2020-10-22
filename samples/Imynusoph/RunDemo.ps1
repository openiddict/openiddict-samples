$root = $PSScriptRoot;
. $root\..\Shared.ps1

$clientUrl = "https://localhost:44379";

# Authorization Server
Push-Location "$root/Imynusoph.Server"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "run urls=https://localhost:44382" -PassThru
Pop-Location

# Angular Application
Push-Location "$root/Imynusoph.Client"
npm install -y
Start-Process npm -ArgumentList "run start" -PassThru
Pop-Location

Start-Browser $clientUrl;