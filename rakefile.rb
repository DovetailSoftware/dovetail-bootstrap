require 'bundler'
require "rubygems/package"

require 'fuburake'

@options = {:source => 'source', :results => 'results'}

solution = FubuRake::Solution.new do |sln|
	sln.compile = {
		:solutionfile => 'source/Bootstrap.sln'
	}

	sln.assembly_info = {
		:product_name => "Dovetail Bootstrap",
		:copyright => 'Copyright Dovetail Software 2013',
		:output_file => 'source/CommonAssemblyInfo.cs',
		:description => 'Collection of common infrastrucure used by applications based on Dovetail SDK: Session Management, Workflow Object History'
	}

	sln.options = @options
	sln.ripple_enabled = true
end

### Edit these settings
DATABASE = "mobilecl125"
DATABASE_TYPE = "mssql"
DATABASE_CONNECTION = "Data Source=localhost;Initial Catalog=mobilecl125;User Id=sa;Password=sa"

SCHEMAEDITOR_PATH = "#{Rake::Win32::normalize(ENV['PROGRAMFILES'])}/Dovetail Software/SchemaEditor/SchemaEditor.exe"

PACKAGEDIR = File.absolute_path("source/packages")
NUGETEXEDIR = File.absolute_path(".nuget")

#Oracle nuget must be pulled separately, before ripple runs, because ripple uses outdated version of nuget.core.dll
# DON'T INCLUDE THIS IN ripple.config FILE:
#    <Dependency Name="Oracle.ManagedDataAccess" Version="19.14.0" Mode="Fixed" />
Rake.application['compile'].prerequisites.unshift "getOracleNuget"

getOracleNuget = Rake::Task.define_task 'getOracleNuget' do
  puts "Unpack Oracle.ManagedDataAccess nuget package"
  FileUtils.mkdir_p(PACKAGEDIR)
  sh "#{NUGETEXEDIR}/NuGet.exe install Oracle.ManagedDataAccess -OutputDirectory #{PACKAGEDIR} -Version 19.14.0 -Verbosity normal -ConfigFile #{NUGETEXEDIR}/NuGet.Config"
end

# #desc "Copy archives to test folder in order to run unit tests"
# output :test_assemblies => [:compile] do |out|
# 	out.from "#{File.dirname(__FILE__)}"
# 	out.to @options[:results]
# 	Dir.glob("**/bin/Debug*/*.*"){ |file|
# 		out.file file, :as => "assemblies/#{File.basename(file)}"
# 	}
# end

def findNunitConsoleExe
	nunitPackageDirectory = Dir.glob('source/packages/NUnit*').first
	raise "NUnit package was not found under source/packages." if nunitPackageDirectory.nil?

	return File.join(nunitPackageDirectory, 'tools/nunit-console.exe')
end

task :nuget_deploy => [:compile, "ripple:package"] do
	DOVETAIL_FEED = ENV['DT_NUGET_FEED'] || ''
	DOVETAIL_NUGET_APIKEY = ENV['DT_NUGET_API_KEY'] || ''

	fail 'You need to define an environment variable "DT_NUGET_API_KEY" with the Dovetail Nuget feed api key as committing credentials is BAD.' if(DOVETAIL_NUGET_APIKEY.empty?)

	sh "ripple publish #{solution.options[:build_number]} #{DOVETAIL_NUGET_APIKEY} --server #{DOVETAIL_FEED}"
end

namespace :setup do

	#desc "Rebuilds development database and populates it with data"
	task :developer => [:clean,:apply_schemascripts]

	desc "Apply all schema scripts in the schema directory"
	task :apply_schemascripts do
		sh "\"#{SCHEMAEDITOR_PATH}\" -g"
		apply_schema
	end

	def apply_schema(database = DATABASE)

		puts "Applying scripts to #{database} database"
		seConfig = 'Default.SchemaEditor'
		seReport = 'SchemaDifferenceReport.txt'

		#SchemaEditor has different (more verbose) database type configuration than Dovetail SDK
		databaseType = (DATABASE_TYPE == 'mssql') ? 'MsSqlServer2005' : 'Oracle9'

		Dir.glob(File.join('packaging', 'schemascripts', "*schemascript.xml")) do |schema_script|

			File.open(seConfig) do |schema_editor_config_file|
				doc = REXML::Document.new(schema_editor_config_file)
				doc.root.elements['database/type'].text = databaseType
				doc.root.elements['database/connectionString'].text = DATABASE_CONNECTION
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
end

desc "Build and host the web application in iisexpress on port 7070"
task :start_web => [:compile] do
	puts "\n\n\n*** Launching iis express for this web application on port 7070 ***\n\n\n"

	IISEXPRESS_EXE = "#{ENV['PROGRAMFILES']}/IIS Express/iisexpress.exe".gsub('/','\\')
	sh "\"#{IISEXPRESS_EXE}\" /path:#{File.absolute_path("source/Web").gsub('/','\\')} /port:7070 /clr:v4.0"
end
