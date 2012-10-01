using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Schema;
using FubuCore;
using FubuCore.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Integration.NextGen
{
	public class TestModel
	{
		public string Title { get; set; }
		public string Site { get; set; }
	}

	public class InputModel	
	{
		public string SiteName { get; set; }
	}

	[TestFixture]
	public class next_gen 
	{
		[Test]
		public void look_see()
		{
			//var schemaCache = MockRepository.GenerateStub<ISchemaCache>();
			var container = MockRepository.GenerateStub<IContainer>();

			var mapFactory = new ModelMapFactory<InputModel, TestModel>(container);

			var map = mapFactory.Create("case", c =>
				{
					c.SelectField("title", f => f.Title);

					c.Join("case_reporter2site", site => { site.SelectField("name", f => f.Site).MatchesInput(input => input.SiteName); });

					c.Join("case_tag", tag =>
						{
							tag.SelectField("name", f => f.Site).MatchesInput(input => input.SiteName);
							tag.Join("tag2user", user => { user.Field("login_name").MatchesInput(c.Get<ICurrentSDKUser>().Username); });
						});
				});

			var builder = new ModelBuilder<InputModel, TestModel>(map);

			var outputModels = builder.Execute(new InputModel {SiteName = "site name"});

			outputModels.First().Title.ShouldEqual("case title");
		}
	}

	public class ModelBuilder<IN,OUT>
	{
		private readonly ModelMap<IN, OUT> _map;

		public ModelBuilder(ModelMap<IN,OUT> map)
		{
			_map = map;
		}

		public IEnumerable<OUT> Execute(IN inputModel)
		{
			//look in field maps and set the InputValue based on the inputModel expression where needed

			//

			//TODO make the sausage

			return new OUT[0];
		}
	}

	public class ModelMapFactory<IN,OUT>
	{
		private readonly IContainer _container;
		private readonly ISchemaCache _schemaCache;

		public ModelMapFactory(IContainer container, ISchemaCache schemaCache)
		{
			_container = container;
			_schemaCache = schemaCache;
		}

		public ModelMap<IN, OUT> Create(string objectName, Action<ModelMapConfigurator<IN, OUT>> config)
		{
			if (!_schemaCache.IsValidTableOrView(objectName))
			{
				throw new DovetailMappingException(2001, "Could not create a ModelMap<{0}, {1}> for the schema object named {2}.".ToFormat(typeof(IN).Name, typeof(OUT).Name, objectName));
			}

			var configurator = new ModelMapConfigurator<IN, OUT>(_container, _schemaCache);
			config(configurator);

			var map = configurator.Map;

			var baseTable = _schemaCache.IsValidTable(objectName) ? _schemaCache.Tables[objectName] as ISchemaTableBase : _schemaCache.Views[objectName] as ISchemaTableBase;
			map.BaseTable = baseTable;

			return map;
		}
	}

	public class MatchesOperator : ModelMapOperator { }
	public class ModelMapOperator { }

	public class FieldMap<IN>
	{
		public PropertyInfo OutProperty { get; set; }
		public PropertyInfo InputProperty { get; set; }
		public object InputValue { get; set; }
		public ModelMapOperator Operator { get; set; }

		public string FieldName { get; private set; }

		public FieldMap(string fieldName)
		{
			FieldName = fieldName;
		}

		public void MatchesInput(Expression<Func<IN, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			Operator = new MatchesOperator();

			InputProperty = propertyInfo;
		}

		public void MatchesInput(object value)
		{
			Operator = new MatchesOperator();

			InputValue = value;
		}
	}

	public class ModelMap<IN, OUT>
	{
		//TODO double check this is needed
		public ModelMap<IN, OUT> Parent { get; set; }

		public ISchemaTableBase BaseTable { get; set; }
		public ISchemaRelation ViaRelation { get; set; }

		public List<FieldMap<IN>> Fields { get; private set; }
		public List<ModelMap<IN,OUT>> Joins { get; private set; }

		public ModelMap()
		{
			Fields = new List<FieldMap<IN>>();
			Joins = new List<ModelMap<IN, OUT>>();
		}
	}

	public class ModelMapConfigurator<IN, OUT> 
	{
		private readonly IContainer _container;
		private readonly ISchemaCache _schemaCache;
		public ModelMap<IN, OUT> Map { get; set; }
		
		public ModelMapConfigurator(IContainer container, ISchemaCache schemaCache)
		{
			_container = container;
			_schemaCache = schemaCache;
			Map = new ModelMap<IN, OUT>();
		}

		public T Get<T>()
		{
			return _container.GetInstance<T>();
		}

		public FieldMap<IN> Field(string fieldName)
		{
			var fieldMap = new FieldMap<IN>(fieldName);

			Map.Fields.Add(fieldMap);

			return fieldMap;
		}

		public FieldMap<IN> SelectField(string fieldName, Expression<Func<OUT, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			var fieldMap = new FieldMap<IN>(fieldName) {OutProperty = propertyInfo};

			Map.Fields.Add(fieldMap);

			return fieldMap;
		}

		public void Join(string relationName, Action<ModelMapConfigurator<IN, OUT>> config)
		{
			if(!_schemaCache.IsValidRelation(Map.BaseTable.Name, relationName))
			{
				throw new DovetailMappingException(2002, "Could not Join the relation {0} for the parent ModelMap<{1}, {2}> based on {3}.".ToFormat(relationName, typeof(IN).Name, typeof(OUT).Name, Map.BaseTable.Name));
			}

			var joinConfigurator = new ModelMapConfigurator<IN, OUT>(_container, _schemaCache);

			config(joinConfigurator);

			var joinMap = joinConfigurator.Map;

			joinMap.Parent = Map;
			joinMap.ViaRelation = _schemaCache.GetRelation(Map.BaseTable.Name, relationName);
			joinMap.BaseTable = joinMap.ViaRelation.SourceTable;

			Map.Joins.Add(joinMap);
		}
	}
}