# This script is about to download the models required by the tests and examples in this project

# Download whisper.onnx from https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_tiny_int8_cpu_ort_1.18.0.onnx

$whisperOnnxUrl = "https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_tiny_int8_cpu_ort_1.18.0.onnx"
$whisperOnnxPath = "models/whisper.onnx"

if (!(Test-Path "models")) {
    New-Item -ItemType Directory -Path "models"
}

if (!(Test-Path $whisperOnnxPath)) {
    Invoke-WebRequest -Uri $whisperOnnxUrl -OutFile $whisperOnnxPath
}

$whisperGgmlUrl = "https://huggingface.co/sandrohanea/whisper.net/resolve/main/q5_0/ggml-base.bin"
$whisperGgmlPath = "models/ggml-base.bin"

if (!(Test-Path $whisperGgmlPath)) {
    Invoke-WebRequest -Uri $whisperGgmlUrl -OutFile $whisperGgmlPath
}

$sileroOnnx = "https://raw.githubusercontent.com/snakers4/silero-vad/refs/heads/master/src/silero_vad/data/silero_vad.onnx"
$sileroOnnxPath = "models/silero_vad.onnx"

if (!(Test-Path $sileroOnnxPath)) {
    Invoke-WebRequest -Uri $sileroOnnx -OutFile $sileroOnnxPath
}

$sherpaPackage = "https://github.com/k2-fsa/sherpa-onnx/releases/download/asr-models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18.tar.bz2"
$sherpaFolder = "models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18"

if (!(Test-Path $sherpaFolder)) {
    $sherpaBz2 = $sherpaFolder + ".tar.bz2"
    Invoke-WebRequest -Uri $sherpaPackage -OutFile $sherpaBz2
    
    Expand-7Zip -ArchiveFileName $sherpaBz2 -TargetPath "models"

    $sherpaTar = $sherpaFolder + ".tar"
    Expand-7Zip -ArchiveFileName $sherpaTar -TargetPath "models"

    Remove-Item -Path $sherpaTar
    Remove-Item -Path $sherpaBz2
}
