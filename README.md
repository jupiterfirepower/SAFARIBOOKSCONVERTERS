"# SAFARIBOOKSONLINE CONVERTOR TO HTML"<br> 
Utility for converting books from www.safaribooksonline.com in html format with images.<br>
Browser Google Chrome<br>
Install plugin that download book as zip-archive<br>
Plugin  - Safari Books Download<br>
https://chrome.google.com/webstore/detail/safari-books-download/anlpljppoinmpaedoilhjibjehpjhcob<br>
<br>
Search books on site<br>
https://www.safaribooksonline.com/search/?query=Clojure<br>
Register on site ten days free<br>
<br>
Open solution in VS2017 compile .net core 2.1 app(restore nuget packages)<br>
Run console with parameters<br>
dotnet FileCoverterApp.dll directory_path_to_book [imageWidthInPixels]<br>
Sample<br>
dotnet FileCoverterApp.dll C:\TEMP\BOOKS\learning_scala_programming 900<br>
or<br>
dotnet FileCoverterApp.dll C:\TEMP\BOOKS\learning_scala_programming<br>
imageWidthInPixels=1000 - default value<br>
Result - in current directory two files<br>
resultbook.html - book in html format Original - 170MB  resultbook.html ~12MB<br>
shortbookcode.txt - code from tags pre<br>
<br>
https://html2pdf.com/ - Converting html in more comfortable format pdf.<br>


# safari-download<br>
chrome extension to help download safari books online from https://www.safaribooksonline.com/<br>
<br>
Fixed url in chrome extention for downloads books<br>
<br>
Open chrome://extensions/ in the chrome browser. <br>
Enable Developer Mode - ON<br>
Click button Pack, select root directory for extention. Click button.<br>
After generation two files .crx and .pam<br>
Add the --enable-easy-off-store-extension-install flag when you start chrome (create shortcut, edit target, append the flag).<br>
run from cmd or far manager or edit shortcut add pparameter?<br> 
chrome.exe --enable-easy-off-store-extension-install<br>
Navigate to chrome://extensions/ in the browser.<br>
Drag 'n drop the .crx to the browser, installing the extension.<br>
