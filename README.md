Imported from BitBucket

1. **Uruchomienie testów z pokryciem:**
   ```bash
   dotnet test --collect:"XPlat Code Coverage"

1. **Uruchomienie testów z pokryciem:**
	```bash
	dotnet tool restore
	reportgenerator -reports:"**/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html