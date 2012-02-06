
DOVETAIL_FEED = "http://focus.dovetailsoftware.com/nuget/nuget"
NUGET_FEEDS.push(DOVETAIL_FEED + "/nuget")

#desc "Deploy nuget packages to local feed (share)"
task :deploy_nuget_packages do 
	packagesDir = File.absolute_path("results/packages")
	Dir.glob(File.join(packagesDir,"*.nupkg")){ |file|
		puts "Deploying #{File.basename(file)} to #{DOVETAIL_FEED}"
		sh "#{NUGET_EXE} push #{file.gsub(/\//,"\\\\")} -s #{DOVETAIL_FEED}"
	}
end