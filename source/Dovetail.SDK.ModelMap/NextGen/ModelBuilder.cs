using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public class ModelBuilder<IN, OUT>
	{
		private readonly MapQueryFactory<IN, OUT> _mapQueryFactory;

		public ModelBuilder(MapQueryFactory<IN, OUT> mapQueryFactory)
		{
			_mapQueryFactory = mapQueryFactory;
		}

		public IEnumerable<OUT> Execute(IN inputModel)
		{
			var query = _mapQueryFactory.Create(inputModel);

			return new OUT[0];
		}
	}
}