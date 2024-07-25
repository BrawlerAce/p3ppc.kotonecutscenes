# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/p3ppc.kotonecutscenes/*" -Force -Recurse
dotnet publish "./p3ppc.kotonecutscenes.csproj" -c Release -o "$env:RELOADEDIIMODS/p3ppc.kotonecutscenes" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location