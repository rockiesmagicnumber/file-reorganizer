# file-reorganizer
Various console apps built to help rearrange my files

Music Reorganizer:
This is for when you want to pull your music down from youtube music, and you have to use Google Takeout, and it gives you those stupid 2GB zips. This unzips all of the files, zip-by-zip, and uses TagLibSharp to read the ID3/ID4 tags and organizes the files by artist/album/song.

Wallpaper Reorganizer:
I have an 8.5GB folder of just desktop wallpapers. They were all loose in one folder. While that's fine, good luck getting THAT monstrosity to display nicely in a Windows Explorer window. This doesn't actually make anything BETTER, mind you, it just makes it easier to access a chunk of 10 (or whatever the Magic Number may be) at a time. Really, this was just a bad practice in recursive functions.

Media Library Reorganizer:
The goal of this guy is to take a flash drive, point the app at it, tell it where your library lives, and it'll use sha256 checksums to sort and optionally deduplicate your media library by media type, visual media creation date, audio media sorted by artist/album/track, or it all defaults to file creation date if the parser can't find anything smarter.

# TODO
* consider sqlite over jsonbackup - although it's already in an IDictionary, so for now that'll suffice as a manifest, but sqlite will probably help us turn this into an APP app
* add music library support
* add 3gp support
