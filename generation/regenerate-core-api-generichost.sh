#!/bin/sh

set -e

# NB - as currently written, this script has only been tested on Mac OSX
# In particular, Mac makes use of a variant of sed which might not work on other UNIX variants

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

#############
# VARIABLES #
#############

packageName='RadixDlt.CoreApiSdk.GenericHost'
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

dummyApiDirectory="$TMPDIR/radix-api-generation-generichost/"

rm -rf "$dummyApiDirectory"
mkdir "$dummyApiDirectory"

templateOverridesDir="tpl"

# We're using our own build/package as OpenAPITools hasn't released develop version with few critical bugfixes yet!
java -jar ./openapi-generator-cli-7.5.0.jar \
    generate \
    -i "$specLocation" \
    -g csharp \
    -o "$dummyApiDirectory" \
    -t "$templateOverridesDir" \
    --library generichost \
    --additional-properties=packageName=$packageName,targetFramework=net8.0,useOneOfDiscriminatorLookup=true,validatable=false

# https://openapi-generator.tech/docs/generators/csharp/
# TODO configure dateFormat dateTimeFormat
# TODO check disallowAdditionalPropertiesIfNotPresent
# TODO check equatable
# TODO check useSourceGeneration
# TODO give library=restsharp a try

rm -rf "../src/${packageName}/generated"
cp -R "${dummyApiDirectory}src/${packageName}/" "../src/${packageName}/generated/"
rm "../src/${packageName}/generated/${packageName}.csproj"

./ensure-license-headers.sh

echo "Done"

