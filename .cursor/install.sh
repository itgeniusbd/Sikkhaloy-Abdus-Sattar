#!/usr/bin/env bash
#
# Cloud Agent install script for SIKKHALOY-V3.
#
# This is a .NET Framework 4.7.2 solution (ASP.NET Web Forms + libraries) that is
# normally built with Visual Studio / MSBuild on Windows. On the Linux Cloud Agent
# image we use the Mono toolchain (mono + Roslyn-based msbuild) to restore NuGet
# packages and compile the buildable projects, and mono-xsp4 to host the web app.
#
# The script is idempotent: it can run repeatedly against a warm or partially
# prepared machine without side effects.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NUGET_EXE="/usr/local/bin/nuget.exe"

echo "==> [1/5] Installing the Mono toolchain (mono 6.12 + Roslyn msbuild + xsp4)"
# Ubuntu's own 'mono' package (+dfsg) ships without the Roslyn compiler, which the
# real 'msbuild' needs. The Mono project repo provides a full mono (6.12) with
# Roslyn plus a matching msbuild and xsp4, so we install from there.
if ! command -v msbuild >/dev/null 2>&1 || [ ! -d /usr/lib/mono/msbuild/Current/bin/Roslyn ]; then
  sudo apt-get update -y
  sudo DEBIAN_FRONTEND=noninteractive apt-get install -y ca-certificates gnupg curl

  if [ ! -f /usr/share/keyrings/mono-official-archive-keyring.gpg ]; then
    sudo gpg --homedir /tmp --no-default-keyring \
      --keyring /usr/share/keyrings/mono-official-archive-keyring.gpg \
      --keyserver hkp://keyserver.ubuntu.com:80 \
      --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
  fi
  echo "deb [signed-by=/usr/share/keyrings/mono-official-archive-keyring.gpg] https://download.mono-project.com/repo/ubuntu stable-focal main" \
    | sudo tee /etc/apt/sources.list.d/mono-official-stable.list >/dev/null

  sudo apt-get update -y
  sudo DEBIAN_FRONTEND=noninteractive apt-get install -y mono-complete msbuild mono-xsp4
fi

echo "==> [2/5] Syncing TLS certificates into Mono's trust store (needed for NuGet over HTTPS)"
sudo cert-sync /etc/ssl/certs/ca-certificates.crt

echo "==> [3/5] Ensuring nuget.exe is available"
if [ ! -f "$NUGET_EXE" ]; then
  sudo curl -sSL -o "$NUGET_EXE" https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
fi

echo "==> [4/5] Restoring NuGet packages"
cd "$REPO_ROOT"
mono "$NUGET_EXE" restore "SmsService/SmsService.csproj"        -PackagesDirectory packages
mono "$NUGET_EXE" restore "SIKKHALOY V2/EDUCATION.COM.csproj"   -PackagesDirectory packages
mono "$NUGET_EXE" restore "Attendance_API/Attendance_API.csproj" -PackagesDirectory packages || true

echo "==> [5/5] Building the buildable projects (Mono cannot build the WPF/desktop projects)"
# SmsService is referenced by the web app, so build it first.
msbuild "SmsService/SmsService.csproj"      /p:Configuration=Debug /verbosity:minimal /nologo
msbuild "SIKKHALOY V2/EDUCATION.COM.csproj" /p:Configuration=Debug /verbosity:minimal /nologo

echo ""
echo "Environment ready."
echo "  - Build the web app : msbuild \"SIKKHALOY V2/EDUCATION.COM.csproj\" /p:Configuration=Debug"
echo "  - Run the web app   : bash .cursor/run-webapp-mono.sh   (serves http://localhost:8080/Login.aspx)"
