param (
    [Parameter(Mandatory = $true)]
    [ValidateSet("run", "init-s3", "publish", "build-ui")]
    [string]$Command
)

function Init-S3 {
    Write-Host "🧠 Creating S3 bucket and uploading files..." -ForegroundColor Cyan
    docker exec -i localstack awslocal s3 mb s3://medical-files | Out-Null

    docker exec -i localstack awslocal s3 cp /mnt/fake_scans/alice_carter/mri_brain.png `
        s3://medical-files/patients/779553dd-de36-4b19-a414-834af7b3e319/MRI/5240699f-3811-443b-b2a4-655a877a7795/mri_brain.jpg
    docker exec -i localstack awslocal s3 cp /mnt/fake_scans/bob_jensen/cat_abdomen.png `
        s3://medical-files/patients/1e21bca6-c9ec-486e-a8bc-80533080c6bc/CAT_SCAN/f9f6cdcd-dbf5-4735-a456-f7146d5b030b/cat_abdomen.jpg
    docker exec -i localstack awslocal s3 cp /mnt/fake_scans/charlie_kim/doctor_report.pdf `
        s3://medical-files/patients/a520a0d7-f575-4d8c-aff3-e9ebe6646b40/DOCTOR_REPORT/bc62b2d3-8b49-4c9c-b3cb-8efb57a18aa4/doctor_report.pdf

    Write-Host "✅ S3 upload complete." -ForegroundColor Green
}

function Publish-App {
    $publishPath = "./PCMSApi/publish"

    if (Test-Path $publishPath) {
        Write-Host "🧹 Cleaning previous publish output..." -ForegroundColor Yellow
        Remove-Item -Path $publishPath -Recurse -Force
    }

    Write-Host "📦 Publishing .NET app into PCMSApi/publish..." -ForegroundColor Cyan
    dotnet publish ./PCMSApi/PCMSApi.csproj -c Release -o $publishPath /p:UseAppHost=false
    Write-Host "✅ Publish complete." -ForegroundColor Green
}


function Build-UI {
    Write-Host "🛠️ Building React UI..." -ForegroundColor Cyan
    Push-Location ./pcms-ui

    if (-not (Test-Path "node_modules")) {
        Write-Host "📦 Installing dependencies..."
        npm install
    }

    npm run build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ UI build failed." -ForegroundColor Red
        exit 1
    }

    Pop-Location
    Write-Host "✅ UI build complete." -ForegroundColor Green
}

switch ($Command) {
    "run" {
        Publish-App
        Build-UI
        Write-Host "🌀 Running Docker Compose..." -ForegroundColor Cyan
        docker compose down -v
        docker compose up -d
    }
    "init-s3" { Init-S3 }
    "publish" { Publish-App }
    "build-ui" { Build-UI }
}
