# Azure Static Web App Setup Script
# Run this in PowerShell after installing Azure CLI and GitHub CLI

param(
    [string]$ResourceGroup = "haircuthistory-rg",
    [string]$AppName = "haircuthistory-website",
    [string]$Location = "eastus2",
    [string]$GitHubRepo = "" # e.g., "marko/HaircutHistoryApp"
)

Write-Host "=== Azure Static Web App Setup ===" -ForegroundColor Cyan

# Check if Azure CLI is installed
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "Azure CLI not found. Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Check if GitHub CLI is installed
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "GitHub CLI not found. Install from: https://cli.github.com/" -ForegroundColor Red
    exit 1
}

# Login to Azure
Write-Host "`n1. Logging into Azure..." -ForegroundColor Yellow
az login

# Get GitHub repo if not provided
if ([string]::IsNullOrEmpty($GitHubRepo)) {
    $remoteUrl = git remote get-url origin 2>$null
    if ($remoteUrl -match "github\.com[:/](.+?)(?:\.git)?$") {
        $GitHubRepo = $Matches[1] -replace "\.git$", ""
        Write-Host "   Detected GitHub repo: $GitHubRepo" -ForegroundColor Green
    } else {
        $GitHubRepo = Read-Host "Enter GitHub repo (e.g., username/HaircutHistoryApp)"
    }
}

# Create resource group
Write-Host "`n2. Creating resource group '$ResourceGroup'..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location --output none
Write-Host "   Resource group created" -ForegroundColor Green

# Create Static Web App
Write-Host "`n3. Creating Static Web App '$AppName'..." -ForegroundColor Yellow
$swaResult = az staticwebapp create `
    --name $AppName `
    --resource-group $ResourceGroup `
    --location $Location `
    --sku Free `
    --output json | ConvertFrom-Json

Write-Host "   Static Web App created" -ForegroundColor Green
Write-Host "   URL: https://$($swaResult.defaultHostname)" -ForegroundColor Cyan

# Get deployment token
Write-Host "`n4. Getting deployment token..." -ForegroundColor Yellow
$token = az staticwebapp secrets list `
    --name $AppName `
    --resource-group $ResourceGroup `
    --query "properties.apiKey" `
    --output tsv

Write-Host "   Token retrieved" -ForegroundColor Green

# Login to GitHub CLI
Write-Host "`n5. Logging into GitHub..." -ForegroundColor Yellow
gh auth login

# Add secret to GitHub
Write-Host "`n6. Adding deployment token to GitHub secrets..." -ForegroundColor Yellow
$token | gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --repo $GitHubRepo

Write-Host "   Secret added to GitHub" -ForegroundColor Green

# Summary
Write-Host "`n=== Setup Complete ===" -ForegroundColor Cyan
Write-Host "Static Web App URL: https://$($swaResult.defaultHostname)" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Push your code to trigger deployment:"
Write-Host "   git add . && git commit -m 'Deploy to Azure' && git push origin main"
Write-Host "`n2. (Optional) Add custom domain in Azure Portal"
