#!/bin/sh

set -e

# NB - as currently written, this script has only been tested on Mac OSX
# In particular, Mac makes use of a variant of sed which might not work on other UNIX variants

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

#############
# VARIABLES #
#############

packageName='RadixCoreApi.Generated'
outputDirectory="../generated-dependencies"
packageVersionLocation="../Directory.Packages.props"
specLocation='./core-api-spec.yaml'

patchVersion="$1" # Patch version override as first command line parameter

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

openApiSpecVersionMajor="$(echo $openApiSpecVersion | cut -d"." -f 1)"
openApiSpecVersionMinor="$(echo $openApiSpecVersion | cut -d"." -f 2)"
openApiSpecVersionPatch="$(echo $openApiSpecVersion | cut -d"." -f 3)"

if [[ -d "$outputDirectory" ]]; then
    previousPackageVersion=`ls "$outputDirectory" | grep $packageName | sed "s/$packageName\.//" | sed "s/\.nupkg//"`
fi

if [[ ! -z "$previousPackageVersion" ]]; then
    previousPackageVersionMajor="$(echo $previousPackageVersion | cut -d"." -f 1)"
    previousPackageVersionMinor="$(echo $previousPackageVersion | cut -d"." -f 2)"
    previousPackageVersionPatch="$(echo $previousPackageVersion | cut -d"." -f 3)"

    if [[ -z "$patchVersion" ]]; then
        patchVersion=$((previousPackageVersionPatch+1))
    fi

    packageVersion="$openApiSpecVersionMajor.$openApiSpecVersionMinor.$patchVersion"
    packageVersionMajor="$(echo $packageVersion | cut -d"." -f 1)"
    packageVersionMinor="$(echo $packageVersion | cut -d"." -f 2)"
    packageVersionPatch="$(echo $packageVersion | cut -d"." -f 3)"

    if [[ "$packageVersionMajor" -lt "$previousPackageVersionMajor" ]]; then
        echo "Version $packageVersion must be larger than last version $previousPackageVersion"
        echo "Bump the API spec version or specify an increase in the patch version using the first command line parameter to this script."
        echo "Ensuring we don't repeat versions is important so that NuGeT doesn't cache the previous version (which can interfere with your IDE)."
        echo "If you wish to ignore this error, just delete the old package from the $outputDirectory folder."
        exit 1
    fi
    if [[ "$packageVersionMajor" -eq "$previousPackageVersionMajor" && "$packageVersionMinor" -lt "$previousPackageVersionMinor" ]]; then
        echo "Version $packageVersion must be larger than last version $previousPackageVersion"
        echo "Bump the API spec version or specify an increase in the patch version using the first command line parameter to this script."
        echo "Ensuring we don't repeat versions is important so that NuGeT doesn't cache the previous version (which can interfere with your IDE)."
        echo "If you wish to ignore this error, just delete the old package from the $outputDirectory folder."
        exit 1
    fi
    if [[ "$packageVersionMajor" -eq "$previousPackageVersionMajor" && "$packageVersionMinor" -eq "$previousPackageVersionMinor" && "$packageVersionPatch" -le "$previousPackageVersionPatch" ]]; then
        echo "Version $packageVersion must be larger than last version $previousPackageVersion"
        echo "Bump the API spec version or specify an increase in the patch version using the first command line parameter to this script."
        echo "Ensuring we don't repeat versions is important so that NuGeT doesn't cache the previous version (which can interfere with your IDE)."
        echo "If you wish to ignore this error, just delete the old package from the $outputDirectory folder."
        exit 1
    fi

    echo "Building generated client $packageName.$packageVersion.nupkg against open api spec version $openApiSpecVersion, replacing old $packageName.$previousPackageVersion.nupkg"
    echo
else
    if [ ! -z "$patchVersion" ]; then
        patchVersion="0"
    fi

    packageVersion="$openApiSpecVersionMajor.$openApiSpecVersionMinor.$patchVersion"
    packageVersionMajor="$(echo $packageVersion | cut -d"." -f 1)"
    packageVersionMinor="$(echo $packageVersion | cut -d"." -f 2)"
    packageVersionPatch="$(echo $packageVersion | cut -d"." -f 3)"

    echo "Building generated client $packageName.$packageVersion.nupkg against open api spec version $openApiSpecVersion"
    echo
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

openapi-generator generate \
    -i "$specLocation" \
    -g csharp-netcore \
    -o "$dummyApiDirectory" \
    --library httpclient \
    --additional-properties=packageName=$packageName,targetFramework=net5.0,packageVersion=$packageVersion,optionalEmitDefaultValues=true

# Fix various issues in the generated code
for f in `find $dummyApiDirectory -name '*.cs'`; do
  if (grep -q "in BaseValidate(" $f) && [[ $f != *"/obj/"* ]]; then
    awk '{sub(/in BaseValidate/,"in base.BaseValidate"); print}' $f > $f.out
    mv $f.out $f
    echo "$f - Performed BaseValidate fix to source code"
  fi
done
# Code to help with testing:
# code $dummyApiDirectory
# exit 1

cd "$dummyApiDirectory"
dotnet pack

cd "$SCRIPT_DIR"
mkdir -p "$outputDirectory"

# Tidy up old versions
find "$outputDirectory" -name "$packageName.*.nupkg" -exec rm {} \;

# Create new versions
find "$dummyApiDirectory/src/$packageName/bin/Debug" -name "*.nupkg" -exec cp {} "$outputDirectory" \;

# Clear up generated api directory
rm -rf "$dummyApiDirectory"

echo
echo "Successfully built client $packageName.$packageVersion.nupkg against open api spec version $openApiSpecVersion."

# Update the version in the packages listing
sed -i.bu -e "s/Include=\"$packageName\" Version=\"[^\"]*\"/Include=\"$packageName\" Version=\"$packageVersion\"/" "$packageVersionLocation"
rm "$packageVersionLocation.bu" # Clear up the back up file from sed if it completes successfully

echo
echo "> And updated $packageVersionLocation to point to $packageName at version $packageVersion"
