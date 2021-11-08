#!/bin/bash

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

#############
# VARIABLES #
#############

packageVersion='0.3.0' # This needs bumping every time

##########
# CHECKS #
##########

if [ -d packed ]; then
  existsCheck=`ls ./packed | grep $packageVersion`
else
  existsCheck=''
fi

if [ ! -z "$existsCheck" ]; then
    echo "Package with version $packageVersion has already been built."
    echo "You should bump the version so that NuGeT doesn't cache the previous version."
    echo "If you wish to ignore this error, just delete the old package from the ./packed folder."
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

dummyApiDirectory="$TMPDIR/radix-generated-api/"

rm -rf "$dummyApiDirectory"
mkdir "$dummyApiDirectory"

openapi-generator generate \
    -i ./core-api-spec.json \
    -g csharp-netcore \
    -o "$dummyApiDirectory" \
    --library httpclient \
    --additional-properties=packageName=RadixCoreApi.GeneratedClient,targetFramework=net5.0,packageVersion=$packageVersion

cd "$dummyApiDirectory"
dotnet pack

cd "$SCRIPT_DIR"
mkdir -p ./packed
find "$dummyApiDirectory/src/RadixCoreApi.GeneratedClient/bin/Debug" -name \*.nupkg -exec cp {} ./packed \;

# Clear up generated api so that it doesn't interfere with indexes/lookups
rm -rf "$dummyApiDirectory"
