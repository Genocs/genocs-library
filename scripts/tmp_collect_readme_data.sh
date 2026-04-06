#!/usr/bin/env bash
set -euo pipefail

out="tmp_readme_data.tsv"
: > "$out"

find src -name README_NUGET.md | grep -v '/_docs/' | sort | while read -r readme; do
  d=$(dirname "$readme")
  csproj=$(find "$d" -maxdepth 1 -name '*.csproj' | head -n1 || true)
  if [[ -z "${csproj}" ]]; then
    continue
  fi

  pkg=$(sed -n 's:.*<PackageId>\(.*\)</PackageId>.*:\1:p' "$csproj" | head -n1)
  if [[ -z "${pkg}" ]]; then
    pkg=$(basename "$csproj" .csproj)
  fi

  desc=$(sed -n 's:.*<Description>\(.*\)</Description>.*:\1:p' "$csproj" | head -n1 | sed 's/^ *//; s/ *$//')
  tfm=$(sed -n 's:.*<TargetFrameworks>\(.*\)</TargetFrameworks>.*:\1:p' "$csproj" | head -n1)
  if [[ -z "${tfm}" ]]; then
    tfm=$(sed -n 's:.*<TargetFramework>\(.*\)</TargetFramework>.*:\1:p' "$csproj" | head -n1)
  fi

  methods=$( (grep -RhoE --include='*Extensions*.cs' 'public[[:space:]]+static[[:space:]]+.*[[:space:]]+(Add|Use|Map|Bind|Configure|Enable)[[:alnum:]_]*[[:space:]]*\(' "$d" 2>/dev/null \
    | sed -E 's/.*[[:space:]]((Add|Use|Map|Bind|Configure|Enable)[[:alnum:]_]*)[[:space:]]*\(.*/\1/' \
    | sort -u \
    | paste -sd ',' -) || true )

  printf '%s|%s|%s|%s|%s\n' "$readme" "$pkg" "$tfm" "$desc" "$methods" >> "$out"
done

echo "WROTE:$out"
wc -l "$out"
