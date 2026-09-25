#!/bin/bash
cd "$(dirname "$0")"
/bin/bash build.sh
result=$?
echo "Press Return to close."
read -r
exit "$result"
