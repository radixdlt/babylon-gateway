import sys
from pathlib import Path

if len(sys.argv) != 3:
    print("Usage: python replace_version.py <old_value> <new_value>")
    sys.exit(1)

old_value = sys.argv[1]
new_value = sys.argv[2]

file_path = Path("../deployment/docker-compose.yml")

if not file_path.exists():
    print(f"Error: {file_path} not found")
    sys.exit(1)

# Read file
content = file_path.read_text(encoding="utf-8")

# Replace text
updated_content = content.replace(old_value, new_value)

# Write back
file_path.write_text(updated_content, encoding="utf-8")

print(f"Replaced '{old_value}' with '{new_value}' in {file_path}")