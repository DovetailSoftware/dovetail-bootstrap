﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.History.Serialization;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.Serialization;
using Dovetail.SDK.ModelMap.Serialization.Overrides;
using FubuCore;

namespace Dovetail.SDK.History.Tests.Serialization
{
	public class HistoryMapParsingScenario
	{
		private readonly string _tempPath;
		private readonly string[] _files;
		private readonly Lazy<ModelMap.ModelMap> _filter;
		private readonly InMemoryServiceLocator _services;

		public HistoryMapParsingScenario(string tempPath, string[] files, InMemoryServiceLocator services)
		{
			_tempPath = tempPath;
			_files = files;
			_services = services;

			_filter = new Lazy<ModelMap.ModelMap>(parseMap);
		}

		public ModelMap.ModelMap Map { get { return _filter.Value; } }
		public IHistoryMapCache Cache { get; private set; }
		public InMemoryServiceLocator Services { get { return _services; } }

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

		public void WhatDoIHave()
		{
			Instructions.Each(_ => Debug.WriteLine(_.GetType().Name));
		}

		public void CleanUp()
		{
			_files.Each(File.Delete);
			Directory.Delete(_tempPath);
		}

		private ModelMap.ModelMap parseMap()
		{
			var logger = new NulloLogger();

			Services.Add<ILogger>(logger);
			Services.Add<IObjectBuilder>(new ObjectBuilder());
			Services.Add<IFilterPolicyRegistry>(new FilterPolicyRegistry());
			Services.Add<IXElementSerializer>(new XElementSerializer(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), _services)));
			Services.Add<IMappingVariableExpander>(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), _services));

			var visitors = Visitors();
			var elementService = new XElementService(visitors);

			var parser = new HistoryMapParser(Services, elementService, logger);
			var overrides = new HistoryMapOverrideParser(parser, new ModelMapDiff(), new HistoryMapDiffOptions());
			var settings = Services.GetInstance<HistorySettings>();
			Cache = new HistoryMapCache(parser, settings, overrides);

			return Cache.Maps().First();
		}

		public static IElementVisitor[] Visitors()
		{
			return TypeScanner
				.ConcreteImplementationsOf<IElementVisitor>()
				.Select(_ => Activator.CreateInstance(_).As<IElementVisitor>())
				.ToArray();
		}

		public static HistoryMapParsingScenario Create(Action<ParsingExpression> config)
		{
			var expression = new ParsingExpression();
			config(expression);

			return expression.As<IScenarioBuilder>().Create();
		}

		private interface IScenarioBuilder
		{
			HistoryMapParsingScenario Create();
		}

		public class ParsingExpression : IScenarioBuilder
		{
			private List<string> _files;
			private readonly string _tempPath;
			private readonly InMemoryServiceLocator _services;

			public ParsingExpression()
			{
				_files = new List<string>();
				_tempPath = Path.GetTempPath().AppendPath(Guid.NewGuid().ToString());
				Directory.CreateDirectory(_tempPath);

				_services = new InMemoryServiceLocator();
				_services.Add(new HistorySettings { Directory = _tempPath });
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

			public InMemoryServiceLocator Services { get {  return _services; } }

			public void UseService<T>(T service)
			{
				_services.Add(service);
			}

			HistoryMapParsingScenario IScenarioBuilder.Create()
			{
				return new HistoryMapParsingScenario(_tempPath, _files.ToArray(), _services);
			}
		}
	}
}
