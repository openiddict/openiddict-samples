function global:Stop-Demo {
    Write-Output "Kill all dotnet and node processes?";
    $response = Read-Host "Input <y> + <enter> for yes or <enter> for no"
    if ("y" -eq $response) {
       Write-Output "Okay. Stopping";
       Stop-Process -name node -ErrorAction SilentlyContinue;
       Stop-Process -name dotnet -ErrorAction SilentlyContinue; 
    } else {
        Write-Output "Okay. Not stopping";
    }
}

function Start-Browser($clientUrl) {
    $port = $clientUrl.Split(':') | Select-Object -Last 1;
    $result = $null;
    do {
        # TODO (nice-to-have) Make this asynchronous
        $result = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
    } until ($null -ne $result)

    Start-Process $clientUrl;
}

[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");