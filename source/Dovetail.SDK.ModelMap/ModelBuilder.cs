using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.ModelMap.Clarify;
using Dovetail.SDK.ModelMap.ObjectModel;
using Dovetail.SDK.ModelMap.Registration;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;
using FChoice.Foundation.Schema;
using FubuCore;
using FubuCore.Reflection;

namespace Dovetail.SDK.ModelMap
{
	public class ModelBuilder<MODEL> : IModelBuilder<MODEL>
    {
        private readonly ModelMap<MODEL> _modelMap;
        private readonly IClarifyListCache _listCache;
        private readonly ISchemaCache _schemaCache;
        private readonly IModelBuilderResultEncoder _assemblerResultEncoder;
        private readonly IMapEntryBuilder _mapEntryBuilder;

    	public ModelBuilder(ModelMap<MODEL> modelMap, IClarifyListCache listCache, ISchemaCache schemaCache, IModelBuilderResultEncoder assemblerResultEncoder, IMapEntryBuilder mapEntryBuilder)
        {
            _modelMap = modelMap;
            _mapEntryBuilder = mapEntryBuilder;
        	_assemblerResultEncoder = assemblerResultEncoder;
            _schemaCache = schemaCache;
            _listCache = listCache;

            FieldSortMapOverrides = new FieldSortMap[0];
        }

        public IEnumerable<FieldSortMap> FieldSortMapOverrides { get; set; }

        public MODEL[] Get(Filter filter)
        {
            var rootGenericMap = _mapEntryBuilder.BuildFromModelMap(_modelMap);
            return assembleWithFilter(filter, rootGenericMap, null).Models;
        }

	    public MODEL[] Get(Func<FilterExpression, Filter> filterFunction)
	    {
            var filter = filterFunction(new FilterExpression());

            return Get(filter);
	    }

		public MODEL[] GetTop(Filter filter, int numberOfRecords)
		{
			return Get(filter, new PaginationRequest {CurrentPage = 1, PageSize = numberOfRecords}).Models;
		}

		public MODEL[] GetTop(Func<FilterExpression, Filter> filterFunction, int numberOfRecords)
		{
			return Get(filterFunction, new PaginationRequest {CurrentPage = 1, PageSize = numberOfRecords}).Models;
		}

		public MODEL GetOne(string identifier)
        {
            var rootGenericMap = _mapEntryBuilder.BuildFromModelMap(_modelMap);
            return assembleWithIdentifier(identifier, rootGenericMap);
        }

        public MODEL GetOne(int identifier)
        {
            var rootGenericMap = _mapEntryBuilder.BuildFromModelMap(_modelMap);
            return assembleWithIdentifier(identifier, rootGenericMap);
        }

		public PaginationResult<MODEL> Get(Func<FilterExpression, Filter> func, IPaginationRequest paginationRequest)
        {
            var filter = func(new FilterExpression());
            
            return Get(filter, paginationRequest);
        }

		public PaginationResult<MODEL> Get(Filter filter, IPaginationRequest paginationRequest)
        {
            var rootGenericMap = _mapEntryBuilder.BuildFromModelMap(_modelMap);
            return assembleWithFilter(filter, rootGenericMap, paginationRequest);
        }

		public MODEL[] GetAll(int dtoCountLimit)
		{
			var rootGenericMap = _mapEntryBuilder.BuildFromModelMap(_modelMap);
			
			if(dtoCountLimit > 0)
				rootGenericMap.ClarifyGeneric.MaximumRows = dtoCountLimit;

			return assembleWithSortOverrides(rootGenericMap, null).Models;
		}

		public MODEL[] GetAll()
		{
			return GetAll(-1);
		}

		private PaginationResult<MODEL> assembleWithFilter(Filter filter, ClarifyGenericMapEntry rootGenericMap, IPaginationRequest paginationRequest)
        {
        	rootGenericMap.ClarifyGeneric.Filter.AddFilter(filter);

        	return assembleWithSortOverrides(rootGenericMap, paginationRequest);
        }

		private PaginationResult<MODEL> assembleWithSortOverrides(ClarifyGenericMapEntry rootGenericMap, IPaginationRequest paginationRequest)
    	{
    		if (FieldSortMapOverrides.Any()) //override map sort with our own
    		{
    			rootGenericMap.ClarifyGeneric.ClearSorts();
    			foreach (var f in FieldSortMapOverrides)
    			{
    				rootGenericMap.ClarifyGeneric.AppendSort(f.FieldName, f.IsAscending);
    			}
    		}

    		return assemble(rootGenericMap, paginationRequest);
    	}

    	private MODEL assembleWithIdentifier(string identifier, ClarifyGenericMapEntry rootGenericMap)
        {
            var identifierFieldName = GetIdentifierFieldName(rootGenericMap);

            var filter = FilterType.Equals(identifierFieldName, identifier);

            return assembleWithFilter(filter, rootGenericMap, null).Models.FirstOrDefault();
        }

        private MODEL assembleWithIdentifier(int identifier, ClarifyGenericMapEntry rootGenericMap)
        {
            var identifierFieldName = GetIdentifierFieldName(rootGenericMap);

            var filter = FilterType.Equals(identifierFieldName, identifier);

			return assembleWithFilter(filter, rootGenericMap, null).Models.FirstOrDefault();
        }

        private static string GetIdentifierFieldName(ClarifyGenericMapEntry rootGenericMap)
        {
            var identifierFieldName = rootGenericMap.GetIdentifierFieldName();

            if (identifierFieldName == null)
            {
                throw new DovetailMappingException(1003, "Map for type {0} does not have an identifying field defined.", typeof(MODEL).Name);
            }

            return identifierFieldName;
        }

		private PaginationResult<MODEL> assemble(ClarifyGenericMapEntry rootGenericMap, IPaginationRequest paginationRequest)
		{
			var result = new PaginationResult<MODEL>();

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

            if (!childSubRootMaps.Any())
                return;

            foreach (var childGenericMap in childSubRootMaps)
            {
                var parentKeyField = childGenericMap.NewRoot.ParentKeyField;
                var subRootGeneric = childGenericMap.ClarifyGeneric;
                var rootKeyField = childGenericMap.NewRoot.RootKeyField;

                if (_schemaCache.IsIntegerField(parentGenericMap.ClarifyGeneric.TableName, parentKeyField))
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
        }

        private MODEL[] createDtosForMap(ClarifyGenericMapEntry genericMap, IEnumerable<ClarifyDataRow> records)
        {
            var dtos = new List<MODEL>();

            foreach (var record in records)
            {
                var dto = Activator.CreateInstance(genericMap.Model.ModelType);

                populateDTOForGenericRecord(genericMap, record, dto);

                dtos.Add((MODEL)dto);
            }

            return dtos.ToArray();
        }

        private void populateDTOForGenericRecord(ClarifyGenericMapEntry genericMap, ClarifyDataRow record, object dto)
        {
            populateDTOWithFieldValues(genericMap, record, dto);

            populateDTOWithRelatedGenericRecords(genericMap, record, dto);

            populateDTOWithRelatedDTOs(genericMap, record, dto);
        }

        private void populateDTOWithRelatedDTOs(ClarifyGenericMapEntry parentGenericMap, ClarifyDataRow parentRecord, object parentDto)
        {
            var childMapsForChildDtos = parentGenericMap.ChildGenericMaps.Where(child => parentDto.GetType() != child.Model.ModelType);

            foreach (var childMap in childMapsForChildDtos)
            {
                var parentProperty = new SingleProperty(childMap.Model.ParentProperty);
                var childModelType = childMap.Model.ModelType;

                if (typeof(IEnumerable).IsAssignableFrom(parentProperty.PropertyType))
                {
                    var propertyDTOs = new ArrayList();
                    foreach (var childRecord in parentRecord.RelatedRows(childMap.ClarifyGeneric))
                    {
                        var propertyDTO = Activator.CreateInstance(childModelType);

                        populateDTOForGenericRecord(childMap, childRecord, propertyDTO);

                        propertyDTOs.Add(propertyDTO);
                    }

                    parentProperty.SetValue(parentDto, propertyDTOs.ToArray(childModelType));
                }
                else //dto is not enumerable just get the first child row
                {
                    var relatedChildRows = parentRecord.RelatedRows(childMap.ClarifyGeneric);
                    if (relatedChildRows.Any())
                    {
                        var propertyDTO = Activator.CreateInstance(childModelType);
                        var childRecord = relatedChildRows.First();
                        populateDTOForGenericRecord(childMap, childRecord, propertyDTO);
                        parentProperty.SetValue(parentDto, propertyDTO);
                    }
                    else
                    {
                        parentProperty.SetValue(parentDto, null);
                    }
                }
            }
        }

    	private void populateDTOWithRelatedGenericRecords(ClarifyGenericMapEntry genericMap, ClarifyDataRow record, object dto)
        {
            var childMapsForDtoType = genericMap.ChildGenericMaps.Where(child => dto.GetType() == child.Model.ModelType).ToArray();

            populateSubRootMaps(dto, childMapsForDtoType, record);

            populateBasedOnRelatedGenerics(dto, childMapsForDtoType, record);
        }

        private void populateSubRootMaps(object dto, IEnumerable<ClarifyGenericMapEntry> childMapsForDtoType, ClarifyDataRow parentRecord)
        {
            var childSubRootMaps = childMapsForDtoType.Where(map => map.IsNewRoot());

            foreach (var childMap in childSubRootMaps)
            {
                var subRootGeneric = childMap.ClarifyGeneric;
                var rootKeyField = childMap.NewRoot.RootKeyField;
                var parentKeyField = childMap.NewRoot.ParentKeyField;

                var childRecord = findRelatedSubRecord(parentRecord, parentKeyField, subRootGeneric, rootKeyField);

                populateDTOForGenericRecord(childMap, childRecord, dto);
            }
        }

        private ClarifyDataRow findRelatedSubRecord(ClarifyDataRow parentRecord, string parentKeyField, ClarifyGeneric subRootGeneric, string rootKeyField)
        {
            if (_schemaCache.IsIntegerField(parentRecord.ParentGeneric.TableName, parentKeyField))
            {
                var parentKeyValue = Convert.ToInt32(parentRecord[parentKeyField]);
                return subRootGeneric.Rows.Cast<ClarifyDataRow>().First(row => Convert.ToInt32(row[rootKeyField]) == parentKeyValue);
            }

            var parentKeyString = parentRecord[parentKeyField].ToString();
            return subRootGeneric.Rows.Cast<ClarifyDataRow>().First(row => row[rootKeyField].ToString() == parentKeyString);
        }

        private void populateBasedOnRelatedGenerics(object dto, IEnumerable<ClarifyGenericMapEntry> childMapsForDtoType,
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

        private void populateDTOWithFieldValues(ClarifyGenericMapEntry genericMap, ClarifyDataRow record, object dto)
        {
            foreach (var fieldMap in genericMap.FieldMaps)
            {
                if (fieldMap.Property == null)
                    continue;

                var propertyValue = GetFieldValueForRecord(fieldMap, record);

                if (propertyValue is string && fieldMap.ShouldEncode)
                {
                    propertyValue = _assemblerResultEncoder.Encode((string)propertyValue);
                }

                if (fieldMap.Property.PropertyType == typeof(int))
                {
                    propertyValue = Convert.ToInt32(propertyValue);
                }

				//if(fieldMap.Property.PropertyType == typeof(DateTime))
				//{
				//    propertyValue = _timezoneService.ConvertToUserTimeZone(Convert.ToDateTime(propertyValue));
				//}

				try
				{
					fieldMap.Property.SetValue(dto, propertyValue, null);	
				}
				catch(Exception ex)
				{
					throw new ApplicationException("Could not set property on type {0}. Field: {1}".ToFormat(dto.GetType().Name, fieldMap.ToString()), ex);
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

				var globalList = _listCache.GetList(globalListConfig.ListName);

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
                return _listCache.GetHgbstList(fieldMap.UserDefinedList);
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