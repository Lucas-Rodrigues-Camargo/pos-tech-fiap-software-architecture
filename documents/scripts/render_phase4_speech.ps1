param([Parameter(Mandatory=$true)][string]$Assets)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Speech
$chapters = Get-Content -LiteralPath (Join-Path $Assets 'chapters.json') -Encoding UTF8 -Raw | ConvertFrom-Json
for ($chapterIndex = 0; $chapterIndex -lt $chapters.Count; $chapterIndex++) {
    $speech = New-Object System.Speech.Synthesis.SpeechSynthesizer
    try {
        $speech.SelectVoice('Microsoft Maria Desktop')
        $speech.Rate = 0
        $speech.SetOutputToWaveFile((Join-Path $Assets ('{0:D2}.wav' -f $chapterIndex)))
        $speech.Speak($chapters[$chapterIndex].narration)
    } finally {
        $speech.Dispose()
    }
}
