Dovetail Bootstrap
==================

This project aims to make your life easier while you write [Dovetail SDK](http://www.dovetailsoftware.com/dovetail-sdk) based applications. Included in this project are two example applications:

Features
--------

### Database operations

* Easily pull data out of your Clarify database and into view models using Model Maps.
* Simplify calling Dovetail SDK APIs and ad-hoc data access while making it easy to manage your session usage using IClarifySessionCache.

### Authentication

* Native ASP.Net authentication support that integrates with Dovetail SDK making it easy to do API operations a behalf of the current user.
* Web service API token authentication support for FubuMVC applications

Examples 
--------

### Web Applications
Our premier example is a [web application](https://github.com/DovetailSoftware/dovetail-bootstrap/tree/master/source/Web) using Dovetail SDK and [FubuMVC](http://mvc.fubu-project.org/). This web application is a good starting point for whatever information or user experiences you wish to expose for your Dovetail/Clarify applications.

![Bootstrap-Web](http://f.cl.ly/items/1g2w0M272r1W240O061V/Image%202012-09-13%20at%202.02.46%20PM.png)

This example web application can: 

* Authenticate a clarify user.
* Show the current user their open cases.
* Show history for a given case.
* Display Gbst lists and details.
* Examples for exposing Dovetail SDK API calls as web service APIs.

### Windows Service 

We also include a [simple Windows service](https://github.com/DovetailSoftware/dovetail-bootstrap/tree/master/source/Service) example using Dovetail SDK and [Topshelf](http://topshelf-project.com/) which polls a Clarify database for open case information.

![Bootstrap-Service](http://cl.ly/JQOJ/Image%202012-09-13%20at%202.08.52%20PM.png)

Setup
----

### Internet Access

Bootstrap build process downloads dependent components from the Internet, so Internet access is required. 

### Dovetail SDK 

Bootstrap requires [.Net 4.0 Full](http://www.microsoft.com/download/en/details.aspx?id=17718) is dependant on the Dovetail SDK [nuget package](http://nuget.org) bundled with the Dovetail SDK starting with [version 3.2](http://support.dovetailsoftware.com/selfservice/products/show/Dovetail%20SDK) released January 19th 2012. We recommend that you use Visual Studio 2010 SP1 with IIS Express installed.

> To download the latest version of Dovetail SDK sign onto [Dovetail's Support Center](http://support.dovetailsoftware.com/selfservice/resources) and click on [My Products](
http://support.dovetailsoftware.com/selfservice/products/owned). If you are entitled to Dovetail SDK you will see it in your list of products. If you do not have access to Dovetail SDK or wish to become a Dovetail customer please [contact us](mailto:support@dovetailsoftware.com)

Out of the box Bootstrap is looking for the Dovetail SDK nuget package in ```c:\Program Files\Dovetail Software\fcSDK```. If you installed Dovetail SDK to another directory please edit [Nuget.targets](https://github.com/DovetailSoftware/dovetail-bootstrap/blob/master/source/.nuget/NuGet.targets) with the correct path.

### Rake

We use rake for our build automation. If you do not have rake already installed. 

1. Download the [ruby installer](http://rubyinstaller.org/downloads/ "I recommend 1.9.3") for Windows.
2. Go to a command prompt.
 * ```gem install rake```

3. Add required gems.
 * ```gem install albacore```

4. Run Rake to build and run unit tests.
 * ```rake```

### Update Config Settings

#### web.config 

To take the web application for a spin you'll need to update your web.config with your database connection settings for your development Clarify database instance. 

#### Integration tests

The ```rakefile.rb``` should get updated with the correct database connection settings for your test database. 

```rb
### Edit these settings 
DATABASE = "mobilecl125"
DATABASE_TYPE = "mssql"
DATABASE_CONNECTION = "Data Source=localhost;Initial Catalog=mobilecl125;User Id=sa;Password=sa"
```

To run the integration tests:

```rake ci```

**Important:** Integration test runs will create test data. **Do not put in your production connection details**

### Optional - Schema update

The authentication token feature for bootstrap APIs require a schema change. Yes we have automation for that! If you have [Dovetail Schema Editor](http://www.dovetailsoftware.com/dovetail-schema-editor) installed all you have to do is:

```rake setup:apply_schemascripts```

Cool thing about this automation is any schemascript files present in the schema directory will get applied. Have fun.

If you want to do this with Amdoc's ddcomp tool... You should really try Schema Editor :heart: [Contact Us](mailto::support@dovetailsoftware.com)

## Take it for a spin

### Open the Solution

Double click on the 2010 Visual Studio solution: _/source/Bootstrap.sln_

### Launch!

Make sure Web is your startup project. If it is. The Web project will be bold in your Solution Explorer.

Hit Ctrl+F5 to build and launch the site.

## Nuget Automation

```nuget:update``` - Update nuget dependencies for the entire project
```nuget:build``` - Update nuget dependencies for the entire project
