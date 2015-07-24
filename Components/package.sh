#!/bin/sh

VERSION=$1
if [ -z "$VERSION" ]; then
    echo "Must specify version."
    exit 1
fi

zip LiveSplit.AdventRising_v${VERSION}.zip LiveSplit.AdventRising.dll ../readme.txt
