#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/release-assets.sh
source "$SCRIPT_DIR/lib/release-assets.sh"

require_cmd curl
require_cmd jq
require_cmd sha256sum
require_cmd zip
require_env CHOCOLATEY_API_KEY

RELEASE_TAG="$(normalize_release_tag "${1:-}")"
if [ -z "$RELEASE_TAG" ]; then
  echo "Usage: ./scripts/publish-chocolatey.sh <release_tag>" >&2
  exit 1
fi

RELEASE_VERSION="$(version_from_tag "$RELEASE_TAG")"
PACKAGE_ID="${CHOCOLATEY_PACKAGE_ID:-editnet}"
PACKAGE_TITLE="${CHOCOLATEY_PACKAGE_TITLE:-Edit.NET}"
PACKAGE_AUTHORS="${CHOCOLATEY_PACKAGE_AUTHORS:-Consolonia Team}"
PROJECT_URL="${CHOCOLATEY_PROJECT_URL:-https://github.com/$(github_repository)}"

RELEASE_JSON="$(release_json_by_tag "$RELEASE_TAG")"
INSTALLER_NAME="$(asset_name_by_regex "$RELEASE_JSON" '\\.msi$')"
if [ -z "$INSTALLER_NAME" ]; then
  INSTALLER_NAME="$(asset_name_by_regex "$RELEASE_JSON" '\\.exe$')"
fi

if [ -z "$INSTALLER_NAME" ]; then
  echo "No .msi or .exe asset found in release '$RELEASE_TAG'." >&2
  exit 1
fi

WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

INSTALLER_PATH="$WORK_DIR/$INSTALLER_NAME"
download_asset_by_name "$RELEASE_JSON" "$INSTALLER_NAME" "$INSTALLER_PATH"
INSTALLER_URL="$(asset_url_by_name "$RELEASE_JSON" "$INSTALLER_NAME")"
INSTALLER_SHA256="$(sha256sum "$INSTALLER_PATH" | awk '{print $1}')"

INSTALLER_EXTENSION="${INSTALLER_NAME##*.}"
FILE_TYPE='EXE'
SILENT_ARGS='/S'
if [ "$INSTALLER_EXTENSION" = "msi" ]; then
  FILE_TYPE='MSI'
  SILENT_ARGS='/qn /norestart'
fi

mkdir -p "$WORK_DIR/tools"

cat > "$WORK_DIR/$PACKAGE_ID.nuspec" <<EOF
<?xml version="1.0"?>
<package>
  <metadata>
    <id>$PACKAGE_ID</id>
    <version>$RELEASE_VERSION</version>
    <title>$PACKAGE_TITLE</title>
    <authors>$PACKAGE_AUTHORS</authors>
    <projectUrl>$PROJECT_URL</projectUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>Terminal-based text editor built with Avalonia and Consolonia.</description>
    <tags>terminal editor avalonia consolonia</tags>
  </metadata>
</package>
EOF

cat > "$WORK_DIR/tools/chocolateyinstall.ps1" <<EOF
\$ErrorActionPreference = 'Stop'
\$packageName = '$PACKAGE_ID'
\$url64 = '$INSTALLER_URL'
\$checksum64 = '$INSTALLER_SHA256'

\$packageArgs = @{
  packageName   = \$packageName
  url64bit      = \$url64
  checksum64    = \$checksum64
  checksumType64 = 'sha256'
  fileType      = '$FILE_TYPE'
  silentArgs    = '$SILENT_ARGS'
}

Install-ChocolateyPackage @packageArgs
EOF

NUPKG_PATH="$WORK_DIR/${PACKAGE_ID}.${RELEASE_VERSION}.nupkg"
(cd "$WORK_DIR" && zip -rq "$NUPKG_PATH" .)

curl -fsS --retry 3 --retry-delay 2 \
  -H "X-NuGet-ApiKey: ${CHOCOLATEY_API_KEY}" \
  --upload-file "$NUPKG_PATH" \
  "https://push.chocolatey.org/"

echo "Published Chocolatey package ${PACKAGE_ID} ${RELEASE_VERSION}."
