#!/usr/bin/env python3
import os
import sys
from pathlib import Path

##################
# INITIAL SETUP  #
##################

# Find the script directory and go one level up (project root)
script_dir = Path(__file__).resolve().parent
project_root = script_dir.parent
os.chdir(project_root)

print(f"\nRunning from the root of {Path.cwd()}...\n")

#############
# VARIABLES #
#############

license_header_path = Path("generation/license_header.txt")
debug_mode = len(sys.argv) > 1 and sys.argv[1] == "--debug"

if not license_header_path.exists():
    print(f"❌ License header file not found at {license_header_path}")
    sys.exit(1)

with open(license_header_path, "r", encoding="utf-8") as f:
    license_header = f.read()

#################
# MAIN FUNCTION #
#################

def strip_bom(text: str) -> str:
    """Remove UTF-8 BOM if present."""
    return text.lstrip("\ufeff")

def prepend_license(file_path: Path):
    """Prepend license header if not already present."""
    try:
        if "/obj/" in str(file_path).replace("\\", "/"):
            if debug_mode:
                print(f"{file_path} - Skipped (inside /obj/)")
            return

        content = file_path.read_text(encoding="utf-8")

        if "Copyright 2021 Radix" in content:
            if debug_mode:
                print(f"{file_path} - No need to copy the License Header")
            return

        clean_content = strip_bom(content)
        new_content = license_header + clean_content

        file_path.write_text(new_content, encoding="utf-8")
        print(f"{file_path} - License Header prepended")

    except Exception as e:
        print(f"❌ Error processing {file_path}: {e}")

#################
# FILE SCANNING #
#################

for cs_file in project_root.rglob("*.cs"):
    prepend_license(cs_file)

print("\n✅ License header update complete!")
