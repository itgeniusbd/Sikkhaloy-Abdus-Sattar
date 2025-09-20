# PowerShell script to fix project references and remove problematic files

# First, let's try to remove the file from the project
Write-Host "Fixing project references..."

# Try to exclude the problematic file from the project using VS developer tools
try {
    $projectPath = "F:\Abdus Sattar\All Projects\?????????\Sikkhaloy-Abdus-Sattar\SIKKHALOY V2\EDUCATION.COM.csproj"
    
    if (Test-Path $projectPath) {
        Write-Host "Project file found: $projectPath"
        
        # Read the project file content
        $content = Get-Content $projectPath -Raw
        
        # Remove the problematic reference
        $newContent = $content -replace '.*Bangla_Result_DirectPrint_Clean\.aspx\.cs.*\r?\n', ''
        
        # Write back the content
        Set-Content $projectPath $newContent
        
        Write-Host "Removed problematic reference from project file"
    } else {
        Write-Host "Project file not found at: $projectPath"
    }
} catch {
    Write-Host "Error occurred: $($_.Exception.Message)"
}

# Try to build the solution
Write-Host "Attempting to build solution..."
try {
    $solutionPath = "F:\Abdus Sattar\All Projects\?????????\Sikkhaloy-Abdus-Sattar"
    Set-Location $solutionPath
    
    # Find .sln file
    $slnFile = Get-ChildItem -Filter "*.sln" | Select-Object -First 1
    if ($slnFile) {
        Write-Host "Found solution file: $($slnFile.Name)"
        
        # Try to build using MSBuild
        & dotnet build $slnFile.FullName
    } else {
        Write-Host "No solution file found"
    }
} catch {
    Write-Host "Build error: $($_.Exception.Message)"
}

Write-Host "Script completed."