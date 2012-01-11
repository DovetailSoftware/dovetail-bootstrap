Dovetail Bootstrap
==================

This project aims to get you started writing web applications using Dovetail SDK and FubuMVC

Setup
-----

### Rake

We use rake for our build automation. If you do not have rake already installed. 

1. Download the [ruby installer](http://rubyinstaller.org/downloads/ "I recommend 1.9.3") for Windows.
2. Go to a command prompt.
 * ```gem install rake```

3. Add required gems.
 * ```gem install albacore```

4. Run Rake.
 * ```rake```


### Copying Dovetail SDK Assemblies

Dovetail Bootstrap does not include Dovetail SDK assemblies. Why? We currently require a licensing agreement with Dovetail Software before shipping our SDK to customers. 
 
1. Install Dovetail SDK if you don't have it already. We recommend a recent version (3.0 or greater).

2. Copy the required sdk assemblies using our handy rake target 
 * ```rake setup:copy_sdk_assemblies```

### Update web.config 

You'll need to update your web.config with your database connection settings for your development Clarify database instance. We may automate this step down the road driving it from the rakefile.rb.

Take it for a spin
------------------

### Open the Solution

Double click on the 2010 Visual Studio solution: _/source/Bootstrap.sln_

### Launch!

Make sure Web is your startup project. If it is. The Web project will be bold in your Solution Explorer.

Hit Ctrl+F5 to build and launch the site.
