#!/bin/sh

# NB - as currently written, this script has only been tested on Mac OSX
# In particular, Mac makes use of a variant of sed which might not work on other UNIX variants

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

#############
# VARIABLES #
#############

packageVersion='0.4.3' # This needs bumping every time
outputDirectory="../generated"
packageVersionLocation="../Directory.Packages.props"

##########
# CHECKS #
##########

if [ -d $outputDirectory ]; then
  existsCheck=`ls "$outputDirectory" | grep $packageVersion`
else
  existsCheck=''
fi

if [ ! -z "$existsCheck" ]; then
    echo "Package with version $packageVersion has already been built."
    echo "You should bump the version in regenerate-transactions-client.sh so that NuGeT doesn't cache the previous version (which can interfere with your IDE)."
    echo "If you wish to ignore this error, just delete the old package from the $outputDirectory folder."
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
mkdir -p "$outputDirectory"

# Tidy up old versions
find "$outputDirectory" -name RadixCoreApi.GeneratedClient.\*.nupkg -exec rm {} \;

# Create new versions
find "$dummyApiDirectory/src/RadixCoreApi.GeneratedClient/bin/Debug" -name \*.nupkg -exec cp {} "$outputDirectory" \;

# Clear up generated api so that it doesn't interfere with indexes/lookups
rm -rf "$dummyApiDirectory"

# Update the version in the packages listing
sed -i.bu -e "s/Include=\"RadixCoreApi.GeneratedClient\" Version=\"[^\"]*\"/Include=\"RadixCoreApi.GeneratedClient\" Version=\"$packageVersion\"/" "$packageVersionLocation"
rm "$packageVersionLocation.bu" # Clear up the back up file from sed if it completes successfully