using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FChoice.Foundation.Clarify.Schema;
using FChoice.Foundation.Schema;
using FubuCore;
using FubuCore.Reflection;
using StructureMap;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public static class LinqExtension
	{
		public static IEnumerable<T> PreorderTraverse<T>(this T node, Func<T, IEnumerable<T>> childrenFor)
		{
			yield return node;

			var childNodes = childrenFor(node);

			if (childNodes == null) yield break;

			foreach (var childNode in childNodes.SelectMany(n => PreorderTraverse(n, childrenFor)))
			{
				yield return childNode;
			}
		}
	}

	public class ModelBuilder<IN, OUT>
	{
		private readonly ModelMapConfig<IN, OUT> _mapConfig;

		public ModelBuilder(ModelMapConfig<IN, OUT> mapConfig)
		{
			_mapConfig = mapConfig;
		}

		public IEnumerable<OUT> Execute(IN inputModel)
		{
			var query = BuildQuery(inputModel);


			/*		var joinFields = joins.SelectMany(r => r.Fields);
					var allFields = _map.Fields.Concat(joinFields);

					allFields.Where(f => f.OutProperty != null).Each(field=>
						{
							field.InputValue = field.OutProperty.GetValue(inputModel, null);
						});		*/

			//root map
			// add fields to select clause
			// add base from clause

			//visit each join, construct the join clause
			//visit each join's field 
			//when inputproperty is present and operator is present put input models matching property into the input value
			//add the selected ones select clause
			//add the constraints to the where clause for fields with operators and input values.
			//recurse into child joins


			//TODO make the sausage

			return new OUT[0];
		}

		//todo refactor this to a class
		private int _selectIndex;
		private int _aliasIndex;
		private int _mtmAliasCount;
		private List<SelectClause> _selectedFields;
		private List<JoinClause> _joinClauses;
		private List<WhereClause> _whereClauses;

		public Query BuildQuery(IN inputModel)
		{
			//look in field maps and set the InputValue based on the inputModel expression where needed
			_selectIndex = 0;
			_aliasIndex = 0;

			//todo DRY up these linq queries
			var rootSelectClauses = _mapConfig.Fields.Where(f => f.OutProperty != null).Select(f => BuildSelect("root", f, _selectIndex++));
			var rootWhereClauses = _mapConfig.Fields.Where(f => f.Operator != null).Select(f => BuildWhere(inputModel, "root", f));

			var rootJoinClause = new JoinClause {Alias = "root", JoinSql = "from table_{0}".ToFormat(_mapConfig.BaseTable.Name)};

			_selectedFields = new List<SelectClause>(rootSelectClauses);
			_joinClauses = new List<JoinClause> {rootJoinClause};
			_whereClauses = new List<WhereClause>(rootWhereClauses);

			_mapConfig.Joins.Each(j => BuildJoin(inputModel, j, rootJoinClause));

			//var joins = _map.Joins.SelectMany(join => @join.PreorderTraverse(j => j.Joins)).ToList();

			return new Query
				{
					Joins = _joinClauses, 
					SelectedFields = _selectedFields, 
					Sql = "sql"
				};
		}

		public void BuildJoin(IN inputModel, ModelMapConfig<IN, OUT> mapConfig, JoinClause parentJoinClause)
		{
			var toAlias = "T" + _aliasIndex++;
			var relation = (SchemaRelation) mapConfig.ViaRelation;
			const string rowIdColumnName = "objid";

			var selects = mapConfig.Fields.Where(f => f.OutProperty != null).Select(field => BuildSelect(toAlias, field, _selectIndex++));
			_selectedFields.AddRange(selects);

			var wheres = mapConfig.Fields.Where(f => f.Operator != null).Select(field => BuildWhere(inputModel, toAlias, field));
			_whereClauses.AddRange(wheres);

			// this is where it gets hairy

			var joinStart = "INNER JOIN table_{0} {1} ON ".ToFormat(relation.TargetName, toAlias);

			var joinBuilder = new StringBuilder();
			string fromField = rowIdColumnName;
			string toField = rowIdColumnName;

			if (relation.Cardinality == SchemaRelationCardinality.ManyToOne || relation.Cardinality == SchemaRelationCardinality.OneToOnePrimary)
			{
				joinBuilder.AppendFormat(joinStart);
	
				if (relation.IsExclusive)
				{
					fromField = relation.RelationPhysicalName;
					joinBuilder.AppendFormat("{0}.{1} = {2} AND ", parentJoinClause.Alias, relation.FocusFieldName, mapConfig.ViaRelation.TargetTable.ObjectID);
				}
				else
				{
					fromField = mapConfig.ViaRelation.Name;
				}

				joinBuilder.AppendFormat("{0}.{1} = {2}.{3}", parentJoinClause.Alias, fromField, toAlias, toField);
			}
			
			else if (relation.Cardinality == SchemaRelationCardinality.OneToMany || relation.Cardinality == SchemaRelationCardinality.OneToOneForeign)
			{
				joinBuilder.AppendFormat(joinStart);

				if (relation.IsExclusive)
				{
					toField = relation.RelationPhysicalName;
					joinBuilder.AppendFormat("{0}.{1} = {2} AND ", toAlias, relation.FocusFieldName, relation.SourceTable.ObjectID);
				}
				else
				{
					toField = relation.InverseRelationName;
				}
				joinBuilder.AppendFormat("{0}.{1} = {2}.{3}", parentJoinClause.Alias, fromField, toAlias, toField);
			}

			else if (relation.Cardinality == SchemaRelationCardinality.ManyToMany)
			{
				var mtmTableAlias = String.Format("M{0}", _mtmAliasCount++);

				joinBuilder.AppendFormat("INNER JOIN {0} {1} ON {2}.{3} = {4}.{5}", relation.MtmTableName, mtmTableAlias, parentJoinClause.Alias, fromField, mtmTableAlias, relation.Name);
				joinBuilder.AppendFormat("{0} ON {1}.{2} = {3}.{4}", joinStart, mtmTableAlias, relation.InverseRelationName, toAlias, toField);
			}

			var joinClause = new JoinClause
				{
					Alias = toAlias,
					JoinSql = joinBuilder.ToString()
				};

			_joinClauses.Add(joinClause);

			mapConfig.Joins.Each(j => BuildJoin(inputModel, j, joinClause));
		}

		public SelectClause BuildSelect(string alias, FieldConfig field, int index)
		{
			//if (field.Operator != null && field.InputProperty != null)
			//{
			//    field.InputValue = field.OutProperty.GetValue(inputModel, null);
			//}

			return new SelectClause {Alias = alias, Field = field, Index = index};
		}

		public WhereClause BuildWhere(IN inputModel, string alias, FieldConfig field)
		{
			var value = field.InputValue;

			if (value == null && field.InputProperty != null)
			{
				field.InputValue = field.OutProperty.GetValue(inputModel, null);
			}

			if (value == null)
			{
				throw new ApplicationException("Field {0} does not have a value.".ToFormat(field.FieldName));
			}

			return new WhereClause { Alias = alias, Field = field, Value = value};
		}


		public class Query
		{
			public string Sql { get; set; }
			public IEnumerable<SelectClause> SelectedFields { get; set; }
			public IEnumerable<JoinClause> Joins { get; set; }
			public IEnumerable<WhereClause> Wheres { get; set; }
		}

		public class SelectClause
		{
			public int Index { get; set; }
			public string Alias { get; set; }
			public string FieldName { get; set; }
			public FieldConfig Field { get; set; }
		}

		public class JoinClause
		{
			public string Alias { get; set; }
			public string JoinSql { get; set; }
		}

		public class WhereClause
		{
			public string Alias { get; set; }
			public object Value { get; set; }
			public FieldConfig Field { get; set; }
		}
	}

	public class ModelMapFactory<IN, OUT>
	{
		private readonly IContainer _container;
		private readonly ISchemaCache _schemaCache;

		public ModelMapFactory(IContainer container, ISchemaCache schemaCache)
		{
			_container = container;
			_schemaCache = schemaCache;
		}

		//TODO see if the resulting modelmap can be cached for reuse.

		public ModelMapConfig<IN, OUT> Create(string objectName, Action<ModelMapConfigurator<IN, OUT>> config)
		{
			if (!_schemaCache.IsValidTableOrView(objectName))
			{
				throw new DovetailMappingException(2001, "Could not create a ModelMap<{0}, {1}> for the schema object named {2}.".ToFormat(typeof (IN).Name, typeof (OUT).Name, objectName));
			}

			var configurator = new ModelMapConfigurator<IN, OUT>(_container, _schemaCache);
			config(configurator);

			var map = configurator.MapConfig;

			var baseTable = _schemaCache.IsValidTable(objectName) ? _schemaCache.Tables[objectName] as ISchemaTableBase : _schemaCache.Views[objectName] as ISchemaTableBase;
			map.BaseTable = baseTable;

			return map;
		}
	}

	public class Matches : FieldConfigOperator
	{
	}

	public class FieldConfigOperator
	{
	}

	public class FieldConfig
	{
		public string FieldName { get; set; }
		public PropertyInfo OutProperty { get; set; }
		public PropertyInfo InputProperty { get; set; }
		public object InputValue { get; set; }
		public FieldConfigOperator Operator { get; set; }
	}

	public class FieldConfig<IN> : FieldConfig
	{
		public FieldConfig(string fieldName)
		{
			FieldName = fieldName;
		}

		public void EqualTo(Expression<Func<IN, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			Operator = new Matches();

			InputProperty = propertyInfo;
		}

		public void EqualTo(object value)
		{
			Operator = new Matches();

			InputValue = value;
		}
	}

	public class ModelMapConfig<IN, OUT>
	{
		public ModelMapConfig<IN, OUT> Parent { get; set; }
		public ISchemaTableBase BaseTable { get; set; }
		public ISchemaRelation ViaRelation { get; set; }

		public List<FieldConfig<IN>> Fields { get; private set; }
		public List<ModelMapConfig<IN, OUT>> Joins { get; private set; }

		public ModelMapConfig()
		{
			Fields = new List<FieldConfig<IN>>();
			Joins = new List<ModelMapConfig<IN, OUT>>();
		}
	}

	public class ModelMapConfigurator<IN, OUT>
	{
		private readonly IContainer _container;
		private readonly ISchemaCache _schemaCache;
		public ModelMapConfig<IN, OUT> MapConfig { get; set; }

		public ModelMapConfigurator(IContainer container, ISchemaCache schemaCache)
		{
			_container = container;
			_schemaCache = schemaCache;
			MapConfig = new ModelMapConfig<IN, OUT>();
		}

		public T Get<T>()
		{
			return _container.GetInstance<T>();
		}

		public FieldConfig<IN> Field(string fieldName)
		{
			var fieldMap = new FieldConfig<IN>(fieldName);

			MapConfig.Fields.Add(fieldMap);

			return fieldMap;
		}

		public FieldConfig<IN> SelectField(string fieldName, Expression<Func<OUT, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			var fieldMap = new FieldConfig<IN>(fieldName) {OutProperty = propertyInfo};

			MapConfig.Fields.Add(fieldMap);

			return fieldMap;
		}

		public void Join(string relationName, Action<ModelMapConfigurator<IN, OUT>> config)
		{
			if (!_schemaCache.IsValidRelation(MapConfig.BaseTable.Name, relationName))
			{
				throw new DovetailMappingException(2002, "Could not Join the relation {0} for the parent ModelMap<{1}, {2}> based on {3}.".ToFormat(relationName, typeof (IN).Name, typeof (OUT).Name, MapConfig.BaseTable.Name));
			}

			var joinConfigurator = new ModelMapConfigurator<IN, OUT>(_container, _schemaCache);

			config(joinConfigurator);

			var joinMap = joinConfigurator.MapConfig;

			joinMap.Parent = MapConfig;
			joinMap.ViaRelation = _schemaCache.GetRelation(MapConfig.BaseTable.Name, relationName);
			joinMap.BaseTable = joinMap.ViaRelation.SourceTable;

			MapConfig.Joins.Add(joinMap);
		}
	}
}