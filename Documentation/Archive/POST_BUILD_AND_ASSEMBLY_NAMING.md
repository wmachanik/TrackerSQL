# Post-Build Event & Assembly Naming - Quick Reference

**Date:** 2026-04-16  
**Status:** ? Complete

---

## ? Post-Build Event Added

The post-build event is now configured in `TrackerSQL.csproj`:

```xml
<Target Name="PostBuildUnblock" AfterTargets="Build">
  <Exec Command="powershell.exe -ExecutionPolicy Bypass -File &quot;$(ProjectDir)UnblockBinaries.ps1&quot;" 
        IgnoreExitCode="true" 
        ContinueOnError="true" />
</Target>
```

**What it does:**
- Runs after every successful build
- Executes `UnblockBinaries.ps1` to unblock all DLLs
- Continues even if the script fails (won't break your build)

**Verify it's working:**
1. Rebuild Solution (Ctrl+Shift+B)
2. Check **Output** window for:
```
Unblocking binaries...
  Unblocked: TrackerDotNet.dll
  ... (other DLLs)
Unblocking complete!
```

---

## ?? Assembly Naming Explained

### Why "TrackerDotNet.dll" not "TrackerSQL.dll"?

**Project File:** `TrackerSQL.csproj`  
**Assembly Name:** `TrackerSQL.dll` ? **UPDATED**  
**Root Namespace:** `TrackerSQL` ? **UPDATED**

**As of 2026-04-16, the namespace has been unified for consistency:**

| Item | Name | Status |
|------|------|--------|
| **Project File** | TrackerSQL.csproj | ? Consistent |
| **Assembly DLL** | TrackerSQL.dll | ? **Updated** (was TrackerDotNet.dll) |
| **Namespaces** | TrackerSQL.* | ? **Updated** (was TrackerDotNet.*) |

**Historical Context:**
- Original project: "Tracker" using Access database
- Migration project: "TrackerSQL" (SQL Server migration)
- **Previous naming:** "TrackerDotNet" (.NET modernization) - REPLACED 2026-04-16
- **Current naming:** "TrackerSQL" (matches project intent)

**Namespace renaming:** See `Documentation/NAMESPACE_RENAME_TRACKERDOTNET_TO_TRACKERSQL.md`

### Code References

All code now uses `TrackerSQL` namespaces:

```csharp
using TrackerSQL.Models;
using TrackerSQL.Repositories;
```

The assembly name now matches both the project file name and the namespaces.

---

## ?? Deployment Considerations

### Development (Current Setup)
? **Post-build script handles everything automatically**
- DLLs unblocked after every build
- No manual intervention needed
- Safe for development environment

### Production Deployment

The post-build script **will NOT run on the server** (this is expected behavior).

#### Deployment Scenario 1: IIS on Windows Server (Most Common)

**Issue:** Windows Defender Application Control is typically **NOT enabled** on production servers.

**Why:** 
- Production servers usually run as IIS Application Pools
- IT teams configure proper security policies
- This is not a common production issue

**If you encounter issues:**
1. Run `UnblockBinaries.ps1` once on the server after deployment
2. OR add exclusion in Windows Defender for the IIS application folder
3. OR have IT team configure Application Control policy

#### Deployment Scenario 2: Azure App Service

**Issue:** Not applicable  
**Why:** Azure App Service doesn't have Windows Defender Application Control

#### Deployment Scenario 3: Azure DevOps / GitHub Actions Build Pipeline

**Issue:** Not applicable  
**Why:** Build agents don't mark DLLs with "Mark of the Web"  
**Result:** Published artifacts are already "unblocked"

#### Deployment Scenario 4: Professional Production (Recommended)

**Solution:** Code-sign the assembly

**Benefits:**
- Windows trusts signed DLLs
- No unblocking needed anywhere
- Professional appearance
- Security best practice

**Cost:** ~$100-500/year for code-signing certificate

**How:**
1. Purchase code-signing certificate (DigiCert, Sectigo, etc.)
2. Configure project to sign assembly:
   ```xml
   <PropertyGroup>
     <SignAssembly>true</SignAssembly>
     <AssemblyOriginatorKeyFile>YourKey.pfx</AssemblyOriginatorKeyFile>
   </PropertyGroup>
   ```

---

## ?? Troubleshooting

### Problem: Build doesn't show "Unblocking binaries..."

**Check:**
1. Output window ? Show output from: **Build**
2. Verify `UnblockBinaries.ps1` exists in project root
3. Rebuild Solution (not just Build)

**Fix if missing:**
```powershell
# Run from project root
powershell.exe -ExecutionPolicy Bypass -File "UnblockBinaries.ps1"
```

### Problem: Assembly still blocked after build

**Possible causes:**
1. PowerShell execution policy too restrictive
2. Antivirus blocking the unblock script
3. Script failed to run

**Fix:**
1. Run `UnblockBinaries.ps1` manually
2. Check Windows Event Viewer for PowerShell errors
3. Temporarily disable real-time protection during build

### Problem: "TrackerDotNet.dll not found" in production

**Causes:**
1. DLL not deployed with application
2. DLL blocked by Windows Defender on server
3. Missing dependencies

**Fix:**
1. Verify DLL is in `bin\` folder on server
2. Run `UnblockBinaries.ps1` on server (once)
3. Check IIS application pool has correct permissions

---

## ?? Related Files

- `TrackerSQL.csproj` - Project configuration (AssemblyName = TrackerDotNet)
- `UnblockBinaries.ps1` - PowerShell script to unblock DLLs
- `Documentation/WorkInProgress/WINDOWS_DEFENDER_FIX.md` - Full documentation
- `QUICK_FIX_WINDOWS_DEFENDER.md` - Quick reference guide

---

## ? Current Status

- ? Post-build event configured
- ? UnblockBinaries.ps1 created
- ? Assembly naming correct (TrackerDotNet.dll)
- ? Namespaces aligned (TrackerDotNet.*)
- ? Development environment working
- ? Production deployment: Test on target server

---

## ?? Next Steps

**For Development:**
1. Test the post-build event (Rebuild Solution)
2. Verify DLLs unblock automatically
3. Continue development as normal

**For Production Deployment:**
1. Deploy to test/staging server first
2. Verify DLLs load correctly
3. If issues arise, run unblock script once
4. Consider code-signing for long-term solution

---

**Status:** ? Complete - No deployment issues expected for standard IIS hosting
