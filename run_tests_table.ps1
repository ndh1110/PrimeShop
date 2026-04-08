param(
    [ValidateSet('all','product','product-core','product-add','product-price','product-list','cart','account')]
    [string]$Scope = 'all',
    [string]$TestNamePattern = $null
)
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

$resultsDir = Join-Path (Join-Path $repoRoot 'obj') 'TestResults'
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null
Remove-Item (Join-Path $resultsDir '*.trx') -ErrorAction SilentlyContinue

$filter = if ($TestNamePattern) {
    $parts = $TestNamePattern -split '[|,]' | Where-Object { $_ -and $_.Trim() }
    if ($parts.Count -gt 1) {
        $expr = ($parts | ForEach-Object { "FullyQualifiedName~$($_.Trim())" }) -join '|'
        "($expr)"
    } else { "FullyQualifiedName~$TestNamePattern" }
} else {
    switch ($Scope) {
        'product' { 'FullyQualifiedName~ProductServiceTests' }
        'cart'    { 'FullyQualifiedName~CartServiceTests' }
        'account' { 'FullyQualifiedName~Account' }
        default   { $null }
    }
}

$dotnetArgs = @('test','1298_DuyHung.csproj','--logger','trx;LogFileName=testResults.trx','--results-directory',$resultsDir,'--verbosity','minimal','--no-restore')
if ($filter) { $dotnetArgs += @('--filter', $filter) }

Write-Host "Running tests (scope=$Scope)..." -ForegroundColor Cyan
try { dotnet @dotnetArgs | Out-Host } catch { $lastExit = 1 }

$trxFile = Get-ChildItem $resultsDir -Filter '*.trx' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $trxFile) { Write-Host 'No TRX file found.' -ForegroundColor Red; exit 1 }
[xml]$trx = Get-Content $trxFile.FullName
$unitResults = $trx.TestRun.Results.UnitTestResult

$descriptions = @{
    # ProductServiceTests
    'AddProduct_WithValidProduct_ReturnsSuccess'                = @{ Case = 1;  Input = "Category(1,'Electronics'); Product('Laptop Dell')"; Output = "Saved success, Id=1"; };
    'AddProduct_WithInvalidCategoryId_ThrowsException'          = @{ Case = 2;  Input = "Product catId=999"; Output = "Throws InvalidOperationException"; };
    'AddProduct_WithZeroCategoryId_Success'                     = @{ Case = 3;  Input = "Product catId=0"; Output = "Saved success, CategoryId=0"; };
    'GetAllProducts_WhenProductsExist_ReturnsProductList'       = @{ Case = 4;  Input = "DB has 3 products"; Output = "List count = 3"; };
    'GetAllProducts_WhenNoProducts_ReturnsEmptyList'            = @{ Case = 5;  Input = "DB empty"; Output = "List empty"; };
    'GetAllProducts_WithCategory_ReturnsProductsWithCategory'   = @{ Case = 6;  Input = "Product + Category loaded"; Output = "Category included: 'Electronics'"; };
    'CalculateTotalPrice_WithValidQuantity_ReturnsCorrectTotal' = @{ Case = 7;  Input = "price=10M; qty=3"; Output = "Total=30,000,000"; };
    'CalculateTotalPrice_WithZeroQuantity_ReturnsZero'          = @{ Case = 8;  Input = "price=10M; qty=0"; Output = "Total=0"; };
    'CalculateTotalPrice_WithOneQuantity_ReturnsPrice'          = @{ Case = 9;  Input = "price=500k; qty=1"; Output = "Total=500,000"; };
    'GetProductById_WhenProductExists_ReturnsProduct'           = @{ Case = 10; Input = "Lookup Id exists"; Output = "Product returned with name/price"; };
    'GetProductById_WhenProductNotExists_ReturnsNull'           = @{ Case = 11; Input = "Id=999"; Output = "null"; };
    'GetProductById_WithCategory_ReturnsProductWithCategory'    = @{ Case = 12; Input = "Lookup with Cat"; Output = "Category 'Electronics' included"; };
    'CalculateTotalPrice_WithNegativeQuantity_ThrowsException'  = @{ Case = 13; Input = "qty=-1"; Output = "Throws ArgumentException"; };
    'CalculateTotalPrice_WithNullProduct_ThrowsException'       = @{ Case = 14; Input = "product=null"; Output = "Throws ArgumentNullException"; };
    'CalculateTotalPrice_WithNegativePrice_ThrowsException'     = @{ Case = 15; Input = "price=-10M"; Output = "Throws ArgumentException"; };
    # CartServiceTests
    'AddItem_NewProduct_AddsToCart'                = @{ Case = 19; Input = "Id=1; Qty=2; Price=15M"; Output = "Item added, Qty=2"; };
    'AddItem_ExistingProduct_IncrementsQuantity'   = @{ Case = 20; Input = "Added twice (1 + 2)"; Output = "Quantity becomes 3"; };
    'GetTotal_ReturnsSumOfAllItems'                = @{ Case = 21; Input = "Laptop 15M x1; Mouse 500k x3"; Output = "Total=16,500,000"; };
    'RemoveItem_RemovesByProductId'                = @{ Case = 22; Input = "Remove Id=1"; Output = "Item removed successfully"; };
    'AddItem_WithInvalidData_Throws'               = @{ Case = 23; Input = "Qty=0 or Price<0"; Output = "Throws ArgumentException"; };
    'AddItem_WithZeroPrice_AllowsAndTotalsZero'    = @{ Case = 24; Input = "Price=0; Qty=2"; Output = "Total remains 0"; };
    'AddItem_UpdatesImageUrlWhenProvided'          = @{ Case = 25; Input = "Image Old -> New"; Output = "ImageUrl updated, Qty=2"; };
    'GetTotal_WhenEmpty_ReturnsZero'               = @{ Case = 26; Input = "Cart empty"; Output = "Total=0"; };
    'RemoveItem_WhenNotFound_ReturnsFalseAndNoChange' = @{ Case = 27; Input = "Remove Id=999"; Output = "False; Cart unchanged"; };
    'Clear_RemovesAllItems'                        = @{ Case = 28; Input = "Clear 2 items"; Output = "Cart empty; Total=0"; };
    # AccountTests
    'Login_WithValidCredentials_RedirectsHomeAndUpdatesLastLogin' = @{ Case = 16; Input = "User: alice, Pass: OK"; Output = "Redirect Home, Update LastLogin"; };
    'Login_WhenCaptchaFails_ReturnsViewWithModelError'            = @{ Case = 17; Input = "Captcha fail"; Output = "ViewResult; ModelState invalid"; };
    'Login_WithInvalidCredentials_AddsErrorAndReturnsView'        = @{ Case = 18; Input = "Wrong password"; Output = "ViewResult; Error added"; };
    'Register_WithValidData_ReturnsSuccess'      = @{ Case = 29; Input = "Valid Data"; Output = "IdentityResult.Success"; };
    'Register_WithDuplicateEmail_ReturnsFailure' = @{ Case = 30; Input = "Email exists"; Output = "Error: Email already exists"; };
}

$table = foreach ($r in $unitResults) {
    $simpleName = ($r.testName -split '\.')[-1]
    $info = $descriptions[$simpleName]
    $outcome = if ($r.outcome -eq 'Passed') { 'PASS' } else { $r.outcome.ToUpper() }
    [pscustomobject]@{ Case = if ($info) { $info.Case } else { 99 }; Result = $outcome; Test = $simpleName; Input = if ($info) { $info.Input } else { '' }; Output = if ($info) { $info.Output } else { '' } }
}

$table = $table | Sort-Object Case
Write-Host "`n============================================================="
Write-Host " KET QUA TEST CHI TIET (Scope: $Scope)" -ForegroundColor Yellow
Write-Host "============================================================="
$table | Select-Object @{N='No';E={'{0:00}' -f $_.Case}}, Result, Test, Input, Output | Format-Table -AutoSize