#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/release-assets.sh
source "$SCRIPT_DIR/lib/release-assets.sh"

require_cmd curl
require_cmd jq
require_cmd sha256sum
require_cmd git
require_cmd ssh-keyscan
require_env AUR_SSH_PRIVATE_KEY
require_env AUR_PACKAGE_NAME

RELEASE_TAG="$(normalize_release_tag "${1:-}")"
if [ -z "$RELEASE_TAG" ]; then
  echo "Usage: ./scripts/publish-aur.sh <release_tag>" >&2
  exit 1
fi

RELEASE_VERSION="$(version_from_tag "$RELEASE_TAG")"
PKGVER="${RELEASE_VERSION//-/_}"
PKGNAME="$AUR_PACKAGE_NAME"
PKGDESC="${AUR_PACKAGE_DESC:-Edit.NET terminal text editor}"
PKGURL="${AUR_PACKAGE_URL:-https://github.com/$(github_repository)}"
PKGLICENSE="${AUR_PACKAGE_LICENSE:-MIT}"
PKGARCH="${AUR_PACKAGE_ARCH:-x86_64}"
BIN_NAME="${AUR_BINARY_NAME:-edit-net}"

RELEASE_JSON="$(release_json_by_tag "$RELEASE_TAG")"
APPIMAGE_NAME="$(asset_name_by_regex "$RELEASE_JSON" '\\.AppImage$')"
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

mkdir -p "$HOME/.ssh"
chmod 700 "$HOME/.ssh"
printf '%s\n' "$AUR_SSH_PRIVATE_KEY" > "$HOME/.ssh/id_ed25519"
chmod 600 "$HOME/.ssh/id_ed25519"
ssh-keyscan aur.archlinux.org >> "$HOME/.ssh/known_hosts"
chmod 644 "$HOME/.ssh/known_hosts"

AUR_DIR="$WORK_DIR/aur"
git clone "ssh://aur@aur.archlinux.org/${PKGNAME}.git" "$AUR_DIR"

cat > "$AUR_DIR/PKGBUILD" <<EOF
pkgname=$PKGNAME
pkgver=$PKGVER
pkgrel=1
pkgdesc='$PKGDESC'
arch=('$PKGARCH')
url='$PKGURL'
license=('$PKGLICENSE')
depends=('glibc')
source=("\${pkgname}-\${pkgver}.AppImage::$APPIMAGE_URL")
sha256sums=('$APPIMAGE_SHA256')

package() {
  install -Dm755 "\${pkgname}-\${pkgver}.AppImage" "\${pkgdir}/usr/bin/$BIN_NAME"
}
EOF

cat > "$AUR_DIR/.SRCINFO" <<EOF
pkgbase = $PKGNAME
  pkgdesc = $PKGDESC
  pkgver = $PKGVER
  pkgrel = 1
  url = $PKGURL
  arch = $PKGARCH
  license = $PKGLICENSE
  depends = glibc
  source = \${pkgname}-\${pkgver}.AppImage::$APPIMAGE_URL
  sha256sums = $APPIMAGE_SHA256

pkgname = $PKGNAME
EOF

pushd "$AUR_DIR" >/dev/null
if git diff --quiet -- PKGBUILD .SRCINFO; then
  echo "AUR package '$PKGNAME' is already up to date for $RELEASE_TAG."
  exit 0
fi

git config user.name "github-actions"
git config user.email "github-actions@github.com"
git add PKGBUILD .SRCINFO
git commit -m "Update $PKGNAME to $PKGVER"
git push origin HEAD
popd >/dev/null

echo "Published AUR update for $PKGNAME ($PKGVER)."
