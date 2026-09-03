#!/usr/bin/env bash
#
# Run the SIKKHALOY V2 ASP.NET Web Forms app on Linux using Mono's xsp4 server.
#
# The application is designed for Windows + IIS + SQL Server. To host it under
# Mono we run it from a throwaway copy (default: /tmp/sikkhaloy-mono-run) so the
# committed source is never modified, and we apply two dev-only adjustments to
# that copy:
#
#   1. Remove native / Windows-only assemblies from bin that Mono's page-compiler
#      bin-scan cannot load as managed images (Microsoft.Data.SqlClient.SNI.*,
#      the System.Runtime.InteropServices.RuntimeInformation facade).
#   2. Trim <assemblies> entries from Web.config that do not exist on Mono
#      (OData Data.Services, System.Design, Microsoft.Build.*, DirectoryServices,
#      EnterpriseServices, ServiceProcess, ReportViewer, ...).
#
# Pages that hit SQL Server will not work (no Windows-auth SQL Server on Linux),
# but anonymous pages such as Login.aspx render, which demonstrates the runtime.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_SRC="$REPO_ROOT/SIKKHALOY V2"
RUN_DIR="${RUN_DIR:-/tmp/sikkhaloy-mono-run}"
PORT="${PORT:-8080}"
ADDRESS="${ADDRESS:-0.0.0.0}"

if [ ! -f "$APP_SRC/bin/EDUCATION.COM.dll" ]; then
  echo "==> Web app not built yet; building it now..."
  msbuild "$APP_SRC/EDUCATION.COM.csproj" /p:Configuration=Debug /verbosity:minimal /nologo
fi

echo "==> Preparing throwaway run copy at $RUN_DIR"
rm -rf "$RUN_DIR"
mkdir -p "$RUN_DIR"
cp -a "$APP_SRC/." "$RUN_DIR/"

echo "==> Removing Windows-only native/facade assemblies from the run copy's bin"
rm -f "$RUN_DIR"/bin/Microsoft.Data.SqlClient.SNI.*.dll
rm -f "$RUN_DIR"/bin/System.Runtime.InteropServices.RuntimeInformation.dll

echo "==> Trimming Mono-incompatible <assemblies> entries from the run copy's Web.config"
sed -i \
  -e '/assembly="System\.Data\.Services\.Client/d' \
  -e '/assembly="System\.Data\.Services\.Design/d' \
  -e '/assembly="System\.Design/d' \
  -e '/assembly="System\.Web\.DynamicData/d' \
  -e '/assembly="System\.Web\.Entity/d' \
  -e '/assembly="Microsoft\.Build\./d' \
  -e '/assembly="System\.DirectoryServices/d' \
  -e '/assembly="System\.EnterpriseServices/d' \
  -e '/assembly="System\.ServiceProcess/d' \
  -e '/assembly="Microsoft\.ReportViewer\./d' \
  "$RUN_DIR/Web.config"

echo "==> Starting xsp4 on http://$ADDRESS:$PORT  (try /Login.aspx)"
cd "$RUN_DIR"
exec xsp4 --port "$PORT" --address "$ADDRESS" --nonstop
