Get-ChildItem .\ -Include bin,obj -Recurse | ForEach ($_) { Remove-Item $_.FullName -Force -Recurse }
Get-ChildItem .\_Dist -Recurse | ForEach ($_) { Remove-Item $_.FullName -Force -Recurse }