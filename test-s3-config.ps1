# Quick S3 Configuration Test Script
# Run this in PowerShell to verify your AWS setup

Write-Host "=== AWS S3 Configuration Test ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check Environment Variables
Write-Host "1. Checking Environment Variables..." -ForegroundColor Yellow
$awsAccessKey = $env:AWS_ACCESS_KEY_ID
$awsSecretKey = $env:AWS_SECRET_ACCESS_KEY
$awsRegion = $env:AWS_REGION
$awsBucket = $env:AWS_S3_BUCKET_NAME

if ($awsAccessKey) {
    Write-Host "   ? AWS_ACCESS_KEY_ID: $($awsAccessKey.Substring(0, [Math]::Min(8, $awsAccessKey.Length)))..." -ForegroundColor Green
} else {
    Write-Host "   ? AWS_ACCESS_KEY_ID: NOT SET" -ForegroundColor Red
}

if ($awsSecretKey) {
    Write-Host "   ? AWS_SECRET_ACCESS_KEY: ****" -ForegroundColor Green
} else {
    Write-Host "   ? AWS_SECRET_ACCESS_KEY: NOT SET" -ForegroundColor Red
}

if ($awsRegion) {
    Write-Host "   ? AWS_REGION: $awsRegion" -ForegroundColor Green
} else {
    Write-Host "   ? AWS_REGION: NOT SET (will default to us-east-1)" -ForegroundColor Yellow
}

if ($awsBucket) {
    Write-Host "   ? AWS_S3_BUCKET_NAME: $awsBucket" -ForegroundColor Green
} else {
    Write-Host "   ? AWS_S3_BUCKET_NAME: NOT SET" -ForegroundColor Red
}

Write-Host ""

# Test 2: Check if AWS CLI is installed
Write-Host "2. Checking AWS CLI..." -ForegroundColor Yellow
$awsCli = Get-Command aws -ErrorAction SilentlyContinue
if ($awsCli) {
    Write-Host "   ? AWS CLI is installed" -ForegroundColor Green
    $awsVersion = aws --version
    Write-Host "   Version: $awsVersion" -ForegroundColor Gray
} else {
    Write-Host "   ? AWS CLI is NOT installed" -ForegroundColor Yellow
    Write-Host "   Install from: https://aws.amazon.com/cli/" -ForegroundColor Gray
}

Write-Host ""

# Test 3: Test AWS Credentials (if CLI is available)
if ($awsCli -and $awsAccessKey -and $awsSecretKey) {
    Write-Host "3. Testing AWS Credentials..." -ForegroundColor Yellow
    
    # Configure AWS CLI temporarily
    $env:AWS_ACCESS_KEY_ID = $awsAccessKey
    $env:AWS_SECRET_ACCESS_KEY = $awsSecretKey
    if ($awsRegion) {
        $env:AWS_DEFAULT_REGION = $awsRegion
    }
    
    $stsTest = aws sts get-caller-identity 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Credentials are VALID" -ForegroundColor Green
        $stsTest | ConvertFrom-Json | Format-List
    } else {
        Write-Host "   ? Credentials are INVALID" -ForegroundColor Red
        Write-Host "   Error: $stsTest" -ForegroundColor Red
    }
}

Write-Host ""

# Test 4: Test S3 Bucket Access (if CLI is available and bucket is set)
if ($awsCli -and $awsAccessKey -and $awsSecretKey -and $awsBucket) {
    Write-Host "4. Testing S3 Bucket Access..." -ForegroundColor Yellow
    
    $s3Test = aws s3 ls "s3://$awsBucket" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Bucket '$awsBucket' is ACCESSIBLE" -ForegroundColor Green
        Write-Host "   Files in bucket:" -ForegroundColor Gray
        $s3Test | Select-Object -First 5
    } else {
        Write-Host "   ? Bucket '$awsBucket' is NOT ACCESSIBLE" -ForegroundColor Red
        Write-Host "   Error: $s3Test" -ForegroundColor Red
    }
}

Write-Host ""

# Test 5: Check S3 Endpoint Connectivity
Write-Host "5. Testing S3 Endpoint Connectivity..." -ForegroundColor Yellow
$region = if ($awsRegion) { $awsRegion } else { "us-east-1" }
$endpoint = "s3.$region.amazonaws.com"

$pingTest = Test-NetConnection -ComputerName $endpoint -Port 443 -InformationLevel Quiet -ErrorAction SilentlyContinue
if ($pingTest) {
    Write-Host "   ? Can connect to $endpoint on port 443" -ForegroundColor Green
} else {
    Write-Host "   ? Cannot connect to $endpoint on port 443" -ForegroundColor Red
    Write-Host "   This may indicate firewall or network issues" -ForegroundColor Yellow
}

Write-Host ""

# Test 6: Upload Test File (if everything else passed)
if ($awsCli -and $awsAccessKey -and $awsSecretKey -and $awsBucket -and $pingTest) {
    Write-Host "6. Testing File Upload..." -ForegroundColor Yellow
    
    $testFile = "s3-test-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
    $testContent = "Test upload at $(Get-Date)"
    $testContent | Out-File -FilePath $testFile -Encoding UTF8
    
    $uploadTest = aws s3 cp $testFile "s3://$awsBucket/$testFile" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? File upload SUCCESSFUL" -ForegroundColor Green
        Write-Host "   File: s3://$awsBucket/$testFile" -ForegroundColor Gray
        
        # Clean up
        aws s3 rm "s3://$awsBucket/$testFile" 2>&1 | Out-Null
        Remove-Item $testFile
    } else {
        Write-Host "   ? File upload FAILED" -ForegroundColor Red
        Write-Host "   Error: $uploadTest" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
Write-Host ""

# Summary
Write-Host "Summary:" -ForegroundColor Cyan
if (-not $awsAccessKey -or -not $awsSecretKey -or -not $awsBucket) {
    Write-Host "? Missing required environment variables. Set them in your .env file and restart your application." -ForegroundColor Yellow
}
elseif (-not $pingTest) {
    Write-Host "? Network connectivity issue. Check your firewall or VPN settings." -ForegroundColor Yellow
}
else {
    Write-Host "? All tests passed! Your AWS S3 configuration should work." -ForegroundColor Green
}

Write-Host ""
Write-Host "If you're still experiencing issues, check S3_TROUBLESHOOTING.md for detailed solutions." -ForegroundColor Gray
