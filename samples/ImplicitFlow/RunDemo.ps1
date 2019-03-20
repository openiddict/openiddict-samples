$global:p = @();

function global:Find-ChildProcess {
    param($ID = $PID)

    $result = Get-CimInstance win32_process |
        Where-Object { $_.ParentProcessId -eq $ID }
    Select-Object -Property ProcessId

    $result
    $result |
        Where-Object { $null -ne $_.ProcessId } |
        ForEach-Object {
        Find-ChildProcess -id $_.ProcessId
    }
}

function global:Stop-Demo {
    $Global:p |
        ForEach-Object { Find-ChildProcess -ID $_.Id } |
        ForEach-Object { Stop-Process -id $_.ProcessId }
}

[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

# Authorization Server
$cmds = {
    "dotnet restore";
    "dotnet build --no-incremental"; #rebuild
    "dotnet watch run server.urls=http://localhost:12345"
};

$global:p += Start-Process powershell -ArgumentList $cmds -WorkingDirectory "./AuthorizationServer"

# Aurelia Application
$cmds = {
  "npm install -y";
  "npm run demo";
};

$global:p += Start-Process powershell -ArgumentList $cmds -WorkingDirectory "./AureliaApp"

# Resource Server 01
$cmds = {
    "dotnet restore";
    "dotnet build --no-incremental"; #rebuild
    "dotnet watch run server.urls=http://localhost:5001"
};

$global:p += Start-Process powershell -ArgumentList $cmds -WorkingDirectory "./ResourceServer01"

# Resource Server 02
$cmds = {
    "dotnet restore";
    "dotnet build --no-incremental"; #rebuild
    "dotnet watch run server.urls=http://localhost:5002"
};

$global:p += Start-Process powershell -ArgumentList $cmds -WorkingDirectory "./ResourceServer02"
