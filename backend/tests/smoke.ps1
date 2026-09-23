param(
    [string]$BaseUrl = "http://localhost:8080",
    [string]$Username = "admin",
    [string]$Password = "ChangeMe123!"
)

$ErrorActionPreference = "Stop"
$projectId = "backend-smoke-$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
$skillTitle = "Backend Smoke $([Guid]::NewGuid().ToString('N').Substring(0, 8))"
$contactId = $null

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw "Assertion failed: $Message"
    }
}

function Get-StatusCode {
    param([scriptblock]$Action)
    try {
        & $Action | Out-Null
        return 200
    }
    catch {
        return [int]$_.Exception.Response.StatusCode
    }
}

try {
    $health = Invoke-RestMethod "$BaseUrl/health"
    Assert-True ($health.status -eq "ok") "health endpoint"

    $cv = Invoke-RestMethod "$BaseUrl/api/cv"
    Assert-True (-not [string]::IsNullOrWhiteSpace($cv.identity.name)) "CV content"

    $loginBody = @{ username = $Username; password = $Password } | ConvertTo-Json -Compress
    $login = Invoke-RestMethod "$BaseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
    Assert-True (-not [string]::IsNullOrWhiteSpace($login.token)) "JWT issuance"
    $auth = @{ Authorization = "Bearer $($login.token)" }

    $unauthorized = Get-StatusCode {
        Invoke-WebRequest "$BaseUrl/api/profile" -Method Put -ContentType "application/json" -Body ($cv.identity | ConvertTo-Json -Compress) -UseBasicParsing
    }
    Assert-True ($unauthorized -eq 401) "unauthenticated writes return 401"

    $invalidCv = $cv | ConvertTo-Json -Depth 100 | ConvertFrom-Json
    $invalidCv.identity.name = ""
    $invalidStatus = Get-StatusCode {
        Invoke-WebRequest "$BaseUrl/api/cv" -Method Put -Headers $auth -ContentType "application/json" -Body ($invalidCv | ConvertTo-Json -Depth 100 -Compress) -UseBasicParsing
    }
    Assert-True ($invalidStatus -eq 400) "invalid CV data returns 400"

    $project = $cv.projects[0] | ConvertTo-Json -Depth 30 | ConvertFrom-Json
    $project.id = $projectId
    $project.name = "Backend Smoke Project"
    $createdProject = Invoke-RestMethod "$BaseUrl/api/projects" -Method Post -Headers $auth -ContentType "application/json" -Body ($project | ConvertTo-Json -Depth 30 -Compress)
    Assert-True ($createdProject.id -eq $projectId) "project creation"
    Assert-True ((Invoke-RestMethod "$BaseUrl/api/projects/$projectId").name -eq "Backend Smoke Project") "project lookup"

    $duplicateStatus = Get-StatusCode {
        Invoke-WebRequest "$BaseUrl/api/projects" -Method Post -Headers $auth -ContentType "application/json" -Body ($project | ConvertTo-Json -Depth 30 -Compress) -UseBasicParsing
    }
    Assert-True ($duplicateStatus -eq 409) "duplicate project returns 409"

    $skill = @{ title = $skillTitle; score = 80; items = @("Validation", "Persistence") }
    $createdSkill = Invoke-RestMethod "$BaseUrl/api/skills" -Method Post -Headers $auth -ContentType "application/json" -Body ($skill | ConvertTo-Json -Compress)
    Assert-True ($createdSkill.title -eq $skillTitle) "skill creation"

    $invalidContactStatus = Get-StatusCode {
        Invoke-WebRequest "$BaseUrl/api/contact" -Method Post -ContentType "application/json" -Body '{"name":"A","email":"wrong","message":"short"}' -UseBasicParsing
    }
    Assert-True ($invalidContactStatus -eq 400) "invalid contact request returns 400"

    $contactBody = @{ name = "Backend Smoke"; email = "smoke@example.com"; message = "Automated backend smoke-test message." } | ConvertTo-Json -Compress
    $contact = Invoke-RestMethod "$BaseUrl/api/contact" -Method Post -ContentType "application/json" -Body $contactBody
    $contactId = $contact.id
    $updatedContact = Invoke-RestMethod "$BaseUrl/api/contact/submissions/$contactId/status" -Method Patch -Headers $auth -ContentType "application/json" -Body '{"status":"Read"}'
    Assert-True ($updatedContact.status -eq "Read") "contact status update"

    Write-Host "All backend smoke tests passed." -ForegroundColor Green
}
finally {
    if ($contactId) {
        try { Invoke-WebRequest "$BaseUrl/api/contact/submissions/$contactId" -Method Delete -Headers $auth -UseBasicParsing | Out-Null } catch { }
    }
    if ($projectId) {
        try { Invoke-WebRequest "$BaseUrl/api/projects/$projectId" -Method Delete -Headers $auth -UseBasicParsing | Out-Null } catch { }
    }
    if ($skillTitle) {
        $skillId = $skillTitle.ToLowerInvariant().Replace(" ", "-")
        try { Invoke-WebRequest "$BaseUrl/api/skills/$skillId" -Method Delete -Headers $auth -UseBasicParsing | Out-Null } catch { }
    }
}
