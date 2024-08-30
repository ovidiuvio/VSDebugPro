# Define parameters with default values
param (
    [string]$localRepoPath = ".",                   # Assume current directory is already a cloned repo
    [string]$outputDir = "./source-collection",     # Default output directory
    [string[]]$fileExtensions = @(                  # Array of all known source code file extensions
        ".cs", ".py", ".java", ".js", ".ts", ".html", ".css", ".php", ".cpp", ".c", ".h", ".m", ".mm",
        ".swift", ".kt", ".rb", ".rs", ".go", ".pl", ".sh", ".bat", ".ps1", ".lua", ".r", ".jl", 
        ".scala", ".hs", ".clj", ".fs", ".dart", ".vb", ".sql", ".json", ".xml", ".yaml", ".yml"
    )
)

# Ensure the output directory exists
if (!(Test-Path -Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force
}

# Function to collect specific files
function Collect-Files {
    param (
        [string]$path,             # The directory path to search
        [string[]]$extensions,     # File extensions to search for
        [string]$destinationDir    # Output directory to copy files to
    )

    foreach ($extension in $extensions) {
        # Get files with the specified extension
        $files = Get-ChildItem -Path $path -Recurse -Include "*$extension"

        foreach ($file in $files) {
            $destinationPath = Join-Path -Path $destinationDir -ChildPath $file.Name
            Write-Output "Copying file: $($file.FullName) to $destinationPath"
            Copy-Item -Path $file.FullName -Destination $destinationPath -Force
        }
    }
}

# Collect files in the current directory or specified path
Collect-Files -path $localRepoPath -extensions $fileExtensions -destinationDir $outputDir

Write-Output "Files collected successfully in: $outputDir"
