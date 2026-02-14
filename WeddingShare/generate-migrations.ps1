param(
    [string]$MigrationName,
    [switch]$Fresh
)

if (-not $MigrationName) {
    Write-Host "Error: MigrationName is required" -ForegroundColor Red
    Write-Host "Usage: .\generate-migrations.ps1 -MigrationName 'InitializeDatabase'" -ForegroundColor Yellow
    exit 1
}

if ($Fresh) {
    Write-Host "FRESH START: Removing all existing migrations..." -ForegroundColor Red
    if (Test-Path "Migrations") {
        Remove-Item -Path "Migrations" -Recurse -Force
        Write-Host "✓ All migrations removed" -ForegroundColor Green
    }
}

Write-Host "Cleaning up temporary files and databases..." -ForegroundColor Yellow

# Delete SQLite files
Get-ChildItem -Path . -Filter "_design_temp_*.db" | Remove-Item -Force -ErrorAction SilentlyContinue
Write-Host "✓ Cleaned SQLite temp files" -ForegroundColor Green

# Drop MySQL design database
try {
    mysql -e "DROP DATABASE IF EXISTS _design_temp_mysql;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Dropped MySQL design database" -ForegroundColor Green
    }
} catch {
    Write-Host "  (Skipped MySQL - not accessible)" -ForegroundColor DarkGray
}

# Drop SQL Server design database
try {
    sqlcmd -Q "DROP DATABASE IF EXISTS _design_temp_mssql;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Dropped SQL Server design database" -ForegroundColor Green
    }
} catch {
    Write-Host "  (Skipped SQL Server - not accessible)" -ForegroundColor DarkGray
}

# Drop Postgres design database
try {
    psql -U postgres -c "DROP DATABASE IF EXISTS _design_temp_postgres;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Dropped Postgres design database" -ForegroundColor Green
    }
} catch {
    Write-Host "  (Skipped Postgres - not accessible)" -ForegroundColor DarkGray
}

Write-Host ""

$providers = @(
    @{ Name = "Sqlite"; Type = "sqlite" },
    @{ Name = "MySql"; Type = "mysql" },
    @{ Name = "SqlServer"; Type = "mssql" },
    @{ Name = "Postgres"; Type = "postgres" }
)

foreach ($provider in $providers) {
    Write-Host "Generating migrations for $($provider.Name)..." -ForegroundColor Cyan
    
    $env:Database__Type = $provider.Type
    
    $outputDir = "Migrations/$($provider.Name)"
    
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
        Write-Host "  Created directory: $outputDir" -ForegroundColor Yellow
    }
    
    # Delete any existing ModelSnapshot before generation
    Get-ChildItem -Path $outputDir -Filter "*ModelSnapshot.cs" | Remove-Item -Force -ErrorAction SilentlyContinue
    
    # Generate migration
    $uniqueMigrationName = "$($provider.Name)_$MigrationName"
    dotnet ef migrations add $uniqueMigrationName --output-dir $outputDir
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Successfully generated: $uniqueMigrationName" -ForegroundColor Green
        
        # Delete ModelSnapshot after generation to keep providers isolated
        Get-ChildItem -Path $outputDir -Filter "*ModelSnapshot.cs" | Remove-Item -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host "  ✗ Failed" -ForegroundColor Red
    }
    Write-Host ""
}

# Final cleanup
Remove-Item Env:Database__Type -ErrorAction SilentlyContinue
Get-ChildItem -Path . -Filter "_design_temp_*.db" | Remove-Item -Force -ErrorAction SilentlyContinue

Write-Host "Done!" -ForegroundColor Green