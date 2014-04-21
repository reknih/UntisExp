The ultimate Untis fetcher lib
=================================

Your education facility uses Untis for creating schedules and enabled webaccess? Then this is your library. Use it to fetch the events for specific groups from the web interface.

How to use it
--------------------------------

Link it in your project. Then, modify the `url` and `pathToNavbar` properties of VConfig according to the web interface of your school. Also, check if you want to keep the different presets
of the other properties.

Then instanciate a `Fetcher` object with the methodnames for either clearing the view, alerts, adding a list of events to the view and adding one event, alerts and adding of new groups to the view or all of them.

Just look into IntelliSense. It tells you which exact types of functions are required.

The  `data` object you get from most operations has two properties which are important to you `Line1` and `Line2`. The first line gives you a raw idea which lesson is affected while the latter provides exact information.

Also make sure that you call `refresh` on each `Data` instance you modify manually.

How to get it
-------------------------------
You can clone this repo or get it from NuGet.
