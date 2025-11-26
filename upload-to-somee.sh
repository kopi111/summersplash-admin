#!/bin/bash

# FTP Configuration
FTP_HOST="Geolocation.somee.com"
FTP_USER="kopi"
FTP_PASS='%MZHeY3$yx3-efU'
REMOTE_DIR="/www.Geolocation.somee.com"
SOURCE_DIR="/home/kopi/Desktop/Desktop/SummerSplashAdmin/publish"

echo "======================================================================"
echo "Uploading SummerSplashAdmin to Somee"
echo "======================================================================"

# Function to create remote directory
create_remote_dir() {
    local dir="$1"
    curl -s --ftp-create-dirs -u "$FTP_USER:$FTP_PASS" "ftp://$FTP_HOST$REMOTE_DIR/$dir/" -Q "MKD $REMOTE_DIR/$dir" 2>/dev/null || true
}

# Function to upload a single file
upload_file() {
    local local_file="$1"
    local remote_path="$2"
    echo "Uploading: $remote_path"
    curl -s -T "$local_file" -u "$FTP_USER:$FTP_PASS" "ftp://$FTP_HOST$REMOTE_DIR/$remote_path" --ftp-create-dirs
}

# Get all subdirectories and create them first
cd "$SOURCE_DIR"
find . -type d | while read dir; do
    if [ "$dir" != "." ]; then
        dir_clean="${dir#./}"
        create_remote_dir "$dir_clean"
    fi
done

# Upload all files
uploaded=0
total=$(find . -type f | wc -l)
echo "Total files to upload: $total"
echo ""

find . -type f | while read file; do
    file_clean="${file#./}"
    upload_file "$file" "$file_clean"
    uploaded=$((uploaded + 1))
    echo "[$uploaded/$total] Done"
done

echo ""
echo "======================================================================"
echo "Upload complete!"
echo "======================================================================"
