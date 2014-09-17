Manifesto
=========

Manifest is a C# 4.0 class library that allows developers to create generic file manifest programmatically.

Nuget package is available here:
https://www.nuget.org/packages/Manifesto/


Why do I need this?
==

Well, *you* may not, but I did.  However, if you're legitimately confused, I've included a list of some generic scenarios below where this library could prove useful:

* Your application relies on data that occasionally gets updated, and you need an easy way of communicating these changes to the client without requiring them to download the entire set of data again.
* You want to monitor a directory of files for changes made when your application wasn't running.
