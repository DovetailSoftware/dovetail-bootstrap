using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FChoice.Foundation.Clarify.Schema;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public interface IMapQueryFactory<IN, OUT>
	{
		MapQueryConfig Create(IN inputModel);
	}

	public class MapQueryFactory<IN, OUT> : IMapQueryFactory<IN, OUT>
	{
		private readonly ModelMapConfig<IN, OUT> _mapConfig;
		private int _selectIndex;
		private int _aliasIndex;
		private int _mtmAliasCount;
		private List<SelectClause> _selectedFields;
		private List<JoinClause> _joinClauses;
		private List<WhereClause> _whereClauses;

		public MapQueryFactory(ModelMapConfig<IN,OUT> mapConfig)
		{
			_mapConfig = mapConfig;
		}

		public MapQueryConfig Create(IN inputModel)
		{
			//look in field maps and set the InputValue based on the inputModel expression where needed
			_selectIndex = 0;
			_aliasIndex = 0;

			//todo DRY up these linq queries
			var rootSelectClauses = _mapConfig.Fields.Where(f => f.OutProperty != null).Select(f => BuildSelect("root", f, _selectIndex++));
			var rootWhereClauses = _mapConfig.Fields.Where(f => f.Operator != null).Select(f => BuildWhere(inputModel, "root", f));

			var rootJoinClause = new JoinClause { Alias = "root", JoinSql = "from table_{0}".ToFormat(_mapConfig.BaseTable.Name) };

			_selectedFields = new List<SelectClause>(rootSelectClauses);
			_joinClauses = new List<JoinClause> { rootJoinClause };
			_whereClauses = new List<WhereClause>(rootWhereClauses);

			_mapConfig.Joins.Each(j => BuildJoin(inputModel, j, rootJoinClause));

			//var joins = _map.Joins.SelectMany(join => @join.PreorderTraverse(j => j.Joins)).ToList();

			return new MapQueryConfig
				{
					Joins = _joinClauses,
					SelectedFields = _selectedFields,
					Wheres = _whereClauses,
				};
		}

		public void BuildJoin(IN inputModel, ModelMapConfig<IN, OUT> mapConfig, JoinClause parentJoinClause)
		{
			var toAlias = "T" + _aliasIndex++;
			var relation = (SchemaRelation)mapConfig.ViaRelation;
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
			return new SelectClause { Alias = alias, Field = field, Index = index };
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

			return new WhereClause { Alias = alias, Field = field, Value = value };
		}
	}

	public class MapQueryConfig
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
		//todo add support for outer join
	}

	public class WhereClause
	{
		public string Alias { get; set; }
		public object Value { get; set; }
		public FieldConfig Field { get; set; }
	}
}