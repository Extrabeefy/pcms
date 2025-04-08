.PHONY: run init-s3 publish

run:
	@echo "🌀 Running Docker Compose..."
	docker compose down -v
	docker compose up --build -d

init-s3:
	@echo "🧠 Creating S3 bucket and uploading files..."
	docker exec -i localstack awslocal s3 mb s3://medical-files
	docker exec -i localstack awslocal s3 cp /mnt/fake_scans/alice_carter/mri_brain.png s3://medical-files/patients/779553dd-de36-4b19-a414-834af7b3e319/MRI/5240699f-3811-443b-b2a4-655a877a7795/mri_brain.jpg
	docker exec -i localstack awslocal s3 cp /mnt/fake_scans/bob_jensen/cat_abdomen.png s3://medical-files/patients/1e21bca6-c9ec-486e-a8bc-80533080c6bc/CAT_SCAN/f9f6cdcd-dbf5-4735-a456-f7146d5b030b/cat_abdomen.jpg
	docker exec -i localstack awslocal s3 cp /mnt/fake_scans/charlie_kim/doctor_report.pdf s3://medical-files/patients/a520a0d7-f575-4d8c-aff3-e9ebe6646b40/DOCTOR_REPORT/bc62b2d3-8b49-4c9c-b3cb-8efb57a18aa4/doctor_report.pdf
	@echo "✅ S3 upload complete."

publish:
	@echo "📦 Publishing .NET API to /PCMSApi/publish..."
	dotnet publish PCMSApi/PCMSApi.csproj -c Release -o PCMSApi/publish /p:UseAppHost=false
	@echo "✅ Publish complete."
