$root = $PSScriptRoot;
. $root\..\Shared.ps1

$serverUrl = "https://localhost:44321";

# Authorization Server
Push-Location "$root/Matty.Server"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run urls=$serverUrl" -PassThru 
Pop-Location

# Client Application
Push-Location "$root/Matty.Client"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run" -PassThru
Pop-Location