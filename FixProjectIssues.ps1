# Automated Fix Script for Duplicate Method Errors

Write-Host "=== Fixing Project Reference Issues ==="

# Method 1: Try to use MSBuild to exclude the file
try {
    $projectDir = Get-Location
    Write-Host "Current directory: $projectDir"
    
    # Find the project file
    $projectFiles = Get-ChildItem -Filter "*.csproj" -Recurse
    if ($projectFiles.Count -gt 0) {
        foreach ($project in $projectFiles) {
            Write-Host "Found project: $($project.FullName)"
            
            # Read project content
            $content = Get-Content $project.FullName -Raw
            
            # Check if it contains the problematic reference
            if ($content -match "Bangla_Result_DirectPrint_Clean") {
                Write-Host "Found problematic reference in: $($project.Name)"
                
                # Remove the line containing the problematic reference
                $lines = Get-Content $project.FullName
                $filteredLines = $lines | Where-Object { $_ -notmatch "Bangla_Result_DirectPrint_Clean" }
                
                # Write back the content
                $filteredLines | Set-Content $project.FullName
                
                Write-Host "Removed problematic reference from project file"
                
                # Also check for any .aspx references
                $aspxContent = $content -replace '.*Bangla_Result_DirectPrint_Clean\.aspx.*\r?\n', ''
                if ($aspxContent -ne $content) {
                    $aspxContent | Set-Content $project.FullName
                    Write-Host "Also removed .aspx reference"
                }
            }
        }
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "=== Manual Steps Required ==="
Write-Host "1. In Visual Studio, go to Solution Explorer"
Write-Host "2. Find 'Bangla_Result_DirectPrint_Clean.aspx.cs' file"
Write-Host "3. Right-click and select 'Exclude from Project'"
Write-Host "4. If you also see 'Bangla_Result_DirectPrint_Clean.aspx', exclude that too"
Write-Host "5. Clean Solution (Build > Clean Solution)"
Write-Host "6. Rebuild Solution (Build > Rebuild Solution)"
Write-Host ""
Write-Host "Script completed."