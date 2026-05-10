#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/release-assets.sh
source "$SCRIPT_DIR/lib/release-assets.sh"

require_cmd curl
require_cmd jq
require_cmd sha256sum
require_cmd git
require_env FLATHUB_TOKEN
require_env FLATHUB_APP_ID

RELEASE_TAG="$(normalize_release_tag "${1:-}")"
if [ -z "$RELEASE_TAG" ]; then
  echo "Usage: ./scripts/publish-flathub.sh <release_tag>" >&2
  exit 1
fi

RELEASE_VERSION="$(version_from_tag "$RELEASE_TAG")"
MANIFEST_REPO="${FLATHUB_MANIFEST_REPO:-flathub/${FLATHUB_APP_ID}}"
MANIFEST_PATH="${FLATHUB_MANIFEST_PATH:-${FLATHUB_APP_ID}.yml}"
TARGET_BRANCH="${FLATHUB_BRANCH:-master}"

RELEASE_JSON="$(release_json_by_tag "$RELEASE_TAG")"
APPIMAGE_NAME="$(asset_name_by_regex "$RELEASE_JSON" '\.AppImage$')"
if [ -z "$APPIMAGE_NAME" ]; then
  echo "No .AppImage asset found in release '$RELEASE_TAG'." >&2
  exit 1
fi

WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

APPIMAGE_PATH="$WORK_DIR/$APPIMAGE_NAME"
download_asset_by_name "$RELEASE_JSON" "$APPIMAGE_NAME" "$APPIMAGE_PATH"
APPIMAGE_URL="$(asset_url_by_name "$RELEASE_JSON" "$APPIMAGE_NAME")"
APPIMAGE_SHA256="$(sha256sum "$APPIMAGE_PATH" | awk '{print $1}')"

MANIFEST_DIR="$WORK_DIR/flathub"
git clone "https://x-access-token:${FLATHUB_TOKEN}@github.com/${MANIFEST_REPO}.git" "$MANIFEST_DIR"
pushd "$MANIFEST_DIR" >/dev/null
git checkout "$TARGET_BRANCH"

if [ ! -f "$MANIFEST_PATH" ]; then
  echo "Manifest '$MANIFEST_PATH' was not found in ${MANIFEST_REPO}." >&2
  exit 1
fi

sed -E -i "0,/^[[:space:]]*url:[[:space:]]*/s#^[[:space:]]*url:[[:space:]]*.*#  url: ${APPIMAGE_URL}#" "$MANIFEST_PATH"
sed -E -i "0,/^[[:space:]]*sha256:[[:space:]]*/s#^[[:space:]]*sha256:[[:space:]]*.*#  sha256: ${APPIMAGE_SHA256}#" "$MANIFEST_PATH"
sed -E -i "0,/^[[:space:]]*tag:[[:space:]]*/s#^[[:space:]]*tag:[[:space:]]*.*#  tag: ${RELEASE_TAG}#" "$MANIFEST_PATH" || true
sed -E -i "0,/^[[:space:]]*version:[[:space:]]*/s#^[[:space:]]*version:[[:space:]]*.*#  version: ${RELEASE_VERSION}#" "$MANIFEST_PATH" || true

if [ -z "$(git status --porcelain -- "$MANIFEST_PATH")" ]; then
  echo "No manifest changes detected in '$MANIFEST_PATH'."
  exit 0
fi

git config user.name "github-actions"
git config user.email "github-actions@github.com"
git add "$MANIFEST_PATH"
git commit -m "Update ${FLATHUB_APP_ID} to ${RELEASE_VERSION}"
git push origin "HEAD:${TARGET_BRANCH}"
popd >/dev/null

echo "Published Flathub manifest update for ${FLATHUB_APP_ID} (${RELEASE_VERSION})."
