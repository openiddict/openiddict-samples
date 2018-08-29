$global:p = @();

function global:Find-ChildProcess {
  param($ID=$PID)

  $result = Get-CimInstance win32_process |
    Where-Object { $_.ParentProcessId -eq $ID }
    Select-Object -Property ProcessId

  $result
  $result |
    Where-Object { $_.ProcessId -ne $null } |
    ForEach-Object {
      Find-ChildProcess -id $_.ProcessId
    }
}

function global:Kill-Demo {
  $Global:p |
    ForEach-Object { Find-ChildProcess -ID $_.Id } |
    ForEach-Object { Stop-Process -id $_.ProcessId }
}

[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

# Authorization Server
Push-Location "./AuthorizationServer"
dotnet restore
dotnet build --no-incremental #rebuild
$global:p += Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:12345" -PassThru
Pop-Location

# Aurelia Application
Push-Location "./AureliaApp"
npm install -y
$global:p += Start-Process yarn -ArgumentList "run start" -PassThru
Pop-Location

# Resource Server 01
Push-Location "./ResourceServer01"
dotnet restore
dotnet build --no-incremental
$global:p += Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5001" -PassThru
Pop-Location

# Resource Server 02
Push-Location "./ResourceServer02"
dotnet restore
dotnet build --no-incremental
$global:p += Start-Process dotnet -ArgumentList "watch run server.urls=http://localhost:5002" -PassThru
Pop-Location

