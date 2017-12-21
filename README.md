# Angular-MVC-Application

A small web project based on the Angular framework with WebApi on the server side.
It implements simple logic for creating, storing and maintaining articles. Some features:

* Authentication (through session and cookies)
* A simple role model
* Administration module
* User settings
* A possibility for rating articles, leaving comments and making complaints
* A notification system (SignalR)
* SQL as a default DB model provider (Entity Framework)

## Role model

Administrators can review articles if they have taken them over as assignment along with approving the latter and thus making the available for other users reading.
They are also responsible for dealing with complaints (either about comments or a whole article).
Administrators can block other users or make the administrators (and return them back to being regular ones).

## Additional Dependencies

* Angular 1.x https://angularjs.org/  
* Bootstrap 3 http://twitter.github.com/bootstrap/
* Font Awesome http://fontawesome.io/
* jQuery http://jquery.com/
* Bootstrap-wysiwyg https://github.com/drc-devs/bootstrap-wysiwyg
* Hotkeys https://github.com/tzuryby/jquery.hotkeys
* SignalR https://www.asp.net/signalr
* Select2 https://github.com/select2/select2
