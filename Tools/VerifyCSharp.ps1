param([string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$editorData = Join-Path (Split-Path $UnityEditor -Parent) 'Data'
$templateAssemblies = Join-Path $editorData 'Resources\PackageManager\ProjectTemplates\libcache\com.unity.template.3d-cross-platform-17.0.14\ScriptAssemblies'
$evidence = Join-Path $projectRoot 'Documentation\Evidence'
$output = Join-Path $projectRoot 'Temp\StandaloneCompile'
New-Item -ItemType Directory -Force -Path $output,$evidence | Out-Null
$references = @()
$references += Get-ChildItem (Join-Path $editorData 'NetStandard\ref\2.1.0') -Filter '*.dll' | ForEach-Object FullName
$references += Get-ChildItem (Join-Path $editorData 'NetStandard\compat\2.1.0\shims\netstandard') -Filter '*.dll' | ForEach-Object FullName
$references += Get-ChildItem (Join-Path $editorData 'NetStandard\compat\2.1.0\shims\netfx') -Filter '*.dll' | ForEach-Object FullName
$references += Get-ChildItem (Join-Path $editorData 'Managed\UnityEngine') -Filter '*.dll' | ForEach-Object FullName
$references += Get-ChildItem $templateAssemblies -Filter '*.dll' | Where-Object { $_.Name -notmatch '^Assembly-CSharp' } | ForEach-Object FullName
$references += Join-Path $editorData 'Resources\PackageManager\BuiltInPackages\com.unity.ext.nunit\net40\unity-custom\nunit.framework.dll'
$common = @('-nologo','-target:library','-langversion:9.0','-nostdlib+','-define:UNITY_EDITOR;UNITY_6000_0_OR_NEWER;UNITY_INCLUDE_TESTS;ENABLE_INPUT_SYSTEM')
$common += $references | Select-Object -Unique | ForEach-Object { '-reference:"' + $_ + '"' }
$assemblies = @(
    @{ Name='Nebula.Runtime'; Path='Assets\Nebula\Scripts'; Extra=@() },
    @{ Name='Nebula.Editor'; Path='Assets\Nebula\Editor'; Extra=@('Nebula.Runtime') },
    @{ Name='Nebula.EditModeTests'; Path='Assets\Nebula\Tests\EditMode'; Extra=@('Nebula.Runtime') },
    @{ Name='Nebula.PlayModeTests'; Path='Assets\Nebula\Tests\PlayMode'; Extra=@('Nebula.Runtime') }
)
$log = @('Standalone Roslyn API compilation against installed Unity engine and bundled template assemblies. This does not import assets, run Unity or validate package resolution/runtime behavior.')
foreach ($assembly in $assemblies) {
    $argsForAssembly = $common + @('-out:"' + (Join-Path $output ($assembly.Name+'.dll')) + '"')
    foreach ($dependency in $assembly.Extra) { $argsForAssembly += '-reference:"' + (Join-Path $output ($dependency+'.dll')) + '"' }
    $argsForAssembly += Get-ChildItem (Join-Path $projectRoot $assembly.Path) -Recurse -Filter '*.cs' | ForEach-Object { '"' + $_.FullName + '"' }
    $response = Join-Path $output ($assembly.Name+'.rsp')
    $argsForAssembly | Set-Content -LiteralPath $response -Encoding UTF8
    $result = & (Join-Path $editorData 'NetCoreRuntime\dotnet.exe') (Join-Path $editorData 'DotNetSdkRoslyn\csc.dll') ('@'+$response) 2>&1
    $code = $LASTEXITCODE
    $log += "Assembly $($assembly.Name): exit $code"
    $log += $result | ForEach-Object { "$_" }
    if ($code -ne 0) { $log | Set-Content (Join-Path $evidence 'standalone-csharp.log'); $log; exit $code }
}
$log | Set-Content (Join-Path $evidence 'standalone-csharp.log')
$log
