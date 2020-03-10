$root = $PSScriptRoot;
. $root\..\Shared.ps1

# Authorization Server
Push-Location "$root/AuthorizationServer"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run -f netcoreapp3.1 server.urls=http://localhost:52698" -PassThru 
Pop-Location

# Client Application
Push-Location "$root/ClientApp"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run -f netcoreapp3.1" -PassThru
Pop-Location