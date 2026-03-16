param(
    [ValidateSet('all','product','product-core','product-add','product-price','product-list','cart','account')]
    [string]$Scope = 'all',
    [string]$TestNamePattern = $null  # Optional: override scope with custom filter pattern (FullyQualifiedName~pattern)
)
$ErrorActionPreference = "Stop"

# Paths
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

# Ensure dotnet uses local writable folders (offline-friendly)
$env:DOTNET_CLI_HOME = Join-Path $repoRoot '.dotnet'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:NUGET_PACKAGES = Join-Path (Join-Path $HOME '.nuget') 'packages'

# Prepare results directory
$resultsDir = Join-Path (Join-Path $repoRoot 'obj') 'TestResults'
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null
Remove-Item (Join-Path $resultsDir '*.trx') -ErrorAction SilentlyContinue

# Build filter for selected scope
$filter = if ($TestNamePattern) {
    $splitters = @('|', ',')
    $parts = $TestNamePattern -split '[|,]' | Where-Object { $_ -and $_.Trim() }
    if ($parts.Count -gt 1) {
        $expr = ($parts | ForEach-Object { "FullyQualifiedName~$($_.Trim())" }) -join '|'
        "($expr)"
    } else {
        "FullyQualifiedName~$TestNamePattern"
    }
} else {
    switch ($Scope) {
        'product' { 'FullyQualifiedName~ProductServiceTests' }
        'product-core' { '(FullyQualifiedName~AddProduct|FullyQualifiedName~GetAllProducts|FullyQualifiedName~CalculateTotalPrice)' }
        'product-add' { 'FullyQualifiedName~AddProduct' }
        'product-price' { 'FullyQualifiedName~CalculateTotalPrice' }
        'product-list' { 'FullyQualifiedName~GetAllProducts' }
        'cart' { 'FullyQualifiedName~CartServiceTests' }
        'account' { 'FullyQualifiedName~AccountControllerTests' }
        default   { $null }
    }
}

# Run tests and produce TRX
$dotnetArgs = @('test','1298_DuyHung.csproj','--logger','trx;LogFileName=testResults.trx','--results-directory',$resultsDir,'--verbosity','minimal','--no-restore')
if ($filter) {
    $dotnetArgs += @('--filter', $filter)
}

Write-Host "Running tests (scope=$Scope) and collecting TRX report..." -ForegroundColor Cyan
$lastExit = 0
try {
    dotnet @dotnetArgs | Out-Host
    $lastExit = $LASTEXITCODE
}
catch {
    $lastExit = 1
}

# Load latest TRX
$trxFile = Get-ChildItem $resultsDir -Filter '*.trx' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $trxFile) {
    Write-Host 'No TRX file found. Tests may not have run.' -ForegroundColor Red
    exit 1
}
[xml]$trx = Get-Content $trxFile.FullName
$unitResults = $trx.TestRun.Results.UnitTestResult

# Mapping test names to friendly descriptions (use short method name as key)
$descriptions = @{
    # ProductServiceTests
    'AddProduct_WithValidProduct_ReturnsSuccess'                = @{ Case = 1;  Input = "Category(1,'Electronics'); Product('Laptop Dell',15,000,000,catId=1)"; Output = "Product saved, Id=1, Name='Laptop Dell', Price=15,000,000"; };
    'AddProduct_WithInvalidCategoryId_ThrowsException'          = @{ Case = 2;  Input = "Product catId=999"; Output = "Throws InvalidOperationException"; };
    'AddProduct_WithZeroCategoryId_Success'                     = @{ Case = 3;  Input = "Product catId=0"; Output = "Product saved, CategoryId=0"; };
    'GetAllProducts_WhenProductsExist_ReturnsProductList'       = @{ Case = 4;  Input = "3 products (catId=1)"; Output = "List count = 3"; };
    'GetAllProducts_WhenNoProducts_ReturnsEmptyList'            = @{ Case = 5;  Input = "DB empty"; Output = "List empty"; };
    'GetAllProducts_WithCategory_ReturnsProductsWithCategory'   = @{ Case = 6;  Input = "Product('Laptop',15,000,000,catId=1) + Category(1,'Electronics')"; Output = "1 item returned, Category loaded: 'Electronics'"; };
    'CalculateTotalPrice_WithValidQuantity_ReturnsCorrectTotal' = @{ Case = 7;  Input = "price=10,000,000; quantity=3"; Output = "Total=30,000,000"; };
    'CalculateTotalPrice_WithZeroQuantity_ReturnsZero'          = @{ Case = 8;  Input = "price=10,000,000; quantity=0"; Output = "Total=0"; };
    'CalculateTotalPrice_WithOneQuantity_ReturnsPrice'          = @{ Case = 9;  Input = "price=500,000; quantity=1"; Output = "Total=500,000"; };
    'GetProductById_WhenProductExists_ReturnsProduct'           = @{ Case = 10; Input = "Product('Laptop Dell',15,000,000,catId=1); lookup by id"; Output = "Product returned with name & price"; };
    'GetProductById_WhenProductNotExists_ReturnsNull'           = @{ Case = 11; Input = "id=999"; Output = "null"; };
    'GetProductById_WithCategory_ReturnsProductWithCategory'    = @{ Case = 12; Input = "Product('Laptop',15,000,000,catId=1) + Category(1,'Electronics')"; Output = "Product returned with Category 'Electronics'"; };
    'CalculateTotalPrice_WithNegativeQuantity_ThrowsException'  = @{ Case = 13; Input = "price=10,000,000; quantity=-1"; Output = "Throws ArgumentException"; };
    'CalculateTotalPrice_WithNullProduct_ThrowsException'       = @{ Case = 14; Input = "product=null; quantity=3"; Output = "Throws ArgumentNullException"; };
    'CalculateTotalPrice_WithNegativePrice_ThrowsException'     = @{ Case = 15; Input = "price=-10,000,000; quantity=3"; Output = "Throws ArgumentException"; };

    # CartServiceTests
    'AddItem_NewProduct_AddsToCart'                = @{ Case = 19; Input = "productId=1; qty=2; price=15,000,000"; Output = "Item added with quantity 2"; };
    'AddItem_ExistingProduct_IncrementsQuantity'   = @{ Case = 20; Input = "productId=1 added twice (1 + 2)"; Output = "Quantity becomes 3"; };
    'GetTotal_ReturnsSumOfAllItems'                = @{ Case = 21; Input = "Laptop 15,000,000 x1; Mouse 500,000 x3"; Output = "Total=16,500,000"; };
    'RemoveItem_RemovesByProductId'                = @{ Case = 22; Input = "Remove productId=1 from 2 items"; Output = "Item removed, only id=2 left"; };
    'AddItem_WithInvalidData_Throws'               = @{ Case = 23; Input = "qty=0 or price<0"; Output = "Throws ArgumentException"; };
    'AddItem_WithZeroPrice_AllowsAndTotalsZero'    = @{ Case = 24; Input = "price=0; qty=2"; Output = "Total remains 0"; };
    'AddItem_UpdatesImageUrlWhenProvided'          = @{ Case = 25; Input = "same id twice, image old -> new"; Output = "ImageUrl updated to new, qty=2"; };
    'GetTotal_WhenEmpty_ReturnsZero'               = @{ Case = 26; Input = "cart empty"; Output = "Total=0"; };
    'RemoveItem_WhenNotFound_ReturnsFalseAndNoChange' = @{ Case = 27; Input = "remove id=999"; Output = "Returns false; cart unchanged"; };
    'Clear_RemovesAllItems'                        = @{ Case = 28; Input = "cart has 2 items"; Output = "Cart empty; total=0"; };

    # AccountControllerTests
    'Login_WithValidCredentials_RedirectsHomeAndUpdatesLastLogin' = @{ Case = 16; Input = "username='alice', password='P@ssw0rd', captcha ok"; Output = "Redirect to Home/Index; LastLogin set; UpdateAsync + PasswordSignInAsync called"; };
    'Login_WhenCaptchaFails_ReturnsViewWithModelError'            = @{ Case = 17; Input = "username='alice', password='P@ssw0rd', captcha fail"; Output = "ViewResult; ModelState invalid; SignIn not called"; };
    'Login_WithInvalidCredentials_AddsErrorAndReturnsView'        = @{ Case = 18; Input = "username='alice', password='wrong', captcha ok"; Output = "ViewResult; ModelState invalid with errors"; };
}

# Build table data
$table = foreach ($r in $unitResults) {
    $simpleName = ($r.testName -split '\.')[-1]
    $info = $descriptions[$simpleName]
    $caseNo = if ($info) { '{0:00}' -f $info.Case } else { '--' }
    $input = if ($info) { $info.Input } else { '' }
    $output = if ($info) { $info.Output } else { '' }
    $outcome = if ($r.outcome -eq 'Passed') { 'PASS' } else { $r.outcome.ToUpper() }
    [pscustomobject]@{
        Case       = $caseNo
        Result     = $outcome
        Test       = $simpleName
        Input      = $input
        Output     = $output
    }
}

# Re-number cases sequentially for the displayed table (avoids gaps when filtering)
$table = $table | Sort-Object Case
$idx = 1
$table = $table | ForEach-Object {
    $_ | Add-Member -NotePropertyName DisplayCase -NotePropertyValue ('{0:00}' -f $idx) -Force
    $idx++
    $_
}

Write-Host ''
Write-Host '=============================================================' -ForegroundColor White
Write-Host (" KET QUA TEST CHI TIET (scope={0})" -f $Scope) -ForegroundColor Yellow
Write-Host '=============================================================' -ForegroundColor White
# Helpers to make table compact
function Trunc([string]$text, [int]$maxLen) {
    if (-not $text) { return '' }
    if ($text.Length -le $maxLen) { return $text }
    return $text.Substring(0, $maxLen - 3) + '...'
}

$wCase   = 4
$wResult = 6
$wTest   = 48
$wInput  = 42
$wOutput = 46

$header = "{0,-4} {1,-6} {2,-48} {3,-42} {4,-46}" -f 'No','Result','Test','Input','Output'
$sep    = '-' * ($header.Length)

Write-Host $header
Write-Host $sep

$table | ForEach-Object {
    $row = "{0,-4} {1,-6} {2,-48} {3,-42} {4,-46}" -f `
        (Trunc $_.DisplayCase $wCase),
        (Trunc $_.Result $wResult),
        (Trunc $_.Test   $wTest),
        (Trunc $_.Input  $wInput),
        (Trunc $_.Output $wOutput)
    Write-Host $row
}

if ($lastExit -ne 0 -or ($table | Where-Object { $_.Result -ne 'PASS' })) {
    exit 1
}
