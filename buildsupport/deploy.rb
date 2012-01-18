NUGET_FEED = "\\\\marvin.fcs.local\\product\\nuget_feed"

#desc "Deploy nuget packages to local feed (share)"
task :deploy_nuget_packages do 
	Dir.glob(File.join("results//packages","*.nupkg")){ |file|
		puts "Deploying #{file} to #{NUGET_FEED}"
		FileUtils.cp file, NUGET_FEED
	}
end

task :dovetail_nuget_source do 
	puts "Adding new nuget source: #{NUGET_FEED}"
	sh "#{NUGET_EXE} sources Add -Name Dovetail -Source #{NUGET_FEED}" do |ok, res|
		puts "Looks like it was already added." if !ok
	end 	
end 