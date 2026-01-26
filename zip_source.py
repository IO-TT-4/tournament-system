import zipfile
import os

BLACKLIST = {'node_modules', 'bin', 'obj', '.git', '.vs', '.vscode', '.idea', 'dist', 'build'}
OUTPUT_ZIP = 'Finalny_Kod_Zrodlowy.zip'

print(f"Zipping source code to {OUTPUT_ZIP}...")
with zipfile.ZipFile(OUTPUT_ZIP, 'w', zipfile.ZIP_DEFLATED) as zf:
    for root, dirs, files in os.walk('.'):
        # Filter directories in-place
        dirs[:] = [d for d in dirs if d not in BLACKLIST]
        
        for file in files:
            # Skip large files or artifacts
            if file == OUTPUT_ZIP or file.endswith('.docx') or file.endswith('.pdf') or file.endswith('.lock'):
                continue
                
            path = os.path.join(root, file)
            # Add to zip with relative path
            zf.write(path, os.path.relpath(path, '.'))

print("Zip created successfully.")
