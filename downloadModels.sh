#!/usr/bin/env bash
set -euo pipefail

# This script downloads the models required by the tests and examples in this project.

if ! command -v curl >/dev/null 2>&1; then
  echo "curl is required but not installed." >&2
  exit 1
fi

mkdir -p models

download_if_missing() {
  local url="$1"
  local path="$2"

  if [ ! -f "$path" ]; then
    echo "Downloading $path..."
    curl -L --fail --retry 3 --retry-delay 2 -o "$path" "$url"
  fi
}

whisper_onnx_url="https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_tiny_int8_cpu_ort_1.18.0.onnx"
whisper_onnx_path="models/whisper.onnx"

download_if_missing "$whisper_onnx_url" "$whisper_onnx_path"

whisper_ggml_url="https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-base.bin"
whisper_ggml_path="models/ggml-base.bin"

download_if_missing "$whisper_ggml_url" "$whisper_ggml_path"

silero_onnx_url="https://raw.githubusercontent.com/snakers4/silero-vad/refs/heads/master/src/silero_vad/data/silero_vad.onnx"
silero_onnx_path="models/silero_vad.onnx"

download_if_missing "$silero_onnx_url" "$silero_onnx_path"

sherpa_package_url="https://github.com/k2-fsa/sherpa-onnx/releases/download/asr-models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18.tar.bz2"
sherpa_folder="models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18"

if [ ! -d "$sherpa_folder" ]; then
  sherpa_archive="${sherpa_folder}.tar.bz2"
  echo "Downloading $sherpa_archive..."
  curl -L --fail --retry 3 --retry-delay 2 -o "$sherpa_archive" "$sherpa_package_url"
  tar -xjf "$sherpa_archive" -C models
  rm -f "$sherpa_archive"
fi
