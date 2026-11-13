param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$MigrationName
)

$projectDirectory = Split-Path -Parent $PSScriptRoot
Push-Location $projectDirectory
try {
    dotnet ef migrations add $MigrationName --output-dir Migrations
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create EF Core migration '$MigrationName'."
    }

    dotnet ef database update
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to apply EF Core migrations to the configured SQL Server database."
    }
}
finally {
    Pop-Location
}
