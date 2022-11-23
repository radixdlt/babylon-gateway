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

# We're using our own build/package as OpenAPITools hasn't released develop version with few critical bugfixes yet!
java -jar ./openapi-generator-cli-6.1.0.jar \
    generate \
    -i "$specLocation" \
    -g csharp-netcore \
    -o "$dummyApiDirectory" \
    --library httpclient \
    --additional-properties=packageName=$packageName,targetFramework=net6.0,optionalEmitDefaultValues=true,useOneOfDiscriminatorLookup=true

rm -rf "../src/${packageName}/generated"
cp -R "${dummyApiDirectory}src/${packageName}/" "../src/${packageName}/generated/"
rm "../src/${packageName}/generated/${packageName}.csproj"

./ensure-license-headers.sh

echo "Done"
