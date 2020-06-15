# PoE-Warehouse
It's a heavily featured trade and stash manager application for Path of Exile. Greatly inspired by the [Acquisistion](https://github.com/xyzz/acquisition) project, but with a much more intuitive interface and many additional features.

## Requirements
- .NET Framework 4.5
- Visual C++ 2012

# Features
- Virtual Stash
- Multiple login methods : with the Session ID (POESESSID) (Like Acquisition and Procurement), in the browser on the Path of Exile website or with Steam (Steam method not implemented yet).
- Auto-Online 
- Automatically update forum thread while needed
- Automatically fetch stash when needed
- Automatically create new thread per league when needed
- Integrate poe.ninja Currency and Builds with automatic league selection
- Integrate www.pathofexile.com/trade with automatic league selection and auto-login
- Show/Hide items on trading websites on demand

## TODO
- Price expiration date and notification when it expires
- Price checking and notifications if price might be wrong
- Price a whole tab (can currently price items one-by-one)
- Multi-user support
- Export items to PoB
- Export items to imgur.com
- Implement the Steam login method

# For developpers
The application is built in C# using WPF (.Net 4.5).
It's using the Mahapps Metro library for the Metro UI.
It's using CefSharp Chromimum based web browser component for the PoE Trade, PoE Ninja and web browser login method. (Require Visual C++ 2012)
