# Creates the release's .zip file

$ModName = "Guil-自然颂歌";
$ModFolder = "./Package/" + $ModName
$ArchiveName = "Guil-Nature.zip"


Remove-Item -ErrorAction SilentlyContinue -Recurse ./Package/*
Remove-Item -ErrorAction SilentlyContinue $ArchiveName
mkdir -ErrorAction SilentlyContinue $ModFolder

Copy-Item -Recurse ./$ModName/* $ModFolder

# English name since github strips Unicode for security purposes.
Compress-Archive -DestinationPath $ArchiveName -Path ./Package/*

