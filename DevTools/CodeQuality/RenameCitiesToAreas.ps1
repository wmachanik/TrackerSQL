# Rename Cities to Areas in Lookups.aspx.cs
# Comprehensive find-replace for consistency

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Renaming Cities ? Areas in Code-Behind" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

$file = "C:\SRC\ASP.net\TrackerSQL\Pages\Lookups.aspx.cs"
$content = Get-Content $file -Raw

# Track replacements
$replacements = 0

# Control declarations
$content = $content.Replace("protected TabPanel tabpnlCities;", "protected TabPanel tabpnlAreas;")
$content = $content.Replace("protected UpdatePanel upnlCities;", "protected UpdatePanel upnlAreas;")
$content = $content.Replace("protected GridView gvCities;", "protected GridView gvAreas;")
$content = $content.Replace("protected GridView gvAreaDays;", "protected GridView gvAreaDays;")
$replacements += 4

# Comments
$content = $content.Replace("// gvAreaDays is not always populated", "// gvAreaDays is not always populated")
$content = $content.Replace("// Initialize Cities grid", "// Initialize Areas grid")
$content = $content.Replace("// Cities Grid - Paging Event Handler", "// Areas Grid - Paging Event Handler")
$content = $content.Replace("// Cities Grid - Sorting Event Handler", "// Areas Grid - Sorting Event Handler")
$replacements += 4

# Method names - Event Handlers
$content = $content.Replace("protected void gvCities_PageIndexChanging", "protected void gvAreas_PageIndexChanging")
$content = $content.Replace("protected void gvCities_Sorting", "protected void gvAreas_Sorting")
$content = $content.Replace("protected void gvCities_RowEditing", "protected void gvAreas_RowEditing")
$content = $content.Replace("protected void gvCities_RowCancelingEdit", "protected void gvAreas_RowCancelingEdit")
$content = $content.Replace("protected void gvCities_RowUpdating", "protected void gvAreas_RowUpdating")
$content = $content.Replace("protected void gvCities_OnRowCommand", "protected void gvAreas_OnRowCommand")
$content = $content.Replace("protected void gvCities_OnSelectedIndexChanged", "protected void gvAreas_OnSelectedIndexChanged")
$replacements += 7

# GridView instance references
$content = $content.Replace("this.gvCities.", "this.gvAreas.")
$content = $content.Replace("gvCities.PageIndex", "gvAreas.PageIndex")
$content = $content.Replace("gvCities.EditIndex", "gvAreas.EditIndex")
$content = $content.Replace("gvCities.SelectedDataKey", "gvAreas.SelectedDataKey")
$content = $content.Replace("gvCities.DataSource", "gvAreas.DataSource")
$content = $content.Replace("gvCities.DataBind", "gvAreas.DataBind")
$content = $content.Replace("gvCities.FooterRow", "gvAreas.FooterRow")
$replacements += 7

# gvAreaDays to gvAreaDays
$content = $content.Replace("this.gvAreaDays.", "this.gvAreaDays.")
$content = $content.Replace("gvAreaDays.Visible", "gvAreaDays.Visible")
$content = $content.Replace("gvAreaDays.DataSource", "gvAreaDays.DataSource")
$content = $content.Replace("gvAreaDays.DataBind", "gvAreaDays.DataBind")
$content = $content.Replace("gvAreaDays.EditIndex", "gvAreaDays.EditIndex")
$content = $content.Replace("gvAreaDays.Rows", "gvAreaDays.Rows")
$content = $content.Replace("gvAreaDays.Controls", "gvAreaDays.Controls")
$content = $content.Replace("gvAreaDays.SelectedIndex", "gvAreaDays.SelectedIndex")
$replacements += 8

# Event handler method names for gvAreaDays
$content = $content.Replace("protected void gvAreaDays_OnRowUpdating", "protected void gvAreaDays_OnRowUpdating")
$content = $content.Replace("protected void gvAreaDays_RowEditing", "protected void gvAreaDays_RowEditing")
$content = $content.Replace("protected void gvAreaDays_RowCancelingEdit", "protected void gvAreaDays_RowCancelingEdit")
$content = $content.Replace("protected void gvAreaDays_RowDeleting", "protected void gvAreaDays_RowDeleting")
$content = $content.Replace("protected void gvAreaDays_RowCommand", "protected void gvAreaDays_RowCommand")
$replacements += 5

# UpdatePanel references
$content = $content.Replace("this.upnlCities.Update", "this.upnlAreas.Update")
$replacements += 1

# Method calls
$content = $content.Replace("BindCitiesGrid()", "BindAreasGrid()")
$content = $content.Replace("private void BindCitiesGrid()", "private void BindAreasGrid()")
$content = $content.Replace("BindAreaDaysGrid()", "BindAreaDaysGrid()")
$content = $content.Replace("private void BindAreaDaysGrid()", "private void BindAreaDaysGrid()")
$replacements += 4

# Button click handler
$content = $content.Replace("protected void btnAddArea_Click", "protected void btnAddAreaDay_Click")
$replacements += 1

# ViewState keys
$content = $content.Replace('"CitiesSortExpression"', '"AreasSortExpression"')
$replacements += 1

# Control ID references in code (tbxArea to tbxAreaName, etc.)
$content = $content.Replace('"tbxArea"', '"tbxAreaName"')
$content = $content.Replace('"lblArea"', '"lblAreaName"')
$content = $content.Replace('"lblAreaID"', '"lblAreaID"')
$content = $content.Replace('"btnAreaUpdate"', '"btnAreaUpdate"')
$content = $content.Replace('"btnAreaCancel"', '"btnAreaCancel"')
$content = $content.Replace('"btnAreaEdit"', '"btnAreaEdit"')
$content = $content.Replace('"btnAreaInsert"', '"btnAreaInsert"')
$content = $content.Replace("CommandName.Equals(`"AddArea`")", "CommandName.Equals(`"AddArea`")")
$replacements += 8

# Comments and error messages referring to cities
$content = $content.Replace('"Error loading cities:', '"Error loading areas:')
$content = $content.Replace('"Error updating Area:', '"Error updating area:')
$content = $content.Replace('"Error adding prep day:', '"Error adding area prep day:')
$content = $content.Replace('"Error loading prep days:', '"Error loading area prep days:')
$content = $content.Replace('"Error updating prep days:', '"Error updating area prep days:')
$replacements += 5

# Save the file
Set-Content -Path $file -Value $content -NoNewline

Write-Host ""
Write-Host "? Successfully renamed Cities ? Areas" -ForegroundColor Green
Write-Host "  Total replacements: ~$replacements patterns updated" -ForegroundColor Gray
Write-Host ""
Write-Host "Changes include:" -ForegroundColor White
Write-Host "  - Control declarations (gvCities ? gvAreas, gvAreaDays ? gvAreaDays)" -ForegroundColor Gray
Write-Host "  - Event handler method names" -ForegroundColor Gray
Write-Host "  - Method calls (BindCitiesGrid ? BindAreasGrid)" -ForegroundColor Gray
Write-Host "  - Control ID references (tbxArea ? tbxAreaName)" -ForegroundColor Gray
Write-Host "  - Comments and error messages" -ForegroundColor Gray
Write-Host ""
Write-Host "Next: Rebuild solution to verify" -ForegroundColor Yellow
Write-Host ""
