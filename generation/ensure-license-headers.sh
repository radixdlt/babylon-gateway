#!/bin/sh

set -e

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR/.."

echo
echo "Running from the root of ${PWD}..."
echo

for f in `find . -name '*.cs'`; do

  if (grep -q "Copyright 2021 Radix" $f) || [[ $f == *"/obj/"* ]]; then
    if [ "$1" == "--debug" ]; then
      echo "$f - No need to copy the License Header"
    fi
  else
    awk 'NR==1{sub(/^\xef\xbb\xbf/,"")}1' $f > $f.nobom
    cat ./generation/license_header.txt $f.nobom > $f.new
    mv $f.new $f
    rm $f.nobom
    echo "$f - License Header prepended"
  fi
done

echo "Running dotnet format..."
echo

dotnet format