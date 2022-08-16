#!/bin/sh

set -e

# NB - as currently written, this script has only been tested on Mac OSX
# In particular, Mac makes use of a variant of sed which might not work on other UNIX variants

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

#############
# VARIABLES #
#############

packageName='RadixDlt.NetworkGateway.GatewayApiSdk'
specLocation='../src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-spec.yaml'

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
# - Unlike with the Core API (where we need to preserve 0s on our request objects)
#   we need to not include nulls on our responses. So I've removed the additional option optionalEmitDefaultValues=true
# - Instead, I've forked the generator to set EmitDefaultValues to true for required properties (https://github.com/OpenAPITools/openapi-generator/pull/11607).
# - nullableReferenceTypes is set to false, because it adds the assembly attribute without actually making non-required types nullable

# Use the local forked generator - built from this PR: https://github.com/OpenAPITools/openapi-generator/pull/11607
# TODO NG-64: This can be replaced by either templates (https://openapi-generator.tech/docs/templating) and/or upstream changes / fixes
java -jar ./openapi-generator-cli-PR13049.jar \
    generate \
    -i "$specLocation" \
    -g csharp-netcore \
    -o "$dummyApiDirectory" \
    --library httpclient \
    --additional-properties=packageName=$packageName,targetFramework=net6.0,nullableReferenceTypes=true,useDateTimeOffset=true

rm -rf "../src/${packageName}/generated"
cp -R "${dummyApiDirectory}src/${packageName}/" "../src/${packageName}/generated/"
rm "../src/${packageName}/generated/${packageName}.csproj"

echo "Done"
