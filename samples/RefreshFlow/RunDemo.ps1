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
$global:p += Start-Process dotnet -ArgumentList "run server.urls=http://localhost:5056" -PassThru
Pop-Location

# Angular Application
Push-Location "./AngularApp"
npm install -y
$global:p += Start-Process npm -ArgumentList "run start" -PassThru
Pop-Location

Start-Process -FilePath http://localhost:5055