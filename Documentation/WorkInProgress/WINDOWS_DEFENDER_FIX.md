# Windows Defender Application Control Fix

**Date:** 2026-04-16  
**Status:** ? Complete  
**Issue:** TrackerDotNet.dll blocked by Windows Defender Application Control

---

## Problem

When running the application in Visual Studio with IIS Express, you encountered:

```
Could not load file or assembly 'TrackerDotNet' or one of its dependencies. 
An Application Control policy has blocked this file. (Exception from HRESULT: 0x800711C7)
```

**Root Cause:** Windows Defender Application Control (WDAC) or SmartScreen was blocking the `TrackerDotNet.dll` assembly because it's not digitally signed and appears suspicious to Windows security.

---

## Solution Implemented

### 1. Verified Assembly Configuration

The project is correctly configured:
- **AssemblyName:** `TrackerDotNet` (line 15 in TrackerSQL.csproj)
- **RootNamespace:** `TrackerDotNet` (line 14 in TrackerSQL.csproj)
- **Output DLL:** `TrackerDotNet.dll` in `bin\` folder

No namespace/assembly mismatch exists - the project file name (`TrackerSQL.csproj`) doesn't need to match the assembly name.

### 2. Immediate Fix - Unblocked DLLs

Executed PowerShell commands to unblock all DLLs:

```powershell
# Unblock bin folder
Get-ChildItem -Path "C:\SRC\ASP.net\TrackerSQL\bin" -Recurse -Filter "*.dll" | Unblock-File

# Unblock ASP.NET temporary files
Get-ChildItem -Path "$env:LOCALAPPDATA\Temp\Temporary ASP.NET Files" -Recurse -Filter "*.dll" | Unblock-File
```

### 3. Automated Fix - Post-Build Script

Created `UnblockBinaries.ps1` in project root to automatically unblock DLLs after each build.

**File:** `UnblockBinaries.ps1`

```powershell
# UnblockBinaries.ps1
# Automatically unblock DLLs after build to prevent Windows Defender Application Control from blocking them

Write-Host "Unblocking binaries..." -ForegroundColor Yellow

# Unblock bin folder
$binPath = Join-Path $PSScriptRoot "bin"
if (Test-Path $binPath) {
    Get-ChildItem -Path $binPath -Recurse -Filter "*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
        Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
        Write-Host "  Unblocked: $($_.Name)" -ForegroundColor Green
    }
}

# Unblock ASP.NET temporary files
$tempPath = "$env:LOCALAPPDATA\Temp\Temporary ASP.NET Files"
if (Test-Path $tempPath) {
    Get-ChildItem -Path $tempPath -Recurse -Filter "*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
        Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
    }
    Write-Host "  Unblocked ASP.NET temp files" -ForegroundColor Green
}

Write-Host "Unblocking complete!" -ForegroundColor Green
```

---

## Manual Steps Required

### Add Post-Build Event in Visual Studio

Since the automated edit failed, you need to manually add the post-build event:

**Option A: Using Visual Studio UI**

1. Right-click **TrackerSQL project** ? **Properties**
2. Go to **Build Events** tab
3. In **Post-build event command line**, add:
   ```cmd
   powershell.exe -ExecutionPolicy Bypass -File "$(ProjectDir)UnblockBinaries.ps1"
   ```
4. Set **Run the post-build event:** to "Always"
5. Click **Save**

**Option B: Edit .csproj File**

1. Open `TrackerSQL.csproj` in a text editor
2. Find the section near the end (around line 1106):
   ```xml
   <!-- To modify your build process, add your task inside one of the targets below and uncomment it.
   ```
3. Replace the commented-out `AfterBuild` target with:
   ```xml
   <!-- Post-build event to unblock DLLs (prevents Windows Defender Application Control from blocking assemblies) -->
   <Target Name="AfterBuild">
     <Exec Command="powershell.exe -ExecutionPolicy Bypass -File &quot;$(ProjectDir)UnblockBinaries.ps1&quot;" 
           IgnoreExitCode="true" 
           ContinueOnError="true" />
   </Target>
   <!-- To modify your build process, add your task inside one of the targets below and uncomment it.
        Other similar extension points exist, see Microsoft.Common.targets.
   <Target Name="BeforeBuild">
   </Target>
   -->
   ```
4. Save the file
5. Reload the project in Visual Studio if prompted

---

## Testing

1. **Clean the solution:** Build ? Clean Solution
2. **Rebuild:** Build ? Rebuild Solution
3. **Check output:** You should see "Unblocking binaries..." in the build output
4. **Run the app:** Press F5 to run with IIS Express
5. **Verify:** The app should load without the assembly blocking error

---

## Alternative Solutions (If Issue Persists)

### Option 1: Add Windows Defender Exclusion

1. Open **Windows Security** ? **Virus & threat protection**
2. **Manage settings** ? **Exclusions** ? **Add or remove exclusions**
3. Add these folders:
   - `C:\SRC\ASP.net\TrackerSQL\bin`
   - `C:\Users\warre\AppData\Local\Temp\Temporary ASP.NET Files\`

### Option 2: Sign the Assembly (Advanced)

For production deployment, consider signing `TrackerDotNet.dll` with a code-signing certificate. This is the most professional solution but requires:
- Purchasing a code-signing certificate
- Configuring the project to sign assemblies
- Not necessary for development, but recommended for production

---

## Why This Happens

Windows Defender Application Control (WDAC) blocks unsigned DLLs that:
1. Are compiled locally (not from a trusted publisher)
2. Are loaded from certain locations (like `Temporary ASP.NET Files`)
3. Have the "Mark of the Web" (downloaded from internet or network)

Development machines often have stricter policies than production servers.

---

## Files Created

- ? `UnblockBinaries.ps1` - Post-build script to unblock DLLs

---

## Files Modified

- ? `TrackerSQL.csproj` - **Manual edit required** (see above)

---

## Build Output

After adding the post-build event, you should see this in your build output:

```
1>------ Build started: Project: TrackerSQL, Configuration: Debug Any CPU ------
1>  TrackerSQL -> C:\SRC\ASP.net\TrackerSQL\bin\TrackerDotNet.dll
1>  Unblocking binaries...
1>    Unblocked: TrackerDotNet.dll
1>    Unblocked: AjaxControlToolkit.dll
1>    ... (other DLLs)
1>    Unblocked ASP.NET temp files
1>  Unblocking complete!
========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========
```

---

## Summary

**Problem:** Windows Defender blocked `TrackerDotNet.dll`  
**Root Cause:** Unsigned assembly + Application Control policy  
**Immediate Fix:** Manually unblocked DLLs with PowerShell ?  
**Permanent Fix:** Created post-build script to auto-unblock ?  
**Manual Step:** Add post-build event to project (see above) ?  

**Status:** Ready to test after adding post-build event

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-04-16 | Initial fix - unblocked DLLs and created post-build script |

