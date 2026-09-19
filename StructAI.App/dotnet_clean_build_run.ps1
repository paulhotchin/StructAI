# E:\work\TQ\Kepler\StructAI\StructAI.App\dotnet_clean_build_run.ps1
Write-Host "=== Clean Build Run StructAI.App ==="
dotnet clean
dotnet build --no-restore

if ($LASTEXITCODE -ne 0) {
	throw "Build failed with exit code $LASTEXITCODE."
}

$url = "http://localhost:5208"
$server = Start-Process dotnet `
	-ArgumentList "run --no-build --launch-profile http" `
	-WorkingDirectory $PSScriptRoot `
	-PassThru

Write-Host "Waiting for $url ..."
$ready = $false
for ($attempt = 0; $attempt -lt 60; $attempt++) {
	try {
		$response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 1
		if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
			$ready = $true
			break
		}
	}
	catch {
		if ($server.HasExited) {
			throw "The development server exited with code $($server.ExitCode)."
		}
	}
}

if (-not $ready) {
	throw "The development server did not respond at $url."
}

Write-Host "Opening and refreshing $url"
Start-Process $url

$shell = New-Object -ComObject WScript.Shell
if ($shell.AppActivate("StructAI")) {
	$shell.SendKeys("{F5}")
}
