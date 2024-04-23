#!/bin/sh

set -e

# NB - as currently written, this script has only been tested on Mac OSX
# In particular, Mac makes use of a variant of sed which might not work on other UNIX variants

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

#############
# VARIABLES #
#############

packageName='RadixDlt.CoreApiSdk.Kiota'
specLocation='../src/RadixDlt.CoreApiSdk/core-api-spec-copy.yaml'

################
# CALCULATIONS #
################

if [[ ! -f "$specLocation" ]]; then
    echo "Couldn't find spec at $SCRIPT_DIR/$specLocation"
    exit 1
fi

#########
# REGEN #
#########

# Find dummy diectory
for TMPDIR in "$TMPDIR" "$TMP" /var/tmp /tmp
do
    test -d "$TMPDIR" && break
done

dummyApiDirectory="$TMPDIR/radix-api-generation-kiota/"

rm -rf "$dummyApiDirectory"
mkdir "$dummyApiDirectory"

kiota generate \
  --openapi "$specLocation" \
  --language csharp \
  --output "../src/${packageName}/generated" \
  --namespace-name "${packageName}" \
  --class-name "MyClass" \
  --exclude-backward-compatible \
  --additional-data false \
  --backing-store

./ensure-license-headers.sh

echo "Done"

