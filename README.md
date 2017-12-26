# Angular-MVC-Application

A web project based on the Angular framework with WebApi on the server side.
It implements simple logic for creating, storing and maintaining articles. Some features:

* Authentication (through session and cookies)
* A simple role model
* Administration module
* User settings
* A possibility for rating articles, leaving comments and making complaints
* A notification system (SignalR)
* SQL as a default DB model provider (Entity Framework)

### Role model

Administrators can review articles if they have taken them over as assignment along with approving the latter and thus making the available for other users reading.
They are also responsible for dealing with complaints (either about comments or a whole article).
Administrators can block other users or make the administrators (and return them back to being regular ones).

## Installing / Getting started

The application is dependant on .NET Framework ([read more](#headPrerequisites)).

All files from the folder WebArticleLibrary should be copied to a catalogue which will be the main one for a web application created through IIS 7+ ([how to do](https://technet.microsoft.com/en-us/library/cc772042(v=ws.10).aspx)).

Installing SQL Server ([read more](#headPrerequisites)) is the next requirement along with altering web.config (lying in the root of the application folder) to write down new parameters for the connection string.

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
* The project was developed in MS Visual Studio 2015 ([the product page](https://www.visualstudio.com/ru/downloads/?rr=https%3A%2F%2Fwww.microsoft.com%2Fru-ru%2FSoftMicrosoft%2Fvs2015ExpressforW10.aspx)).

### Setting up Dev

## Api Reference

The api module is available through the base url address by adding the api postfix and a respective controller along with method and its parameters. For instance, http://localhost/WebArticleLibrary/api/Article/GetArticleTitles. The external api methods (unneccesary parameters are marked with *?*):
* **POST Authentication/LogIn** with the object *{ name, password }*, returns the logged in user's data *{ status, id, name, firstName, lastName, patronymicName, email, photo, showPrivateInfo, insertDate }*
* **POST Authentication/Register** with the object *{ name, password, firstName, lastName, patronymicName?, email }*
* **GET Authentication/LogOut**
* **GET Authentication/Confirm** with the parameter *confirmationId*, which is a GUID number formed and sent in a link to a new user's email when registering in the application
* **GET Info/GetBasicInfo** returns the company's contact data *{ fax, phone, mail,	youtubeLink, facebookLink }*
* **GET Info/GetAboutUsInfo** returns a short description from the AboutUs page *{ aboutUs }*
* **GET Article/GetDefaultCategories** returns an array of the names of default article categories existing as constant strings in the system
* **GET Article/SearchArticles** searches articles by means of the next parameters *author?, tags?* (a part of the string containing categories), *text?* (it's either a part of the description or name field), *dateStart?, dateEnd?* (a date range of when articles were created), *page=1* (a result page number; the number of items per page is the constant of 10), *colIndex=7* (an index for a sorting column; possible values: NAME = 0, AUTHOR = 1,	TAGS = 2,	ASSIGNED_TO = 3, STATUS = 4, TEXT = 5, ID = 6, DATE = 7), *asc=false* (a direction of sorting results: ascending or otherwise descending). It returns the object *{ articles: [{ id, name, tags, insertDate, status, description, authorId }], articleCount, pageLength, userNames: [], estimates: [] }*


## Database

The project database is based upon [SQL Server 2012](https://www.microsoft.com/en-US/download/details.aspx?id=29062); later versions of the product are also possible to make use of.

The database model is built with Entity Framework 6.1.3.

There are 8 tables altogether:
* USER stores users' data such as: preferences, email, personal data, etc.
* ARTICLE keeps all information related to articles
* USER_COMMENT, USER_COMPLAINT, USER_ESTIMATE are connected to both of the tables preceded and contain respective information (comments for articles, complaints related to either an article or a comment, ratings for articles)
* AMENDMENT is needed to store amendments corresponding to an article being reviewed
* ARTICLE_HISTORY contains all events related to a particular article
* USER_NOTIFICATION makes up of data related to events from ARTICLE_HISTORY and connected to a particular user (for instance, a notifications for administrators about a new article being waited for a review)

![alt text](https://github.com/Jahn08/Angular-MVC-Application/blob/master/DB_Diagram.jpg "A database diagram")
