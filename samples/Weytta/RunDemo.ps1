$root = $PSScriptRoot;
. $root\..\Shared.ps1

# Authorization Server
Push-Location "$root/Weytta.Server"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run urls=https://localhost:44319" -PassThru 
Pop-Location

# Client Application
Push-Location "$root/Weytta.Client"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run" -PassThru
Pop-Location