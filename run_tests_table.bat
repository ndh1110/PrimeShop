@echo off
echo ========================================================================
echo                    CHAY TEST XUNIT - PRODUCT SERVICE
echo ========================================================================
echo.

cd /d "%~dp0"

echo [1] AddProduct Tests...
dotnet test 1298_DuyHung.csproj --filter "FullyQualifiedName~AddProduct" --no-build -v q
echo.

echo [2] GetAllProducts Tests...
dotnet test 1298_DuyHung.csproj --filter "FullyQualifiedName~GetAllProducts" --no-build -v q
echo.

echo [3] CalculateTotalPrice Tests...
dotnet test 1298_DuyHung.csproj --filter "FullyQualifiedName~CalculateTotalPrice" --no-build -v q
echo.

echo [4] GetProductById Tests...
dotnet test 1298_DuyHung.csproj --filter "FullyQualifiedName~GetProductById" --no-build -v q
echo.

echo ========================================================================
echo                        TONG KET: 15 TESTS
echo ========================================================================
pause

