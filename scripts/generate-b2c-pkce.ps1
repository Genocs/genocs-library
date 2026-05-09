param(
    [Parameter(Mandatory = $true)]
    [string]$TenantName,

    [Parameter(Mandatory = $true)]
    [string]$TenantDomain,

    [Parameter(Mandatory = $true)]
    [string]$Policy,

    [Parameter(Mandatory = $true)]
    [string]$ClientId,

    [Parameter(Mandatory = $true)]
    [string]$RedirectUri,

    [Parameter(Mandatory = $true)]
    [string]$Scope,

    [switch]$OpenAuthorizeUrl
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function New-Base64UrlString {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes
    )

    $encoded = [Convert]::ToBase64String($Bytes)
    return $encoded.TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function New-RandomBytes {
    param(
        [Parameter(Mandatory = $true)]
        [int]$Length
    )

    $bytes = New-Object byte[] $Length
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()

    try {
        $rng.GetBytes($bytes)
    }
    finally {
        $rng.Dispose()
    }

    return $bytes
}

function New-PkceVerifier {
    param(
        [int]$Length = 64
    )

    $buffer = New-RandomBytes -Length $Length
    return New-Base64UrlString -Bytes $buffer
}

$verifier = New-PkceVerifier -Length 64
$verifierBytes = [System.Text.Encoding]::ASCII.GetBytes($verifier)
$sha256 = [System.Security.Cryptography.SHA256]::Create()

try {
    $challengeBytes = $sha256.ComputeHash($verifierBytes)
}
finally {
    $sha256.Dispose()
}

$challenge = New-Base64UrlString -Bytes $challengeBytes

$stateBytes = New-RandomBytes -Length 16
$nonceBytes = New-RandomBytes -Length 16
$state = New-Base64UrlString -Bytes $stateBytes
$nonce = New-Base64UrlString -Bytes $nonceBytes

$encodedRedirectUri = [System.Uri]::EscapeDataString($RedirectUri)
$encodedScope = [System.Uri]::EscapeDataString($Scope)

$authorizeUrl = "https://$TenantName.b2clogin.com/$TenantDomain/$Policy/oauth2/v2.0/authorize?client_id=$ClientId&response_type=code&redirect_uri=$encodedRedirectUri&response_mode=query&scope=$encodedScope&code_challenge=$challenge&code_challenge_method=S256&state=$state&nonce=$nonce"

Write-Host ""
Write-Host "=== Azure AD B2C PKCE Values ===" -ForegroundColor Cyan
Write-Host "Tenant Name   : $TenantName"
Write-Host "Tenant Domain : $TenantDomain"
Write-Host "Policy        : $Policy"
Write-Host "Client Id     : $ClientId"
Write-Host "Redirect Uri  : $RedirectUri"
Write-Host "Scope         : $Scope"
Write-Host ""
Write-Host "Code Verifier : $verifier"
Write-Host "Code Challenge: $challenge"
Write-Host "State         : $state"
Write-Host "Nonce         : $nonce"
Write-Host ""
Write-Host "Authorize URL :" -ForegroundColor Green
Write-Host $authorizeUrl
Write-Host ""
Write-Host "--- demo-app.rest snippets ---" -ForegroundColor Yellow
Write-Host "@b2cCodeVerifier = $verifier"
Write-Host "@b2cCodeChallenge = $challenge"
Write-Host ""

if ($OpenAuthorizeUrl.IsPresent) {
    Start-Process $authorizeUrl
}
