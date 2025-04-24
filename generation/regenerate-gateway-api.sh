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
specLocation='../src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-schema.yaml'

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

dummyApiDirectory="$TMPDIR/radix-api-generation/"

rm -rf "$dummyApiDirectory"
mkdir "$dummyApiDirectory"

# We're using our own build/package as OpenAPITools hasn't released develop version with few critical bugfixes yet!
# Pull requests that isn't merged/fixed yet:
# https://github.com/OpenAPITools/openapi-generator/pull/14533
# 6.1.1-custom is generated using Krzysztof's pull request from that branch https://github.com/krzlabrdx/openapi-generator/tree/csharp_improvements

java -jar ./openapi-generator-cli-6.1.1-custom.jar \
    generate \
    -i "$specLocation" \
    -g csharp-netcore \
    -o "$dummyApiDirectory" \
    -t "template-overrides" \
    --library httpclient \
    --inline-schema-name-defaults SKIP_SCHEMA_REUSE=true \
    --additional-properties=packageName=$packageName,targetFramework=net6.0,optionalEmitDefaultValues=true,useOneOfDiscriminatorLookup=true,validatable=false

rm -rf "../src/${packageName}/generated"
cp -R "${dummyApiDirectory}src/${packageName}/" "../src/${packageName}/generated/"
rm "../src/${packageName}/generated/${packageName}.csproj"

./ensure-license-headers.sh

echo "Done"
