@echo off
chcp 65001 >nul
echo 正在检测和转换文件编码为UTF-8...
echo.

powershell.exe -ExecutionPolicy Bypass -Command ^
"$textExtensions = @('.txt', '.md', '.cs', '.js', '.json', '.xml', '.html', '.htm', '.css', '.py', '.java', '.cpp', '.c', '.h', '.bat', '.ps1', '.sql', '.log', '.ini', '.cfg', '.conf', '.yaml', '.yml', '.csv', '.tsv', '.sh', '.php', '.rb', '.go', '.rs', '.swift', '.kt', '.scala', '.pl', '.r', '.m', '.mm', '.lua', '.vb', '.fs', '.clj', '.elm', '.dart', '.ts', '.jsx', '.tsx', '.vue', '.svelte', '.toml', '.gitignore', '.gitattributes', '.editorconfig'); ^
Write-Host '正在扫描文件...' -ForegroundColor Yellow; ^
$files = Get-ChildItem -Path '.' -Recurse -File ^| Where-Object { $textExtensions -contains $_.Extension -or ($_.Extension -eq '' -and $_.Name -match '^[^^.]*$' -and (Get-Content $_.FullName -TotalCount 1 -ErrorAction SilentlyContinue) -ne $null) }; ^
$convertedCount = 0; ^
$totalFiles = $files.Count; ^
Write-Host \"找到 $totalFiles 个文本文件\" -ForegroundColor Green; ^
Write-Host ''; ^
foreach ($file in $files) { ^
    try { ^
        if ($file.FullName -match '\\(\.git^|\.vs^|\.idea^|Library^|Temp^|obj^|bin^|node_modules^|Logs)\\') { continue; } ^
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue; ^
        if ($content -eq $null) { continue; } ^
        $bytes = [System.IO.File]::ReadAllBytes($file.FullName); ^
        $encoding = $null; ^
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { ^
            $encoding = 'UTF8-BOM'; ^
        } elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) { ^
            $encoding = 'UTF16-LE'; ^
        } elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) { ^
            $encoding = 'UTF16-BE'; ^
        } else { ^
            try { ^
                $utf8Content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8); ^
                $utf8Bytes = [System.Text.Encoding]::UTF8.GetBytes($utf8Content); ^
                if ([System.Linq.Enumerable]::SequenceEqual($bytes, $utf8Bytes)) { ^
                    $encoding = 'UTF8'; ^
                } else { ^
                    $encoding = 'Other'; ^
                } ^
            } catch { ^
                $encoding = 'Other'; ^
            } ^
        } ^
        if ($encoding -ne 'UTF8' -and $encoding -ne 'UTF8-BOM') { ^
            Write-Host \"转换文件: $($file.FullName) (当前编码: $encoding)\" -ForegroundColor Cyan; ^
            $encodings = @([System.Text.Encoding]::Default, [System.Text.Encoding]::ASCII, [System.Text.Encoding]::Unicode, [System.Text.Encoding]::BigEndianUnicode, [System.Text.Encoding]::UTF32); ^
            $success = $false; ^
            foreach ($enc in $encodings) { ^
                try { ^
                    $content = [System.IO.File]::ReadAllText($file.FullName, $enc); ^
                    $utf8NoBom = New-Object System.Text.UTF8Encoding($false); ^
                    [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom); ^
                    $convertedCount++; ^
                    $success = $true; ^
                    break; ^
                } catch { ^
                    continue; ^
                } ^
            } ^
            if (-not $success) { ^
                Write-Host \"  警告: 无法转换文件 $($file.Name)\" -ForegroundColor Red; ^
            } ^
        } else { ^
        } ^
    } catch { ^
        Write-Host \"  错误: 处理文件 $($file.Name) 时出错: $($_.Exception.Message)\" -ForegroundColor Red; ^
    } ^
} ^
Write-Host ''; ^
Write-Host \"转换完成! 共转换了 $convertedCount 个文件\" -ForegroundColor Green;"

echo.
echo 按任意键继续...
pause >nul 