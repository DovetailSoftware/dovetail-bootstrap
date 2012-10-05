using System;
using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.ModelMap.Extensions
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
}