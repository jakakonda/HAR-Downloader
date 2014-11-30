# HAR Downloader #

Download same files as your browser did during website viewing and recreate file structure as it is on the remote server.

```
har-dl.exe file.har [target folder]
```
*[target folder]* is optional parameter, if it's not specified it will be automatically created using and .har filename and and *.dl* suffix 

## Example ##

```
har-dl.exe www.google.com.har ./google
```
