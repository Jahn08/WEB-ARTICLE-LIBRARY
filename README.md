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

Installing SQL Server (([read more](#headPrerequisites))) is the next requirement along with altering web.config (lying in the root of the application folder) to write down new parameters for the connection string.

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
* [SQL Server 2012](https://www.microsoft.com/en-US/download/details.aspx?id=29062)

The project was developed in MS Visual Studio 2015 ([the product page](https://www.visualstudio.com/ru/downloads/?rr=https%3A%2F%2Fwww.microsoft.com%2Fru-ru%2FSoftMicrosoft%2Fvs2015ExpressforW10.aspx)).

### Setting up Dev
