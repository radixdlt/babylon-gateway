#!/usr/bin/env python3
import os
from pathlib import Path

print(f"\nRunning from the root of {Path.cwd()}...\n")

def unix2dos(file_path: Path):
    """Convert Unix (LF) or mixed endings to DOS (CRLF) safely in binary mode."""
    try:
        # Read raw bytes
        raw = file_path.read_bytes()

        # Decode ignoring errors (preserve structure)
        text = raw.decode("utf-8", errors="ignore")

        # Normalize all to LF first
        text = text.replace("\r\n", "\n").replace("\r", "\n")

        # Then convert to CRLF safely
        converted = text.replace("\n", "\r\n")

        # Write back as UTF-8 without modifying content beyond line endings
        file_path.write_bytes(converted.encode("utf-8"))

        print(f"Converted: {file_path}")

    except Exception as e:
        print(f"❌ Failed to convert {file_path}: {e}")


def convert_directory(base_path: Path, pattern: str):
    """Find and convert all matching files recursively."""
    if not base_path.exists():
        print(f"⚠️ Directory not found: {base_path}")
        return
    for file in base_path.rglob(pattern):
        if file.is_file():
            unix2dos(file)


# Targets to process
targets = [
    Path("../src/radixdlt.networkgateway.gatewayapisdk"),
    Path("../src/radixdlt.coreapisdk"),
    Path("../sdk/typescript"),
]

# Convert all matching files
convert_directory(targets[0], "*.cs")
convert_directory(targets[1], "*.cs")
convert_directory(targets[2], "*.ts")

# Convert specific files
extra_files = [
    Path("../sdk/typescript/lib/generated/.openapi-generator-ignore"),
    Path("../sdk/typescript/lib/generated/.openapi-generator/FILES"),
]

for file in extra_files:
    if file.exists():
        unix2dos(file)
    else:
        print(f"⚠️ File not found: {file}")

print("\n✅ Conversion complete!")
