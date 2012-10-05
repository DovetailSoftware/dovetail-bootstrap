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

	public class MapQueryConfigFactory<IN, OUT> : IMapQueryFactory<IN, OUT>
	{
		private readonly ModelMapConfig<IN, OUT> _mapConfig;
		private int _selectIndex;
		private int _aliasIndex;
		private int _mtmAliasCount;
		private List<SelectItem> _selectedFields;
		private List<JoinItem> _joinClauses;
		private List<WhereItem> _whereClauses;

		public MapQueryConfigFactory(ModelMapConfig<IN,OUT> mapConfig)
		{
			_mapConfig = mapConfig;
		}

		public MapQueryConfig Create(IN inputModel)
		{
			//look in field maps and set the InputValue based on the inputModel expression where needed
			_selectIndex = 0;
			_aliasIndex = 0;

			//todo DRY up these linq queries
			var rootSelectClauses = _mapConfig.Fields.Where(f => f.OutProperty != null).Select(f => BuildSelectItem("root", f, _selectIndex++));
			var rootWhereClauses = _mapConfig.Fields.Where(f => f.Operator != null).Select(f => BuildWhereItem(inputModel, "root", f));

			var rootJoinClause = new JoinItem { Alias = "root", JoinSql = "from table_{0}".ToFormat(_mapConfig.BaseTable.Name) };

			_selectedFields = new List<SelectItem>(rootSelectClauses);
			_joinClauses = new List<JoinItem> { rootJoinClause };
			_whereClauses = new List<WhereItem>(rootWhereClauses);

			_mapConfig.Joins.Each(j => BuildJoinItem(inputModel, j, rootJoinClause));

			return new MapQueryConfig
				{
					Joins = _joinClauses,
					Selects = _selectedFields,
					Wheres = _whereClauses,
				};
		}

		public void BuildJoinItem(IN inputModel, ModelMapConfig<IN, OUT> mapConfig, JoinItem parentJoinItem)
		{
			var toAlias = "T" + _aliasIndex++;
			var relation = (SchemaRelation)mapConfig.ViaRelation;
			const string rowIdColumnName = "objid";

			var selects = mapConfig.Fields.Where(f => f.OutProperty != null).Select(field => BuildSelectItem(toAlias, field, _selectIndex++));
			_selectedFields.AddRange(selects);

			var wheres = mapConfig.Fields.Where(f => f.Operator != null).Select(field => BuildWhereItem(inputModel, toAlias, field));
			_whereClauses.AddRange(wheres);

			//where this gets hairy

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
					joinBuilder.AppendFormat("{0}.{1} = {2} AND ", parentJoinItem.Alias, relation.FocusFieldName, mapConfig.ViaRelation.TargetTable.ObjectID);
				}
				else
				{
					fromField = mapConfig.ViaRelation.Name;
				}

				joinBuilder.AppendFormat("{0}.{1} = {2}.{3}", parentJoinItem.Alias, fromField, toAlias, toField);
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
				joinBuilder.AppendFormat("{0}.{1} = {2}.{3}", parentJoinItem.Alias, fromField, toAlias, toField);
			}

			else if (relation.Cardinality == SchemaRelationCardinality.ManyToMany)
			{
				var mtmTableAlias = String.Format("M{0}", _mtmAliasCount++);

				joinBuilder.AppendFormat("INNER JOIN {0} {1} ON {2}.{3} = {4}.{5}", relation.MtmTableName, mtmTableAlias, parentJoinItem.Alias, fromField, mtmTableAlias, relation.Name);
				joinBuilder.AppendFormat("{0} ON {1}.{2} = {3}.{4}", joinStart, mtmTableAlias, relation.InverseRelationName, toAlias, toField);
			}

			var joinClause = new JoinItem
				{
					Alias = toAlias,
					JoinSql = joinBuilder.ToString()
				};

			_joinClauses.Add(joinClause);

			mapConfig.Joins.Each(j => BuildJoinItem(inputModel, j, joinClause));
		}

		public SelectItem BuildSelectItem(string alias, FieldConfig field, int index)
		{
			return new SelectItem { Alias = alias, Field = field.SchemaField, Index = index };
		}

		public WhereItem BuildWhereItem(IN inputModel, string alias, FieldConfig field)
		{
			var value = field.InputValue;

			if (value == null && field.InputProperty != null)
			{
				value = field.InputProperty.GetValue(inputModel, null);
			}

			if (value == null)
			{
				throw new ApplicationException("Field {0} has not been given an input value.".ToFormat(field.SchemaField.Name));
			}

			return new WhereItem { Alias = alias, Field = field.SchemaField, Value = value };
		}
	}

	public class MapQueryConfig
	{
		public IEnumerable<SelectItem> Selects { get; set; }
		public IEnumerable<JoinItem> Joins { get; set; }
		public IEnumerable<WhereItem> Wheres { get; set; }
	}

	public class SelectItem
	{
		public int Index { get; set; }
		public string Alias { get; set; }
		public ISchemaField Field { get; set; }
	}

	public class JoinItem
	{
		public string Alias { get; set; }
		public string JoinSql { get; set; }
		//todo add support for outer join
	}

	public class WhereItem
	{
		public string Alias { get; set; }
		public object Value { get; set; }
		public ISchemaField Field { get; set; }
	}
}