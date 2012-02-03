NUGET_FEED = "http://focus.dovetailsoftware.com/nuget/"

#desc "Deploy nuget packages to local feed (share)"
task :deploy_nuget_packages do 
	packagesDir = File.absolute_path("results/packages")
	Dir.glob(File.join(packagesDir,"*.nupkg")){ |file|
		puts "Deploying #{File.basename(file)} to #{NUGET_FEED}"
		sh "#{NUGET_EXE} push #{file.gsub(/\//,"\\\\")} -s #{NUGET_FEED}"
	}
end

task :dovetail_nuget_source do 
	puts "Adding new nuget source: #{NUGET_FEED}"
	sh "#{NUGET_EXE} sources Add -Name Dovetail -Source #{NUGET_FEED}" do |ok, res|
		puts "Looks like it was already added." if !ok
	end 	
end 