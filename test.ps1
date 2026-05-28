try {
    $r = Invoke-WebRequest -Uri 'http://localhost:5049/Admin/Exams/Create' -Method POST -Body 'Input.Title=Math+Final+Exam&Input.Description=Semester+1+Final&Input.StartTime=2026-05-27T16:18&Input.EndTime=2026-05-27T18:28&Input.DurationMinutes=60&Input.DisplayMode=1&Input.TokenRequired=false&Input.RandomizeQuestions=false' -ContentType 'application/x-www-form-urlencoded' -UseBasicParsing;
    Write-Output $r.StatusCode
} catch {
    Write-Output $_.Exception.Response.StatusCode
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    Write-Output $reader.ReadToEnd()
}
