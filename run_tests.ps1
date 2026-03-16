# xUnit Test Runner Script
Write-Host "Running xUnit Tests..." -ForegroundColor Green

# Navigate to project directory
Set-Location "C:\Users\kaqua\HungRZ_Shop"

# Run dotnet test
dotnet test --verbosity normal

Write-Host "`nTest execution completed." -ForegroundColor Green
