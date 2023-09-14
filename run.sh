#!/bin/bash

CSPROJ_PATH="./main"
PY_FILE_PATH="./translation/translation.py"

# run C# file
pushd "$CSPROJ_PATH"
dotnet run
popd

# run Python file
if [ -f "$PY_FILE_PATH" ]; then
    python "$PY_FILE_PATH"
else
    echo "Error: $PY_FILE_PATH does not exist."
fi
