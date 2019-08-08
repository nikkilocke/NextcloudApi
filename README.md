# NextcloudApi

This is a C# wrapper to (the most important bits of) the [Nextcloud API](https://docs.nextcloud.com/server/16/developer_manual/client_apis/index.html).

It is provided with a Visual Studio 2019 build solution for .NET Standard, so can be used with any version of .NET.

There is a test project (for net core only) which also demonstrates usage.

## Setup before using the API

In order to use the Nextcloud API you need to supply an Api Username and Password. Some calls can be made using OAuth2, instead of providing a Username and Password in Settings, but any calls that insist on a recent password login (like most of the admin ones) will not work with OAuth2.

This information has to be provided in an object that implements the [ISettings](../master/NextcloudApi/Settings.cs) interface, which is then used to create a NextcloudApi instance. A Settings class which implements this interface is provided, to save you work. This provides a static Load method, reads the settings from *LocalApplicationData*/NextcloudApi/Settings.json. On a Windows 10 machine, *LocalApplicationData* is `C:\Users\<USER>\AppData\Local`, on Linux it is `~user/.local/share`.

## Testing

In order to run the Unit Tests provided, you must provide additional data in your ISettings object - see the Settings object in [UnitTest1.cs](../master/Tests/UnitTest1.cs).

## Hooks for more complex uses

You do not have to use the provided Settings class, provided you have a class that implements ISettings.

## License

This wrapper is licensed under creative commons share-alike, see [license.txt](../master/license.txt).

## Using the Api

The Unit Tests should give you sufficient examples on using the Api.

An Api instance is created by passing it an object that implements ISettings (a default class is provided which will read the settings from a json file). The Api instance is IDisposable, so should be Disposed when no longer needed (this is because it contains an HttpClient).

C# classes are provided for the objects you can send to or receive from the Nextcloud api. For instance the Group object represents groups. These main objects have methods which call the Nextcloud api - such as Group.Create to create a new group, Group.Get to get group details, etc.

Some Api calls return a list of items (such as Group.List). These are returned as a subclass of ApiList<Group>. The Nextcloud api itself usually only returns the first few items in the list, and needs to be called again to return the next chunk of items. This is all done for you by ApiList - it has a method called All(Api) which will return an IEnumerable of the appropriate listed object. Enumerating the enumerable will return all the items in the first chunk, then call the Nextcloud api to get the next chunk, return them and so on. It hides all that work from the caller, while remaining as efficient as possible by only getting data when needed - for instance, using Linq calls like Any or First will stop getting data when the first item that matches the selection function is found.


