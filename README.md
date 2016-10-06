# iMessage Bridge

**Current version: 1.0-a1 (This is only an alpha version so changes in new versions will usually be bug fixes or fixed mistakes in the source code)**

An iMessage server that allows many clients including Windows to communicate using iMessage and SMS. This project is only in the alpha stage so not all features will work properly.

The iMessage bridge comes bundled with:
- The server that is used to send out incoming messages to the clients.
- An API DLL to be used with .NET applications although other developers can also port the API to be used with Java and many other platforms.
- A simple Windows client to test the bridge. Third-party developers can design new clients for the bridge.

To download and run the server, find a Mac with iMessage set up, install the [Mono framework](http://www.mono-project.com/download/), [download the project](https://github.com/3dflash/iMessageBridge/archive/master.zip), unzip it, navigate to the iMessageBridge folder, and then simply run "iMessage Bridge.app".

Please note that the source code was developed with Visual Studio so they might not open correctly with Xamarin.