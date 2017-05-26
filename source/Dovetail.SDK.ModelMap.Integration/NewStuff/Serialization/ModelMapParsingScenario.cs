using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.ModelMap.NewStuff;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using Dovetail.SDK.ModelMap.NewStuff.Serialization;
using Dovetail.SDK.ModelMap.NewStuff.Serialization.Overrides;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
	public class ModelMapParsingScenario
	{
		private readonly string _tempPath;
		private readonly string[] _files;
		private readonly Lazy<ModelMap.NewStuff.ModelMap> _filter;

		public ModelMapParsingScenario(string tempPath, string[] files)
		{
			_tempPath = tempPath;
			_files = files;

			_filter = new Lazy<ModelMap.NewStuff.ModelMap>(parseMap);
		}

		public ModelMap.NewStuff.ModelMap Map { get { return _filter.Value; } }

		public IModelMapInstruction[] Instructions {  get { return Map.Instructions.ToArray(); } }

		public TInstruction Get<TInstruction>(int index)
		{
			return Instructions[index].As<TInstruction>();
		}

		public void Verify<TInstruction>(int index, Action<TInstruction> action)
		{
			var instruction = Get<TInstruction>(index);
			action(instruction);
		}

		public void CleanUp()
		{
			_files.Each(_ => File.Delete(_));
			Directory.Delete(_tempPath);
		}

		private ModelMap.NewStuff.ModelMap parseMap()
		{
			var logger = new NulloLogger();
			var services = new InMemoryServiceLocator();

			services.Add<ILogger>(logger);
			services.Add<IObjectBuilder>(new ObjectBuilder());
			services.Add<IFilterPolicyRegistry>(new FilterPolicyRegistry());
			services.Add<IXElementSerializer>(new XElementSerializer(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services)));
			services.Add<IMappingVariableExpander>(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services));

			var visitors = Visitors();
			var elementService = new XElementService(visitors);
			var parser = new ModelMapParser(services, elementService, logger);
			var overrides = new ModelMapOverrideParser(parser, new ModelMapDiff());

			var settings = new ModelMapSettings { Directory = _tempPath };
			var cache = new ModelMapCache(parser, overrides, settings);

			return cache.Maps().Single();
		}

		public static IElementVisitor[] Visitors()
		{
			return TypeScanner
				.ConcreteImplementationsOf<IElementVisitor>()
				.Select(_ => Activator.CreateInstance(_).As<IElementVisitor>())
				.ToArray();
		}

		public static ModelMapParsingScenario Create(Action<ParsingExpression> config)
		{
			var expression = new ParsingExpression();
			config(expression);

			return expression.As<IScenarioBuilder>().Create();
		}

		private interface IScenarioBuilder
		{
			ModelMapParsingScenario Create();
		}

		public class ParsingExpression : IScenarioBuilder
		{
			private List<string> _files;
			private readonly string _tempPath;

			public ParsingExpression()
			{
				_files = new List<string>();
				_tempPath = Path.GetTempPath().AppendPath(Guid.NewGuid().ToString());
				Directory.CreateDirectory(_tempPath);
			}

			public void UseFile(string filePath)
			{
				var type = GetType();
				using (var stream = type.Assembly.GetManifestResourceStream("{0}.{1}".ToFormat(type.Namespace, filePath)))
				{
					var file = _tempPath.AppendPath(filePath);
					_files.Add(file);

					using (var writer = new StreamWriter(file))
						writer.Write(stream.ReadAllText());
				}
			}

			ModelMapParsingScenario IScenarioBuilder.Create()
			{
				return new ModelMapParsingScenario(_tempPath, _files.ToArray());
			}
		}
	}
}