$ErrorActionPreference = "Stop"

function Assert-True($cond, $message) {
  if (-not $cond) { throw $message }
}

$helper = Get-Content -Raw "QuietOnTheSetUI\AutostartHelper.cs"
Assert-True ($helper.Contains('return "' + [char]92 + [char]34 + '" + trimmed + "' + [char]92 + [char]34 + '";') -or $helper.Contains('return "\"" + trimmed + "\"";')) "QuoteExecutablePath must wrap path in quotes"
Assert-True ($helper.Contains('StartupApproved')) "Must detect StartupApproved without clearing it"
Assert-True (-not ($helper -match 'StartupApproved[\s\S]{0,200}DeleteValue')) "Must not delete StartupApproved values"

$form = Get-Content -Raw "QuietOnTheSetUI\Form1.cs"
Assert-True ($form.Contains('InitializeAutostartUi')) "Form1 must initialize autostart UI from helper"
Assert-True ($form.Contains('_suppressAutostartEvents')) "Form1 must suppress recursive checkbox events"
Assert-True (-not ($form -match 'SetValue\("silencer", Application\.ExecutablePath')) "Old unquoted SetValue path must be gone"

$nsi = Get-Content -Raw "installer\silencer.nsi"
Assert-True ($nsi.Contains('RequestExecutionLevel user')) "Installer must be per-user"
Assert-True ($nsi.Contains('$LOCALAPPDATA')) "Installer must use LocalAppData"
Assert-True ($nsi.Contains('QuietOnTheSet')) "Installer should clean legacy QuietOnTheSet Run value"
Assert-True ($nsi.Contains('"$INSTDIR\${APP_EXE}"')) "Installer Run value must quote exe path"
Assert-True (-not ($nsi.Contains('Quiet on the Set}'))) "Must not use Quiet on the Set as install folder macro target"

$settings = Get-Content -Raw "QuietOnTheSetUI\Properties\Settings.settings"
Assert-True ($settings -match 'Name="StartAutomatically"[\s\S]*?<Value Profile="\(Default\)">True</Value>') "Default StartAutomatically must be True"

$exe = "QuietOnTheSetUI\bin\Release\silencer.exe"
Assert-True (Test-Path $exe) "Release exe missing"
$versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Resolve-Path $exe))
Assert-True ($versionInfo.FileVersion -like '1.1.0*') "Expected file version 1.1.0.*, got $($versionInfo.FileVersion)"

Write-Host "static-check.ps1 passed"
