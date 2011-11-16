
Dovetail Bootstrap
==================

This project aims to get you started writing web applications using Dovetail SDK and FubuMVC

Setup
-----

Currently our build automation is non-existent. For now here are a few steps to get you setup.


### Installing Nuget Dependencies 

Run the included batch file to install nuget dependencies to _/source/packages_ 

```
]> install-packages.bat
```


### Copying Dovetail SDK Assemblies

Dovetail Bootstrap does not include Dovetail SDK assemblies. Why? We currently require a licensing agreement with Dovetail Software before shipping our SDK to customers. 
 
1. Install Dovetail SDK

2. Copy the following Dovetail SDK assemblies to the _/lib/DovetailSDK_ directory.

 + FChoice.Common.dll
 + FChoice.Foundation.Clarify.Compatibility.dll
 + FChoice.Foundation.Clarify.Compatibility.Toolkits.dll
 + FChoice.Toolkits.Clarify.dll
 + fcSDK.dll
 + log4net.dll

### Open the Solution

Double click on the 2010 Visual Studio solution: _/sources/Bootstrap.sln_