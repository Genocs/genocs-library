#!/usr/bin/env bash
set -euo pipefail

repo_win=$(pwd -W)
patch_file="tmp_readme_patch.txt"
modified_file="tmp_readme_modified_files.txt"
noep_file="tmp_readme_no_entry_points.txt"

: > "$patch_file"
: > "$modified_file"
: > "$noep_file"

echo '*** Begin Patch' >> "$patch_file"

while IFS='|' read -r readme pkg tfm desc methods; do
  [[ -z "${readme}" ]] && continue
  abs_path="${repo_win}\\${readme//\//\\}"

  echo "$readme" >> "$modified_file"

  {
    echo "*** Delete File: $abs_path"
    echo "*** Add File: $abs_path"
    echo "+# $pkg"
    echo "+"
    echo "+$desc Target frameworks: \`$tfm\`."
    echo "+"
    echo "+## Installation"
    echo "+"
    echo "+- \`dotnet add package $pkg\`"
    echo "+"
    echo "+## Getting Started"
    echo "+"
    if [[ -n "${methods}" ]]; then
      echo "+Register this package in your composition root using the extension methods listed below, then bind required options from configuration."
    else
      echo "+Reference this package from your project and integrate its exposed abstractions and types in your application flow."
      echo "$readme" >> "$noep_file"
    fi
    echo "+"

    if [[ -n "${methods}" ]]; then
      echo "+## Main Entry Points"
      echo "+"
      IFS=',' read -r -a arr <<< "$methods"
      for m in "${arr[@]}"; do
        [[ -z "${m}" ]] && continue
        echo "+- \`$m\`"
      done
      echo "+"
    fi

    echo "+## Support"
    echo "+"
    echo "+- Repository: https://github.com/genocs/genocs-library"
    echo "+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs"
    echo "+"
    echo "+## Release Notes"
    echo "+"
    echo "+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md"
    echo "+- Releases: https://github.com/genocs/genocs-library/releases"
  } >> "$patch_file"
done < tmp_readme_data.tsv

echo '*** End Patch' >> "$patch_file"

echo "PATCH_FILE:$patch_file"
wc -l "$patch_file"
wc -l "$modified_file"
wc -l "$noep_file"
