function global:Kill-Demo {
    Write-Host "Kill all dotnet and node processes (default no)?";
    $response = Read-Host "Input 'y' for yes; press enter to continue" 
    if ('y' -eq $response) {
       kill -name node -ErrorAction SilentlyContinue;
       kill -name dotnet -ErrorAction SilentlyContinue; 
    }
}

[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

# Authorization Server
Push-Location "./AuthorizationServer"
dotnet restore
dotnet build --no-incremental #rebuild
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:12345" -PassThru 
Pop-Location

# Aurelia Application
Push-Location "./AureliaApp"
npm install -y
Start-Process yarn -ArgumentList "run start" -PassThru
Pop-Location

# Resource Server 01
Push-Location "./ResourceServer01"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5001" -PassThru
Pop-Location

# Resource Server 02
Push-Location "./ResourceServer02"
dotnet restore
dotnet build --no-incremental
Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5002" -PassThru
Pop-Location

