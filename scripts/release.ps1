<#
.SYNOPSIS
    Cuts a tm7 release by creating and pushing a version tag, which triggers the
    Release workflow (.github/workflows/release.yml) to build and publish the
    GitHub Release with cross-platform binaries.

.DESCRIPTION
    Computes the next version from the latest existing tag (default: bump the
    patch), shows you exactly what it will do, and — after confirmation — creates
    an annotated tag and pushes it to the remote. Pushing the tag is all that is
    needed; the workflow does the rest.

.PARAMETER Version
    Explicit version to release, e.g. '0.2.0' or '0.2.0-rc.1' (a leading 'v' is
    optional). Overrides the -Major/-Minor/-Patch bump switches.

.PARAMETER Major
    Bump the major version (e.g. 0.3.1 -> 1.0.0).

.PARAMETER Minor
    Bump the minor version (e.g. 0.3.1 -> 0.4.0).

.PARAMETER Patch
    Bump the patch version (e.g. 0.3.1 -> 0.3.2). This is the default. If the latest
    tag is a pre-release (e.g. v1.0.0-rc.1), the default patch bump instead promotes
    it to the stable base (v1.0.0).

.PARAMETER Yes
    Skip the interactive confirmation prompt.

.PARAMETER Remote
    Git remote to push the tag to. Defaults to 'origin'.

.EXAMPLE
    ./scripts/release.ps1
    # Next patch release (v0.3.1 -> v0.3.2), with confirmation.

.EXAMPLE
    ./scripts/release.ps1 -Minor
    # Next minor release (v0.3.1 -> v0.4.0).

.EXAMPLE
    ./scripts/release.ps1 0.5.0 -Yes
    # Release exactly v0.5.0, no prompt.

.EXAMPLE
    ./scripts/release.ps1 -WhatIf
    # Show what would happen without tagging or pushing.
#>
[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'High')]
param(
    [Parameter(Position = 0)]
    [string]$Version,

    [switch]$Major,
    [switch]$Minor,
    [switch]$Patch,

    [switch]$Yes,

    [string]$Remote = 'origin'
)

$ErrorActionPreference = 'Stop'

function Invoke-Git {
    # Run git, capture output, and throw on non-zero exit.
    $output = & git @args 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "git $($args -join ' ') failed:`n$output"
    }
    return $output
}

# --- Resolve repository root and operate from there -------------------------
$repoRoot = (& git rev-parse --show-toplevel 2>$null)
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
    throw 'Not inside a git repository.'
}
Push-Location $repoRoot
try {
    # --- Decide bump type vs explicit version -------------------------------
    $bumpCount = @($Major, $Minor, $Patch | Where-Object { $_ }).Count
    if ($Version -and $bumpCount -gt 0) {
        throw 'Specify either an explicit version or a bump switch (-Major/-Minor/-Patch), not both.'
    }
    if ($bumpCount -gt 1) {
        throw 'Specify only one of -Major, -Minor, or -Patch.'
    }
    $bump = if ($Major) { 'major' } elseif ($Minor) { 'minor' } else { 'patch' }

    # --- Sync tags and inspect repository state -----------------------------
    Write-Host "Fetching tags from '$Remote'..." -ForegroundColor DarkGray
    Invoke-Git fetch --tags --quiet $Remote | Out-Null

    $branch = (Invoke-Git rev-parse --abbrev-ref HEAD).Trim()
    $headSha = (Invoke-Git rev-parse --short HEAD).Trim()
    $headSubject = (Invoke-Git log -1 --pretty=%s).Trim()
    $isDirty = -not [string]::IsNullOrWhiteSpace((& git status --porcelain))

    $latestTag = (& git tag --list 'v*' --sort=-v:refname | Select-Object -First 1)
    if ($latestTag) { $latestTag = $latestTag.Trim() }

    # --- Compute the new version --------------------------------------------
    if ($Version) {
        $clean = $Version.TrimStart('v', 'V')
        if ($clean -notmatch '^\d+\.\d+\.\d+(-[0-9A-Za-z.\-]+)?$') {
            throw "Version '$Version' is not valid semver (expected e.g. 0.2.0 or 0.2.0-rc.1)."
        }
        $newVersion = $clean
    }
    elseif (-not $latestTag) {
        # Fresh repository with no release tags yet.
        $newVersion = '0.1.0'
    }
    else {
        if ($latestTag -notmatch '^v?(\d+)\.(\d+)\.(\d+)(-[0-9A-Za-z.\-]+)?$') {
            throw "Cannot parse latest tag '$latestTag' as a version."
        }
        $maj = [int]$Matches[1]; $min = [int]$Matches[2]; $pat = [int]$Matches[3]
        $latestIsPrerelease = [bool]$Matches[4]
        $newVersion = switch ($bump) {
            'major' { "$($maj + 1).0.0" }
            'minor' { "$maj.$($min + 1).0" }
            'patch' {
                # If the latest tag is a pre-release (e.g. 1.0.0-rc.1), a default
                # patch bump promotes it to the stable base (1.0.0) instead of
                # advancing the patch (1.0.1).
                if ($latestIsPrerelease) { "$maj.$min.$pat" } else { "$maj.$min.$($pat + 1)" }
            }
        }
    }

    $newTag = "v$newVersion"

    # --- Guard against duplicate tags ---------------------------------------
    & git rev-parse -q --verify "refs/tags/$newTag" *> $null
    if ($LASTEXITCODE -eq 0) {
        throw "Tag $newTag already exists locally. Pick a different version."
    }
    if (& git ls-remote --tags $Remote "refs/tags/$newTag") {
        throw "Tag $newTag already exists on '$Remote'. Pick a different version."
    }

    $isPrerelease = $newVersion -match '-'

    # --- Show the plan -------------------------------------------------------
    Write-Host ''
    Write-Host 'Release plan' -ForegroundColor Cyan
    Write-Host ('  Previous tag : {0}' -f ($(if ($latestTag) { $latestTag } else { '(none - first release)' })))
    Write-Host ('  New tag      : {0}{1}' -f $newTag, $(if ($isPrerelease) { '  (pre-release)' } else { '' })) -ForegroundColor Green
    Write-Host ('  Branch       : {0}' -f $branch)
    Write-Host ('  Commit       : {0}  {1}' -f $headSha, $headSubject)
    Write-Host ''

    if ($branch -ne 'main') {
        Write-Warning "You are on '$branch', not 'main'. The release will be built from this commit."
    }
    if ($isDirty) {
        Write-Warning 'Working tree has uncommitted changes. They will NOT be included (the tag points at the committed HEAD).'
    }

    # --- Confirm and execute ------------------------------------------------
    if (-not $Yes -and -not $WhatIfPreference) {
        $answer = Read-Host "Create and push $newTag to '$Remote'? (y/N)"
        if ($answer -notmatch '^(y|yes)$') {
            Write-Host 'Aborted.' -ForegroundColor Yellow
            return
        }
    }

    if ($PSCmdlet.ShouldProcess("$Remote ($newTag)", "Create annotated tag and push")) {
        Invoke-Git tag -a $newTag -m "Release $newTag" | Out-Null
        try {
            Invoke-Git push $Remote $newTag | Out-Null
        }
        catch {
            # Roll back the local tag so a failed push can be retried cleanly.
            & git tag -d $newTag *> $null
            throw
        }

        Write-Host ''
        Write-Host "Pushed $newTag. The Release workflow is now building the binaries." -ForegroundColor Green

        $remoteUrl = (& git remote get-url $Remote 2>$null)
        if ($remoteUrl -match 'github\.com[:/](?<slug>[^/]+/.+?)(?:\.git)?$') {
            $slug = $Matches['slug']
            Write-Host "Watch:   https://github.com/$slug/actions/workflows/release.yml"
            Write-Host "Release: https://github.com/$slug/releases/tag/$newTag"
        }
    }
}
finally {
    Pop-Location
}
