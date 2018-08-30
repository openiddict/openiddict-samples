# TODO Share this with the other RunDemo.ps1 files.
function global:Stop-Demo {
    Write-Output "Kill all dotnet and node processes?";
    Write-Output "Input 'y' for yes (default no). Press enter to continue."
    $response = Read-Host
    if ('y' -eq $response) {
       Stop-Process -name node -ErrorAction SilentlyContinue;
       Stop-Process -name dotnet -ErrorAction SilentlyContinue; 
    }
}

[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

$authServerUrl = "http://localhost:54540";
$clientUrl = "http://localhost:53507";

# Authorization Server
Push-Location "./AuthorizationServer"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run -f netcoreapp2.0 server.urls=$authServerUrl" -PassThru 
Pop-Location

# Client Application
Push-Location "./ClientApp"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run -f netcoreapp2.0 server.urls=$clientUrl" -PassThru
Pop-Location

Write-Output 
Write-Output "Client is running at $clientUrl"
