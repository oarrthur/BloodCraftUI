# Script PowerShell para corrigir todas as referências do projeto
$projectFile = "BloodCraftUI\BloodCraftUI.OnlyFams.csproj"
$content = Get-Content $projectFile -Raw

# Corrigir todas as referências interop
$content = $content -replace '\.\.\\interop\\', 'interop\'

# Salvar o arquivo corrigido
Set-Content -Path $projectFile -Value $content

Write-Host "Referências corrigidas no arquivo $projectFile"