using System;
using Dovetail.SDK.ModelMap.Registration.DSL;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.ModelMap.Extensions
{
    public static class MappingExtensions
    {
        public static IMapExpressionPostRoot<MODEL> DescribedElapsedTime<MODEL>(this IMapExpressionPostBasedOnField<MODEL> expression, DateTime basedOn)
        {
            Func<string, object> describeDateTime = value =>
            {
				DateTime dateTime;
				if (DateTime.TryParse(value, out dateTime))
					return string.Empty;

				return dateTime.ElapsedTimeDescription(true, basedOn);
            };

            expression.Do(describeDateTime);

            return expression as IMapExpressionPostRoot<MODEL>;
        }

        public static IMapExpressionPostRoot<MODEL> GetGlobalListElement<MODEL>(this IMapExpressionPostBasedOnField<MODEL> expression, string listName)
        {
            var listCache = ClarifyApplication.Instance.ListCache;

            Func<string, object> getListElementFromCache = value =>
            {
                var elementObjid = Convert.ToInt32(value);
                return listCache.GetGbstElmByID(listName, elementObjid);
            };

            expression.Do(getListElementFromCache);

            return expression as IMapExpressionPostRoot<MODEL>;
        }

    }
}