pingactivedirectoryservers
==========================

This a C# tool to check that advertised active directory servers can authenticate users.
Used to check for dodgy domain controllers...

Running:
Open in Visual Studio.
Build.
Run console app, it'll prompt for a username and password and then use those credentials to authenticate the user on all advertised domain controllers on the network.

The app outputs CSV like syntax for easy parsing in Excel.

