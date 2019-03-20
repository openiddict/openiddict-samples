function Start-Commands {
    param($cmds, $dir)
    Start-Process powershell -ArgumentList $cmds -WorkingDirectory $dir
}

function Stop-Demo {
    # TODO Kill all the processes that we started.
}

[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

# Authorization Server
Start-Commands -dir './AuthorizationServer' -cmds {
    "dotnet restore";
    "dotnet build --no-incremental"; #rebuild
    "dotnet watch run server.urls=http://localhost:12345"
}

# Resource Server 01
Start-Commands -dir './ResourceServer01' -cmds {
    "dotnet restore";
    "dotnet build --no-incremental"; #rebuild
    "dotnet watch run server.urls=http://localhost:5001"
}

# Resource Server 02
Start-Commands -dir './ResourceServer02' -cmds {
    "dotnet restore";
    "dotnet build --no-incremental"; #rebuild
    "dotnet watch run server.urls=http://localhost:5002"
}

# Aurelia Application
Start-Commands -dir './AureliaApp' -cmds {
    "npm install -y";
    "npm run demo";
}
