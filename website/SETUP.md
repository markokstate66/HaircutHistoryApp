# Azure Static Web App Setup

Follow these steps to deploy the HairCut History landing page to Azure Static Web Apps.

## Prerequisites

- Azure account with an active subscription
- GitHub repository with this code pushed

## Step 1: Create Azure Static Web App

### Option A: Via Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **Create a resource**
3. Search for **Static Web App** and select it
4. Click **Create**
5. Fill in the details:
   - **Subscription**: Select your subscription
   - **Resource Group**: Create new or use existing
   - **Name**: `haircuthistory-website` (or your preferred name)
   - **Plan type**: Free
   - **Region**: Choose closest to your users
   - **Source**: GitHub
6. Click **Sign in with GitHub** and authorize
7. Select:
   - **Organization**: Your GitHub org/username
   - **Repository**: `HaircutHistoryApp`
   - **Branch**: `main`
8. In Build Details:
   - **Build Presets**: Custom
   - **App location**: `website`
   - **Api location**: (leave empty)
   - **Output location**: (leave empty)
9. Click **Review + create**, then **Create**

### Option B: Via Azure CLI

```bash
# Login to Azure
az login

# Create resource group (if needed)
az group create --name haircuthistory-rg --location eastus2

# Create Static Web App
az staticwebapp create \
  --name haircuthistory-website \
  --resource-group haircuthistory-rg \
  --source https://github.com/YOUR_USERNAME/HaircutHistoryApp \
  --location eastus2 \
  --branch main \
  --app-location "website" \
  --login-with-github
```

## Step 2: Get Deployment Token

1. In Azure Portal, go to your Static Web App
2. Click **Manage deployment token** in the Overview blade
3. Copy the token

## Step 3: Add GitHub Secret

1. Go to your GitHub repository
2. Navigate to **Settings** > **Secrets and variables** > **Actions**
3. Click **New repository secret**
4. Name: `AZURE_STATIC_WEB_APPS_API_TOKEN`
5. Value: Paste the deployment token from Step 2
6. Click **Add secret**

## Step 4: Trigger Deployment

The deployment will automatically trigger when you:
- Push to `main` branch with changes in `website/` or `docs/`
- Open a PR targeting `main` with changes in those folders

To manually trigger:
```bash
git add .
git commit -m "Deploy website to Azure"
git push origin main
```

## Step 5: Verify Deployment

1. Go to Azure Portal > Your Static Web App
2. Find the URL in the Overview blade (e.g., `https://happy-rock-123456.azurestaticapps.net`)
3. Click to open your deployed website

## Custom Domain (Optional)

1. In Azure Portal, go to your Static Web App
2. Click **Custom domains** in the left menu
3. Click **+ Add**
4. Enter your domain (e.g., `www.haircuthistory.app`)
5. Follow the DNS configuration instructions
6. Azure provides free SSL certificate automatically

## Workflow File

The GitHub Actions workflow is located at:
`.github/workflows/azure-static-web-app.yml`

It will:
- Deploy on push to `main` (when website files change)
- Create preview environments for pull requests
- Clean up preview environments when PRs are closed

## Troubleshooting

### Deployment fails with "Token not found"
- Ensure `AZURE_STATIC_WEB_APPS_API_TOKEN` secret is set correctly
- Regenerate the token in Azure Portal if needed

### 404 errors on routes
- The `staticwebapp.config.json` handles routing
- Ensure it's in the `website/` folder

### Changes not appearing
- Check GitHub Actions for build status
- Clear browser cache or use incognito mode
- It may take 1-2 minutes for changes to propagate
