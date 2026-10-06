Desktop app installer folder (SIKKHALOY Hybrid).

Place the latest SIKKHALOY_Hybrid_Setup_x.x.x.exe file here.
The newest .exe in this folder is offered for download from the home page
(Download Desktop App button) and the site header.

AUTO-UPDATE (Hybrid 1.0.9 and later)
Installed apps check /Handeler/Desktop_App_Update.ashx, which reads this folder.
On every release upload BOTH files from Hybrid\Installer\Output:
  1. SIKKHALOY_Hybrid_Setup_x.x.x.exe   (upload first)
  2. SIKKHALOY_Hybrid_Update.json       (upload last, overwrite the old one)
The json (written by Build-Installer.ps1) holds the version, size and SHA-256.
Until the exe named in the json is fully uploaded (same size), no update is offered.
Old setup .exe files can stay or be removed.
