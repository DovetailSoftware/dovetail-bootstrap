using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.Clarify;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.Legacy;
using Dovetail.SDK.ModelMap.ObjectModel;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
    public class ModelBuilder : IModelBuilder
    {
        private readonly IModelMapRegistry _models;
        private readonly ISchemaCache _schema;
        private readonly IOutputEncoder _encoder;
        private readonly IMapEntryBuilder _entries;
        private readonly IClarifyListCache _lists;
	    private readonly IServiceLocator _services;

        public ModelBuilder(IModelMapRegistry models, ISchemaCache schema, IOutputEncoder encoder, IMapEntryBuilder entries, IClarifyListCache lists, IServiceLocator services)
        {
            _models = models;
            _schema = schema;
            _encoder = encoder;
            _entries = entries;
            _lists = lists;
	        _services = services;

	        FieldSortMapOverrides = new FieldSortMap[0];
        }

	    public IEnumerable<FieldSortMap> FieldSortMapOverrides { get; set; }

        public ModelData GetOne(string name, int identifier)
        {
            var map = _models.Find(name);
            var rootGenericMap = _entries.BuildFromModelMap(map);
            return assembleWithIdentifier(map, identifier, rootGenericMap);
        }

	    public ModelData GetOne(string name, string identifier)
	    {
			var map = _models.Find(name);
			var rootGenericMap = _entries.BuildFromModelMap(map);
			return assembleWithIdentifier(map, identifier, rootGenericMap);
		}

	    public ModelData[] Get(string name, Filter filter)
	    {
			var map = _models.Find(name);
			var rootGenericMap = _entries.BuildFromModelMap(map);
			return assembleWithFilter(filter, rootGenericMap, null).Models;
		}

	    public ModelData[] Get(string name, Func<FilterExpression, Filter> filterFunction)
	    {
			var filter = filterFunction(new FilterExpression());

			return Get(name, filter);
		}

	    public ModelData[] GetTop(string name, Filter filter, int numberOfRecords)
	    {
			return Get(name, filter, new PaginationRequest { CurrentPage = 1, PageSize = numberOfRecords }).Models;
		}

	    public ModelData[] GetTop(string name, Func<FilterExpression, Filter> filterFunction, int numberOfRecords)
	    {
			return Get(name, filterFunction, new PaginationRequest { CurrentPage = 1, PageSize = numberOfRecords }).Models;
		}

	    public PaginationResult Get(string name, Filter filter, IPaginationRequest paginationRequest)
	    {
			var map = _models.Find(name);
			var rootGenericMap = _entries.BuildFromModelMap(map);
			return assembleWithFilter(filter, rootGenericMap, paginationRequest);
		}

	    public PaginationResult Get(string name, Func<FilterExpression, Filter> filterFunction, IPaginationRequest paginationRequest)
	    {
			var filter = filterFunction(new FilterExpression());

			return Get(name, filter, paginationRequest);
		}

	    public ModelData[] GetAll(string name)
	    {
		    return GetAll(name, -1);
	    }

		public ModelData[] GetAll(string name, int dtoCountLimit)
		{
			var map = _models.Find(name);
			var rootGenericMap = _entries.BuildFromModelMap(map);

			if (dtoCountLimit > 0)
				rootGenericMap.ClarifyGeneric.MaximumRows = dtoCountLimit;

			return assembleWithSortOverrides(rootGenericMap, null).Models;
		}

		private PaginationResult assembleWithFilter(Filter filter, ClarifyGenericMapEntry rootGenericMap, IPaginationRequest paginationRequest)
        {
            rootGenericMap.ClarifyGeneric.Filter.AddFilter(filter);

            return assembleWithSortOverrides(rootGenericMap, paginationRequest);
        }

        private PaginationResult assembleWithSortOverrides(ClarifyGenericMapEntry rootGenericMap, IPaginationRequest paginationRequest)
        {
            if (FieldSortMapOverrides.Any()) //override map sort with our own
            {
                rootGenericMap.ClarifyGeneric.ClearSorts();
                foreach (var f in FieldSortMapOverrides)
                {
                    rootGenericMap.ClarifyGeneric.AppendSort(f.Field.Resolve(_services).ToString(), f.IsAscending);
                }
            }

            return assemble(rootGenericMap, paginationRequest);
        }

        private ModelData assembleWithIdentifier(ModelMap map, int identifier, ClarifyGenericMapEntry rootGenericMap)
        {
            var identifierFieldName = findIdentifierFieldName(map, rootGenericMap);

            var filter = FilterType.Equals(identifierFieldName, identifier);

            return assembleWithFilter(filter, rootGenericMap, null).Models.FirstOrDefault();
        }

		private ModelData assembleWithIdentifier(ModelMap map, string identifier, ClarifyGenericMapEntry rootGenericMap)
		{
			var identifierFieldName = findIdentifierFieldName(map, rootGenericMap);

			var filter = FilterType.Equals(identifierFieldName, identifier);

			return assembleWithFilter(filter, rootGenericMap, null).Models.FirstOrDefault();
		}

		private static string findIdentifierFieldName(ModelMap map, ClarifyGenericMapEntry rootGenericMap)
        {
            var identifierFieldName = rootGenericMap.GetIdentifierFieldName();

            if (identifierFieldName == null)
            {
                throw new DovetailMappingException(1003, "Map \"{0}\" does not have an identifying field defined.", map.Name);
            }

            return identifierFieldName;
        }

        private PaginationResult assemble(ClarifyGenericMapEntry rootGenericMap, IPaginationRequest paginationRequest)
        {
            var result = new PaginationResult();

            var rootClarifyGeneric = rootGenericMap.ClarifyGeneric;

            //setup SDK generic for PaginationRequest if necessary
            if (paginationRequest != null)
            {
                result.CurrentPage = paginationRequest.CurrentPage;
                result.PageSize = paginationRequest.PageSize;

                rootClarifyGeneric.MaximumRows = 1; //hack! what if query has only one result?
                rootClarifyGeneric.MaximumRowsExceeded += (sender, args) =>
                {
                    args.RowsToReturn = paginationRequest.CurrentPage * paginationRequest.PageSize;
                    result.TotalRecordCount = args.TotalPossibleRows;
                };
            }

            rootClarifyGeneric.DataSet.Query(rootClarifyGeneric);

            traverseGenericsPopulatingSubRootMaps(rootGenericMap);

            var records = rootGenericMap.ClarifyGeneric.DataRows();

            //take the results and constrain them to the requested page 
            if (paginationRequest != null)
            {
                var startRow = (paginationRequest.CurrentPage - 1) * paginationRequest.PageSize;
                records = records.Skip(startRow).Take(paginationRequest.PageSize);
            }

            result.Models = createDtosForMap(rootGenericMap, records);

            return result;
        }

        private void traverseGenericsPopulatingSubRootMaps(ClarifyGenericMapEntry parentGenericMap)
        {
            if (parentGenericMap.ClarifyGeneric.Count < 1)
                return;

            var childSubRootMaps = parentGenericMap.ChildGenericMaps.Where(map => map.IsNewRoot()).ToArray();
            foreach (var childGenericMap in childSubRootMaps)
            {
                var parentKeyField = childGenericMap.NewRoot.ParentKeyField;
                var subRootGeneric = childGenericMap.ClarifyGeneric;
                var rootKeyField = childGenericMap.NewRoot.RootKeyField;

                if (_schema.IsIntegerField(parentGenericMap.ClarifyGeneric.TableName, parentKeyField))
                {
                    var parentIds = parentGenericMap.ClarifyGeneric.Rows.Cast<ClarifyDataRow>().Select(row => Convert.ToInt32(row[parentKeyField])).ToArray();
                    subRootGeneric.Filter.AddFilter(FilterType.IsIn(rootKeyField, parentIds));
                }
                else
                {
                    var parentIds = parentGenericMap.ClarifyGeneric.Rows.Cast<ClarifyDataRow>().Select(row => row[parentKeyField].ToString()).ToArray();
                    subRootGeneric.Filter.AddFilter(FilterType.IsIn(rootKeyField, parentIds));
                }

                subRootGeneric.Query();

                traverseGenericsPopulatingSubRootMaps(childGenericMap);
            }

			parentGenericMap.ChildGenericMaps.Where(map => !map.IsNewRoot()).Each(traverseGenericsPopulatingSubRootMaps);
		}

        private ModelData[] createDtosForMap(ClarifyGenericMapEntry genericMap, IEnumerable<ClarifyDataRow> records)
        {
            var rows = new List<ModelData>();

            foreach (var record in records)
            {
                var row = new ModelData { Name = genericMap.Model.ModelName, Entity = genericMap.Entity };

                populateDTOForGenericRecord(genericMap, record, row);

	            genericMap.Tags.Each(_ => row.AddTag(_));

	            if (!row.Has("entity") && genericMap.Entity.IsNotEmpty())
		            row["entity"] = genericMap.Entity;

				rows.Add(row);
            }

            return rows.ToArray();
        }

        private void populateDTOForGenericRecord(ClarifyGenericMapEntry genericMap, ClarifyDataRow record, ModelData dto)
        {
            populateDTOWithFieldValues(genericMap, record, dto);

			populateDTOWithRelatedGenericRecords(genericMap, record, dto);

            populateDTOWithRelatedDTOs(genericMap, record, dto);

			foreach (var transform in genericMap.Transforms)
			{
				transform.Execute(dto, _services);
			}
		}

        private void populateDTOWithRelatedDTOs(ClarifyGenericMapEntry parentGenericMap, ClarifyDataRow parentRecord, ModelData parentModel)
        {
            var childMapsForChildDtos = parentGenericMap.ChildGenericMaps.Where(child => parentModel.Name != child.Model.ModelName);

            foreach (var childMap in childMapsForChildDtos)
            {
                if (childMap.Model.IsCollection)
                {
                    var children = new List<ModelData>();
                    foreach (var childRecord in parentRecord.RelatedRows(childMap.ClarifyGeneric))
                    {
                        var childModel = new ModelData { Name = childMap.Model.ModelName };

                        populateDTOForGenericRecord(childMap, childRecord, childModel);

						if (!childModel.IsEmpty())
							children.Add(childModel);
                    }

                    parentModel[childMap.Model.ParentProperty] = children;
                }
                else if (parentGenericMap.ClarifyGeneric == childMap.ClarifyGeneric)
                {
					var childModel = new ModelData { Name = childMap.Model.ModelName };
					populateDTOForGenericRecord(childMap, parentRecord, childModel);

					if (!childModel.IsEmpty())
						parentModel[childMap.Model.ParentProperty] = childModel;
					else if (childMap.Model.AllowEmpty)
						parentModel[childMap.Model.ParentProperty] = null;
				}
				else
                {
                    var relatedChildRows = parentRecord.RelatedRows(childMap.ClarifyGeneric);
                    if (relatedChildRows.Any())
                    {
                        var childRecord = relatedChildRows.First();
                        var childModel = new ModelData {Name = childMap.Model.ModelName};
                        populateDTOForGenericRecord(childMap, childRecord, childModel);

						if (!childModel.IsEmpty())
							parentModel[childMap.Model.ParentProperty] = childModel;
						else if (childMap.Model.AllowEmpty)
							parentModel[childMap.Model.ParentProperty] = null;
					}
                    else
                    {
                        parentModel[childMap.Model.ParentProperty] = null;
                    }
                }
            }
        }

        private void populateDTOWithRelatedGenericRecords(ClarifyGenericMapEntry genericMap, ClarifyDataRow record, ModelData model)
        {
            var childMapsForDtoType = genericMap.ChildGenericMaps.Where(child => model.Name == child.Model.ModelName).ToArray();

            populateSubRootMaps(model, childMapsForDtoType, record);

            populateBasedOnRelatedGenerics(model, childMapsForDtoType, record);
        }

        private void populateSubRootMaps(ModelData dto, IEnumerable<ClarifyGenericMapEntry> childMapsForDtoType, ClarifyDataRow parentRecord)
        {
            var childSubRootMaps = childMapsForDtoType.Where(map => map.IsNewRoot());

            foreach (var childMap in childSubRootMaps)
            {
                var subRootGeneric = childMap.ClarifyGeneric;
                var rootKeyField = childMap.NewRoot.RootKeyField;
                var parentKeyField = childMap.NewRoot.ParentKeyField;

                var childRecord = findRelatedSubRecord(parentRecord, parentKeyField, subRootGeneric, rootKeyField);
	            if (childRecord == null)
		            continue;

                populateDTOForGenericRecord(childMap, childRecord, dto);
            }
        }

        private ClarifyDataRow findRelatedSubRecord(ClarifyDataRow parentRecord, string parentKeyField, ClarifyGeneric subRootGeneric, string rootKeyField)
        {
            if (_schema.IsIntegerField(parentRecord.ParentGeneric.TableName, parentKeyField))
            {
                var parentKeyValue = Convert.ToInt32(parentRecord[parentKeyField]);
                return subRootGeneric.Rows.Cast<ClarifyDataRow>().FirstOrDefault(row => Convert.ToInt32(row[rootKeyField]) == parentKeyValue);
            }

            var parentKeyString = parentRecord[parentKeyField].ToString();
            return subRootGeneric.Rows.Cast<ClarifyDataRow>().FirstOrDefault(row => row[rootKeyField].ToString() == parentKeyString);
        }

        private void populateBasedOnRelatedGenerics(ModelData dto, IEnumerable<ClarifyGenericMapEntry> childMapsForDtoType,
                                                    ClarifyDataRow record)
        {
            foreach (var childMap in childMapsForDtoType.Where(map => !map.IsNewRoot()))
            {
                foreach (var childRecord in record.RelatedRows(childMap.ClarifyGeneric))
                {
                    populateDTOForGenericRecord(childMap, childRecord, dto);
                }
            }
        }

        private void populateDTOWithFieldValues(ClarifyGenericMapEntry genericMap, ClarifyDataRow record, ModelData model)
        {
			populateDataWithFieldValues(genericMap.FieldMaps, record, model);
			populateDataWithFieldValues(genericMap.Model.FieldMaps, record, model);
		}

	    private void populateDataWithFieldValues(FieldMap[] fieldMaps, ClarifyDataRow record, ModelData model)
	    {
			foreach (var fieldMap in fieldMaps)
			{
				if (fieldMap.Key.IsEmpty())
					continue;

				try
				{
					var propertyValue = GetFieldValueForRecord(fieldMap, record);

					if (propertyValue is string && fieldMap.ShouldEncode)
					{
						propertyValue = _encoder.Encode((string)propertyValue);
					}

					if (fieldMap.PropertyType == typeof(int))
					{
						propertyValue = Convert.ToInt32(propertyValue);
					}

					if (fieldMap.PropertyType == typeof(DateTime))
					{
						var dateTime = Convert.ToDateTime(propertyValue);
						var utcDateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
						propertyValue = utcDateTime;
					}

					model[fieldMap.Key] = propertyValue;
				}
				catch (Exception ex)
				{
					throw new ApplicationException("Could not set property on type {0}. Field: {1}".ToFormat(model.GetType().Name, fieldMap.ToString()), ex);
				}
			}
		}


		private object GetFieldValueForRecord(FieldMap fieldMap, ClarifyDataRow record)
        {
            if (fieldMap.FieldValueMethod != null)
            {
                return fieldMap.FieldValueMethod();
            }

            if (fieldMap.GlobalListConfig != null)
            {
                var globalListConfig = fieldMap.GlobalListConfig;

                var globalList = _lists.GetList(globalListConfig.ListName);

                if (globalListConfig.SelectionFromRelation != null)
                {
                    var listDatabaseIdentifier = Convert.ToInt32(GetGenericFieldValue(fieldMap, record));
                    globalList.SetSelection(listDatabaseIdentifier);
                }

                if (globalListConfig.TitleToDisplayTitleMap != null)
                {
                    var map = globalListConfig.TitleToDisplayTitleMap;
                    globalList.PopulateDisplayTitles(map);
                }

                return globalList;
            }

            if (fieldMap.UserDefinedList != null)
            {
                return _lists.GetHgbstList(fieldMap.UserDefinedList);
            }

            if (fieldMap.MapFieldValuesToObject != null)
            {
                var fieldValues = GetGenericFieldValues(fieldMap, record);

                return fieldMap.MapFieldValuesToObject(fieldValues);
            }

            if (fieldMap.StringToFieldValueMethod != null)
            {
                var fieldValue = GetGenericFieldValue(fieldMap, record);
                var stringValue = Convert.ToString(fieldValue);

                return fieldMap.StringToFieldValueMethod(stringValue);
            }

            return GetGenericFieldValue(fieldMap, record);
        }

        private static string[] GetGenericFieldValues(FieldMap fieldMap, ClarifyDataRow record)
        {
            return fieldMap.FieldNames.Select(fieldName => record[fieldName].ToString()).ToArray();
        }

        private static object GetGenericFieldValue(FieldMap fieldMap, ClarifyDataRow record)
        {
            if (fieldMap.FieldNames.Length == 1)
            {
                var fieldName = fieldMap.FieldNames[0];
                var fieldValueForRecord = record[fieldName];

                if (DBNull.Value.Equals(fieldValueForRecord))
                {
                    fieldValueForRecord = null;
                }

                return fieldValueForRecord;
            }

            if (fieldMap.FieldNames.Length > 1)
            {
                var recordValues = Array.ConvertAll(fieldMap.FieldNames, x => record[x].ToString());

                return String.Join(" ", recordValues);
            }

            return new Exception("No fields were specified for this assignment");
        }
    }
}