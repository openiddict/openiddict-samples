$root = $PSScriptRoot;
. $root\..\Shared.ps1

# Authorization Server
Push-Location "$root/Aridka.Server"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run urls=https://localhost:44385" -PassThru 
Pop-Location

# Client Application
Push-Location "$root/Aridka.Client"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run" -PassThru
Pop-Location