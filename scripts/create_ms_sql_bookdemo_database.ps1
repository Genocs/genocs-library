# This script allows to create the bookdemo database in a MS SQL Server instance. It is used by the demo container to initialize the database with the required schema and data for the demo application.
# It can also be used to create the database in a local MS SQL Server instance for development purposes.

# To Run the script with the default .env file in the current directory:
# cd .\scripts; .\create_ms_sql_bookdemo_database.ps1; cd ..

# To Run the script with a specific .env file:
# cd .\scripts; .\create_ms_sql_bookdemo_database.ps1 -envFilePath "path\to\your\.env"; cd ..

param (
    [Parameter(Mandatory=$false)]
    [string]$envFilePath = ".env"
)

# Resolve .env file path against common repository locations.
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$candidateEnvPaths = @(
    $envFilePath,
    (Join-Path $scriptDir $envFilePath),
    (Join-Path $repoRoot $envFilePath),
    (Join-Path $repoRoot "infrastructure\containers\.env")
)

$resolvedEnvFilePath = $candidateEnvPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

# Load environment variables from the discovered .env file.
if ($resolvedEnvFilePath) {
    Get-Content $resolvedEnvFilePath | ForEach-Object {
        if ($_ -match "^\s*#" -or [string]::IsNullOrWhiteSpace($_)) {
            return
        }

        if ($_ -match "^\s*([^=]+?)\s*=\s*(.*)\s*$") {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim().Trim('"').Trim("'")
            Set-Item -Path ("Env:{0}" -f $name) -Value $value
        }
    }

    Write-Host "Environment variables loaded from: $resolvedEnvFilePath"
} else {
    Write-Warning ".env file not found. Checked: $($candidateEnvPaths -join ', ')"
}

# Load the connection string from the environment variable
$connectionString = $env:MSSQL_CONNECTION_STRING

$databasePassword = $env:MSSQL_PASSWORD
# Check if the connection string is set
if (-not $connectionString) {
    Write-Error "MSSQL_CONNECTION_STRING environment variable is not set."
    exit 1
}
# Create a new SQL connection
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
try {
    # Open the connection
    $connection.Open()
    Write-Host "Connection to MS SQL Server established successfully."

    # Create the bookdemo database if it does not exist
    $createDatabaseCommand = $connection.CreateCommand()
    $createDatabaseCommand.CommandText = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'bookdemo') CREATE DATABASE bookdemo;"
    $createDatabaseCommand.ExecuteNonQuery()
    Write-Host "Database 'bookdemo' created or already exists."

    # Use the bookdemo database
    $useDatabaseCommand = $connection.CreateCommand()
    $useDatabaseCommand.CommandText = "USE bookdemo;"
    $useDatabaseCommand.ExecuteNonQuery()

    # Setup a new user and login for the bookdemo database
    $createLoginCommand = $connection.CreateCommand()
    $createLoginCommand.CommandText = @"
IF NOT EXISTS (SELECT name FROM sys.sql_logins WHERE name = 'bookdemo_user')
BEGIN
    CREATE LOGIN bookdemo_user WITH PASSWORD = '$databasePassword';
    CREATE USER bookdemo_user FOR LOGIN bookdemo_user;
    ALTER ROLE db_owner ADD MEMBER bookdemo_user;
END
"@
    $createLoginCommand.ExecuteNonQuery()
    write-Host "Login and user 'bookdemo_user' created or already exists with db_owner role." ConsoleColor.Green

    # Create the schema and insert sample data
    #$schemaAndDataCommand = $connection.CreateCommand()
    #$schemaAndDataCommand.CommandText = @"

    # end try block
}catch {
    Write-Error "An error occurred: $_"
}
finally {
    # Close the connection
    if ($connection.State -eq 'Open') {
        $connection.Close()
        Write-Host "Connection to MS SQL Server closed."
    }
}