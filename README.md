
![alt text](https://github.com/Jahn08/Angular-MVC-Application/blob/master/WebArticleLibrary/images/indexIcon.ico)

# Angular-MVC-Application

A web project based on the Angular framework with WebApi on the server side.
It implements simple logic for creating, storing and maintaining articles. Some features:

* Authentication (through the session storage and cookies)
* A simple role model
* An administration module
* User settings
* Rating articles, leaving comments and making complaints
* A notification system (SignalR)
* SQL as a default DB model provider (Entity Framework) ([read more](#headDatabase))

### Role model

Administrators can review articles if they have taken them over as assignment along with approving the latter and thus making them available for other users reading.

They are also responsible for dealing with complaints (either about comments or a whole article).

Administrators can block other users or make them administrators (as well as return them back to being regular ones).

## Installing / Getting started

The application is dependent on .NET Framework ([read more](#headPrerequisites)) along with SQL Server ([read more](#headSettingUpDev)).

There is an MSI-installer for the application lying in the folder WebArticleLibrary.Setup/bin/Release. Firstly, the installer lets opt for the names of the application site and its pool on IIS7. Further, components for the connection string should be set up, namely: an SQL Server name, a database name and preferences for logging in the database (user/password or the integrated security mode) ([how to configure the connection string manually](#headConfiguration)). The installer virtually repeats [this process](https://technet.microsoft.com/en-us/library/cc772042(v=ws.10).aspx).

The first user registered in the application will be granted administrative rights.

## Developing

### Built with

* [Entity Framework 6.1.3](https://www.nuget.org/packages/EntityFramework/6.1.3)
* [Angular 1.6.4](https://www.nuget.org/packages/AngularJS.Core/1.6.4)
* [Bootstrap 3.3.7](https://www.nuget.org/packages/bootstrap/3.3.7)
* [Font Awesome 4.7.0](https://www.nuget.org/packages/FontAwesome/4.7.0)
* [jQuery 2.0.3](https://www.nuget.org/packages/jQuery/2.0.3)
* [Bootstrap-wysiwyg 1.04](https://www.nuget.org/packages/Bootstrap.Wysiwyg/1.0.4)
* [Hotkeys 0.8](https://www.nuget.org/packages/jQuery.Hotkeys/0.8.0.20131227)
* [SignalR 2.2.2](https://www.nuget.org/packages/Microsoft.AspNet.SignalR/2.2.2)
* [Select2](https://www.nuget.org/packages/Select2.js/4.0.3)

### <a name="headPrerequisites"></a>Prerequisites

* [.NET Framework 4.5.2](https://www.microsoft.com/en-ca/download/details.aspx?id=42642)
* The project was developed in MS Visual Studio 2015 ([the product page](https://www.visualstudio.com/ru/downloads/?rr=https%3A%2F%2Fwww.microsoft.com%2Fru-ru%2FSoftMicrosoft%2Fvs2015ExpressforW10.aspx))

### <a name="headSettingUpDev"></a>Setting up Dev

Setting up IIS services is a necessity to start developing. In Windows Components there must be the ASP.NET option turned on and other standard options for IIS services. The IIS management console also ought to be included as it gives an opportunity to configure IIS sites by means of graphic interface.

The developer computer has to have an access to MS SQL Server installed to deploy the database ([read more](#headDatabase)).   

### Deploying / Publishing

To build a new version of the installer project, one should take advantage of the next command in the project folder:
*msbuild /t:Build;CreateInstaller;DeleteTmpFiles Setup.build*. Note, that if there are new targets added in the Setup.build file and necessary to be included in the process, they have to be listed in the expression too.

After building another version of the installer, the respective MSI-file should come up in the folder WebArticleLibrary.Setup/bin/Release alongside a renewed file cab1.cab. Both of them ought to be stored together as they take part in the process of installing.

Should there any mistakes arise during the processes of installing or uninstalling, the next command might help out with logging such activities:
*msiexec /i "WebArticleLibrary.Setup.msi" /l*v "log.log"*

## <a name="headConfiguration"></a>Configuration

web.config lying in the root catalogue of the main project serves as the general file for configuration. In the file section *appSettings* the next parameters can be set: 
* *smtpHost, smtpPort, smtpUserName, smtpPassword* for configuring a mail address which will be used to send messages to the users when registering, resetting password, changing email addresses or their statuses being altered
* *aboutUs, fax, phone, mail, youtubeLink, facebookLink* for storing additional contact information shown in the bottom section of the site

There is also one more imperative parameter in the same file, but in the section *connectionStrings*, where the connection string to the database should be added (or changed). There are already some default preferences (for instance, integrated security is turned on and therefore it implies the Windows integrated security to log in to SQL Server without writing down any passwords; [read more about connection strings](https://msdn.microsoft.com/en-us/library/jj653752%28v=vs.110%29.aspx)).

## Api Reference

The api module is available through the base url address by adding the api postfix and a respective controller along with a method and its parameters. For instance, http://localhost/WebArticleLibrary/api/Article/GetArticleTitles. The external api methods (unnecessary parameters are marked with *?*):
* **POST Authentication/LogIn** with an object *{ name, password }*, returns a logged in user's data *{ status, id, name, firstName, lastName, patronymicName, email, photo, showPrivateInfo, insertDate }*
* **POST Authentication/Register** with the object *{ name, password, firstName, lastName, patronymicName?, email }*
* **GET Authentication/LogOut**
* **GET Authentication/Confirm** with a parameter *confirmationId*, which is a GUID number formed and sent in a link to a new user's email when registering in the application
* **GET Info/GetBasicInfo** returns the company's contact data *{ fax, phone, mail,	youtubeLink, facebookLink }* ([how to configure](#headConfiguration))
* **GET Info/GetAboutUsInfo** returns a short description from the AboutUs page *{ aboutUs }*
* **GET Article/GetDefaultCategories** returns an array of the names of default article categories existing as constant strings in the system
* **GET Article/GetArticleTitles** gives two dictionaries and an outcome as an object *{ articles: { id, authorId, name, tags, insertDate, estimate }, userNames: [] }*, where *userNames* is a dictionary with user identificators as keys along with their names as values
* **GET Article/ViewArticle** accepts two parameters: *id, userId?*. *userId* indicates that the outcome should contain additional information such as related comments, photos, current article rating. The result is *{ article: { id, authorId, assignedToId, name, description, tags, insertDate, status, content }, updatedDate, comments: [{ id, authorId, responseToId, articleId, content, insertDate, status }], userNames: [], userPhotos: [], estimate, curEstimate }*, where *userNames* and *userPhotos* are dictionaries with user identificators as keys along with the respective objects as values
* **GET Article/SearchArticles** searches articles by means of the next parameters: *author?, tags?* (a part of the string containing categories), *text?* (it's either a part of the description or name field), *dateStart?, dateEnd?* (a date range of when articles were created), *page = 1* (a result page number; the number of items per page is the constant of 10), *colIndex = 7* (an index for a sorting column; possible values: NAME = 0, AUTHOR = 1,	TAGS = 2,	ASSIGNED_TO = 3, STATUS = 4, TEXT = 5, ID = 6, DATE = 7), *asc=false* (a direction of sorting results: ascending or otherwise descending). It returns the object *{ articles: [{ id, name, tags, insertDate, status, description, authorId }], articleCount, pageLength, userNames: [], estimates: [] }*, whereas *userNames* and *estimates* are dictionaries with user identificators as keys along with the respective objects as values
* **POST UserInfo/ReplacePassword** accepts an object *{ newPassword, confirmationId }*
* **GET UserInfo/ResetPassword** requires a parameter *email* as an email address where a message for resetting password will be sent if the respective user exists in the database
* **GET UserInfo/ConfirmEmail** requires a parameter *confirmationId*

## <a name="headDatabase"></a>Database

The project database is based upon [SQL Server 2012](https://www.microsoft.com/en-US/download/details.aspx?id=29062); later versions of the product are also possible to make use of.

The database model is built with Entity Framework 6.1.3. When a database being created automatically, a user account under which the web application works has to have proper rights for the action on the respective SQL Server (the dbcreator role). For instance, if the application registered with the ApplicationPoolIdentity credentials, then such a user should be added to the logins of the SQLServer: for DefaultAppPool the name is *IIS APPPOOL\DefaultAppPool*.

There are 8 tables altogether:
* USER stores users' data such as: preferences, email, personal data, etc.
* ARTICLE keeps all information related to articles
* USER_COMMENT, USER_COMPLAINT, USER_ESTIMATE are connected to both of the tables preceded and contain respective information (comments for articles, complaints related to either an article or a comment, ratings for articles)
* AMENDMENT is needed to store amendments corresponding to an article being reviewed
* ARTICLE_HISTORY contains descriptions of all events related to a particular article
* USER_NOTIFICATION is made up of data related to events from ARTICLE_HISTORY and connected to a particular user (for instance, notifications for administrators about a new article being waited for a review)

![alt text](https://github.com/Jahn08/Angular-MVC-Application/blob/master/DB_Diagram.jpg "A database diagram")
