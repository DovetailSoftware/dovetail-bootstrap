require 'albacore'
include REXML
include Rake::DSL

BUILD_NUMBER_BASE = "1.0.0"
PROJECT_NAME = "Bootstrap"
SLN_PATH = "source/#{PROJECT_NAME}.sln"
SLN_FILES = [SLN_PATH]
DATABASE = "mobilecl125"
COMPILE_TARGET = "Debug"

DOVETAILSDK_PATH = "#{Rake::Win32::normalize(ENV['ProgramW6432'])}/Dovetail Software/fcSDK/bin"
SCHEMAEDITOR_PATH = "#{Rake::Win32::normalize(ENV['PROGRAMFILES'])}/Dovetail Software/SchemaEditor/SchemaEditor.exe"
NUNIT_PATH = "tools/NUnit/nunit-console.exe"

props = {:archive => "build", :testing => "results", :database => ""}

desc "**Default**, compiles and runs unit tests"
task :default => [:clean, :compile,:test_assemblies,:unit_tests]

#task :ci => [:on_exit,:clean,:apply_schemascripts,:compile,:test_assemblies,:unit_tests]

desc "Rebuilds development database and populates it will data"
task :setup_developer => [:clean,:apply_schemascripts, :install_packages]

desc "Run a sample build using the MSBuildTask"
msbuild :msbuild, [:clean] do |msb,args|
	msb.properties :configuration => :Debug
	msb.targets :Clean, :Build
	msb.verbosity = "minimal"
	msb.solution = args[:solution] || SLN_PATH
end

task :compile => [:restore_if_missing] do 
	SLN_FILES.each do |f|
		Rake::Task["msbuild"].execute(:solution => f)
	end
end

desc "Copy archives to test folder in order to run unit tests"
output :test_assemblies => [:compile] do |out|
	out.from "#{File.dirname(__FILE__)}"
	out.to "#{props[:testing]}"
	Dir.glob("**/bin/Debug*/*.*"){ |file|
		out.file file, :as => "assemblies/#{File.basename(file)}"
	}	
end

desc "Run NUnit Test for any dlls that contain the work test"
nunit :unit_tests do |nunit|
	nunit.command = NUNIT_PATH
	nunit.assemblies = Dir.glob("results/assemblies/*{T,t}ests.dll").uniq
	nunit.options '/xml=results/nunit-results.xml'
end

desc "Run nuget install on all the projects"
task :install_packages => [:clean] do 
	Dir.glob(File.join("**","packages.config")){ |file|
		puts "Installing packages for #{file}"
		sh "tools/nuget.exe install #{file} -OutputDirectory source/packages"
	}
end

desc "Run nuget udpate on all the projects"
task :update_packages => [:clean] do 
	Dir.glob(File.join("**","packages.config")){ |file|
		puts "Updating packages for #{file}"
		sh "tools/nuget.exe update #{file} -RepositoryPath source/packages"
	}
end

desc "Copy Doveatail SDK assemblies to this project's tool directory"
task :copy_sdk_assemblies do 
	projectSDK = 'libs/DovetailSDK'
	sdkAssemblies = ['FChoice.Common.dll', 'FChoice.Foundation.Clarify.Compatibility.dll','FChoice.Foundation.Clarify.Compatibility.Toolkits.dll' , 'FChoice.Toolkits.Clarify.dll', 'fcSDK.dll', 'log4net.dll']
	FileUtils.mkdir_p(projectSDK)
	sdkAssemblies.each do |asm|
		FileUtils.cp File.join(DOVETAILSDK_PATH, asm), projectSDK
	end	
end

desc "Update the version information for the build"
assemblyinfo :version do |asm|
	asm_version = BUILD_NUMBER_BASE + ".0"

	begin
		gittag = `git describe --long`.chomp 	# looks something like v0.1.0-63-g92228f4
		gitnumberpart = /-(\d+)-/.match(gittag)
		gitnumber = gitnumberpart.nil? ? '0' : gitnumberpart[1]
		commit = `git log -1 --pretty=format:%H`
	rescue
		commit = "git unavailable"
		gitnumber = "0"
	end
	build_number = "#{BUILD_NUMBER_BASE}.#{gitnumber}"
	tc_build_number = ENV["BUILD_NUMBER"]
	puts "##teamcity[buildNumber '#{build_number}-#{tc_build_number}']" unless tc_build_number.nil?
	asm.trademark = commit
	asm.product_name = "#{PRODUCT} #{gittag}"
	asm.description = build_number
	asm.version = asm_version
	asm.file_version = build_number
	asm.custom_attributes :AssemblyInformationalVersion => asm_version
	asm.copyright = COPYRIGHT
	asm.output_file = COMMON_ASSEMBLY_INFO
end

desc "Prepares the working directory for a new build"
task :clean do	

	props.each do |key,val|
		FileUtils.rm_r(Dir.glob("#{val}/*"), :force => true) if File.exists?val
		FileUtils.rmdir(val) if File.exists?val  
	end
	# Clean up all bin folders in the source folder
	FileUtils.rm_rf(Dir.glob("**/{obj,bin}"))
	
	fubucontent = 'source/Web/fubu-content/'
	Dir.new(fubucontent).each do |file|
		if !file.end_with?('zip') and file != '.' and file != '..'
			path = File.join(fubucontent,file)
			puts "removing exploded bottle " + path
			FileUtils.rm_rf(path)
		end
	end 
end

desc "Apply all schema scripts in the schema directory"
task :apply_schemascripts do
	sh "\"#{SCHEMAEDITOR_PATH}\" -g"
	apply_schema
end

def apply_schema(database = DATABASE)  

	puts "Applying scripts to #{database} database"
	seConfig = 'Default.SchemaEditor'           
	seReport = 'SchemaDifferenceReport.txt'

	Dir.glob(File.join('schema', "*schemascript.xml")) do |schema_script|  
 
		File.open(seConfig) do |schema_editor_config_file|
			doc = Document.new(schema_editor_config_file)
			doc.root.elements['database/type'].text = 'MsSqlServer2005'
			doc.root.elements['database/connectionString'].text = "Data Source=.; Initial Catalog=#{database};User Id=sa; Password=sa;"
			doc.root.elements['inputFilePath'].text = schema_script.gsub('/','\\')
			formatter = REXML::Formatters::Default.new
			File.open(seConfig, 'w') do |result|
				formatter.write(doc, result)
			end
		end

		puts "\n\nApplying schemascript #{schema_script}"
		sh "\"#{SCHEMAEDITOR_PATH}\" -a"
	end
	sh "type #{seReport}"
	File.delete(seConfig)
	File.delete seReport if File.exists? seReport
end