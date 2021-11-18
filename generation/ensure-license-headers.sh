#!/bin/sh

set -e

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR/.."

echo
echo "Running from the root of ${PWD}..."
echo

for f in `find . -name '*.cs'`; do

  if (grep -q "Copyright 2021 Radix" $f) || [[ $f == *"/obj/"* ]]; then
    echo "$f - No need to copy the License Header"
  else
    cat ./generation/license_header.txt $f > $f.new
    mv $f.new $f
    echo "$f - License Header prepended"
  fi
done
