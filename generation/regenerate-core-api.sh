#!/bin/sh

set -e

# NB - as currently written, this script has only been tested on Mac OSX
# In particular, Mac makes use of a variant of sed which might not work on other UNIX variants

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

#############
# VARIABLES #
#############

packageName='RadixDlt.CoreApiSdk'
specLocation='../src/RadixDlt.CoreApiSdk/core-api-spec-copy.yaml'

################
# CALCULATIONS #
################

if [[ ! -f "$specLocation" ]]; then
    echo "Couldn't find spec at $SCRIPT_DIR/$specLocation"
    exit 1
fi

openApiSpecVersion="$(head -5 $specLocation | grep "version: " | cut -d":" -f 2 | cut -d" " -f 2)"

if [[ -z "$openApiSpecVersion" ]]; then
    echo "Couldn't read open api spec version from the first 5 lines of the api spec"
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

dummyApiDirectory="$TMPDIR/radix-api-generation/"

rm -rf "$dummyApiDirectory"
mkdir "$dummyApiDirectory"

## A note on settings used, and fixes:
# - We use optionalEmitDefaultValues=true to ensure optional parameters set to 0 are included;
#   to work around bugs in the generator (see https://github.com/OpenAPITools/openapi-generator/pull/11607).
#   This means 0s and nulls are emitted. The latter isn't technically spec compliant, but the Java Core API doesn't mind.
#   For situations where we need to _not_ emit 0s sometimes, the fields are explicitly set to null in
#   grep fixes in later lines of this script.
#   When we fix the generator to emit nullable reference types, it will be better.
# - For fields where we have to send requests with missing fields (eg EpochUnlock, Epoch in Core API), we manually grep
#   the fields to replace them with nullable fields.
# - We perform other fixes as per NG-64
# - nullableReferenceTypes is set to false, because it adds the assembly attribute without actually making non-required types nullable

# Use the local forked generator - built from this PR: https://github.com/OpenAPITools/openapi-generator/pull/11607
# TODO NG-64: This can be replaced by either templates (https://openapi-generator.tech/docs/templating) and/or upstream changes/fixes
java -jar ./openapi-generator-cli-PR13049.jar \
    generate \
    -i "$specLocation" \
    -g csharp-netcore \
    -o "$dummyApiDirectory" \
    --library httpclient \
    --additional-properties=packageName=$packageName,targetFramework=net6.0,optionalEmitDefaultValues=true,nullableReferenceTypes=false,useDateTimeOffset=true

rm -rf "../src/${packageName}/generated"
cp -R "${dummyApiDirectory}src/${packageName}/" "../src/${packageName}/generated/"
rm "../src/${packageName}/generated/${packageName}.csproj"

echo "Done"
