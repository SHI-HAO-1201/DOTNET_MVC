param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ConnectionString
)

$projectDirectory = Split-Path -Parent $PSScriptRoot
Push-Location $projectDirectory
try {
    dotnet ef dbcontext scaffold $ConnectionString Microsoft.EntityFrameworkCore.SqlServer `
        --context AppDbContext `
        --context-dir Data/DatabaseFirst `
        --output-dir Data/Models/DatabaseFirst `
        --no-onconfiguring
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to scaffold the SQL Server database."
    }
}
finally {
    Pop-Location
}
