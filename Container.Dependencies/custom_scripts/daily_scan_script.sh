#!/bin/bash

LOGFILE="$2";
DIRTOSCAN="$1";

#TODAY=$(date +%u);

clamscan -r $DIRTOSCAN >> $LOGFILE;

#clamAV error exit code
if [ $? -eq 2]
then
	exit 1;
else
	exit 0;
fi

