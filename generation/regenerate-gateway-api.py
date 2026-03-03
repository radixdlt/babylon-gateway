#!/usr/bin/env python3
import os
import sys
import shutil
import subprocess
import tempfile
from pathlib import Path

# Exit immediately on error
def run(cmd, **kwargs):
    print(f"> {' '.join(cmd)}")
    subprocess.run(cmd, check=True, **kwargs)


##################
# INITIAL SETUP  #
##################

script_dir = Path(__file__).resolve().parent
os.chdir(script_dir)

#############
# VARIABLES #
#############

package_name = "RadixDlt.NetworkGateway.GatewayApiSdk"
spec_location = Path("../src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-schema.yaml")

################
# VALIDATION   #
################

if not spec_location.is_file():
    print(f"Couldn't find spec at {script_dir / spec_location}")
    sys.exit(1)

#########
# REGEN #
#########

# Use a system temp directory
dummy_api_dir = Path(tempfile.gettempdir()) / "radix-api-generation"

if dummy_api_dir.exists():
    shutil.rmtree(dummy_api_dir)

dummy_api_dir.mkdir(parents=True)

# Note: Adjust the JAR path or version if needed
jar_path = script_dir / "openapi-generator-cli-6.1.1-custom.jar"

# Run openapi-generator
run([
    "java", "-jar", str(jar_path),
    "generate",
    "-i", str(spec_location),
    "-g", "csharp-netcore",
    "-o", str(dummy_api_dir),
    "-t", "template-overrides",
    "--library", "httpclient",
    "--inline-schema-name-defaults", "SKIP_SCHEMA_REUSE=true",
    f"--additional-properties=packageName={package_name},"
    "targetFramework=net6.0,"
    "optionalEmitDefaultValues=true,"
    "useOneOfDiscriminatorLookup=true,"
    "validatable=false"
])

generated_dir = Path(f"../src/{package_name}/generated")

if generated_dir.exists():
    shutil.rmtree(generated_dir)

shutil.copytree(dummy_api_dir / f"src/{package_name}", generated_dir)

# Remove unwanted csproj
csproj_path = generated_dir / f"{package_name}.csproj"
if csproj_path.exists():
    csproj_path.unlink()

# Ensure license headers
run(["python", "./ensure-license-headers.py"])

print("Done ✅")