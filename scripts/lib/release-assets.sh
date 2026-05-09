#!/usr/bin/env bash

set -euo pipefail

GH_API_BASE="${GH_API_BASE:-https://api.github.com}"

normalize_release_tag() {
  local value="${1:-}"
  value="${value#refs/tags/}"
  echo "$value"
}

version_from_tag() {
  local tag
  tag="$(normalize_release_tag "${1:-}")"
  echo "${tag#v}"
}

require_cmd() {
  local command_name="$1"
  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "Required command '$command_name' is not available." >&2
    exit 1
  fi
}

require_env() {
  local env_name="$1"
  if [ -z "${!env_name:-}" ]; then
    echo "Required environment variable '$env_name' is not set." >&2
    exit 1
  fi
}

github_repository() {
  local repository="${GITHUB_REPOSITORY:-${GH_REPO:-}}"
  if [ -z "$repository" ]; then
    echo "Missing repository context. Set GITHUB_REPOSITORY or GH_REPO." >&2
    exit 1
  fi
  echo "$repository"
}

github_api_get() {
  local path="$1"
  local repository_url="${GH_API_BASE}${path}"

  if [ -n "${GITHUB_TOKEN:-}" ]; then
    curl -fsSL \
      -H "Accept: application/vnd.github+json" \
      -H "Authorization: Bearer ${GITHUB_TOKEN}" \
      "$repository_url"
    return
  fi

  if [ -n "${GH_TOKEN:-}" ]; then
    curl -fsSL \
      -H "Accept: application/vnd.github+json" \
      -H "Authorization: Bearer ${GH_TOKEN}" \
      "$repository_url"
    return
  fi

  curl -fsSL -H "Accept: application/vnd.github+json" "$repository_url"
}

release_json_by_tag() {
  local release_tag
  release_tag="$(normalize_release_tag "$1")"
  github_api_get "/repos/$(github_repository)/releases/tags/${release_tag}"
}

asset_name_by_regex() {
  local release_json="$1"
  local pattern="$2"

  echo "$release_json" | jq -r --arg pattern "$pattern" '.assets[] | select(.name | test($pattern)) | .name' | head -n1
}

asset_url_by_name() {
  local release_json="$1"
  local asset_name="$2"

  echo "$release_json" | jq -r --arg name "$asset_name" '.assets[] | select(.name == $name) | .browser_download_url' | head -n1
}

download_asset_by_name() {
  local release_json="$1"
  local asset_name="$2"
  local output_path="$3"

  local asset_url
  asset_url="$(asset_url_by_name "$release_json" "$asset_name")"
  if [ -z "$asset_url" ]; then
    echo "Could not resolve download URL for asset '$asset_name'." >&2
    exit 1
  fi

  curl -fsSL "$asset_url" -o "$output_path"
}
