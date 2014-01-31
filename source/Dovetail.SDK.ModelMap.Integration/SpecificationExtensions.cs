using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
	public delegate void MethodThatThrows();

	public static class SpecificationExtensions
	{
		public static void ShouldBeFalse(this bool condition)
		{
			Assert.IsFalse(condition);
		}

		public static void ShouldBeTrue(this bool condition)
		{
			Assert.IsTrue(condition);
		}

        public static void ShouldBeFalse(this bool? condition)
        {
            Assert.IsFalse(condition.Value);
        }

        public static void ShouldBeTrue(this bool? condition)
        {
            Assert.IsTrue(condition.Value);
        }

		public static object ShouldEqual(this object actual, object expected)
		{
			Assert.AreEqual(expected, actual);
			return expected;
		}

		public static object ShouldBeClose(this DateTime actual, DateTime expected)
		{
			actual.Subtract(expected).TotalSeconds.ShouldBeLessThan(1);
			
			return expected;
		}

		public static object ShouldEqual(this object actual, object expected, string why)
		{
			Assert.AreEqual(expected, actual, why);
			return expected;
		}
		public static void ShouldHaveMatchingContents<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
		{
			if (!actual.Any())
				Assert.Fail("actual enumerable is empty");

			if(!expected.Any())
				Assert.Fail("expected enumerable is empty");

			var areDifferent = actual.Except(expected).Any();

			if(areDifferent)
			{
				var expectedValues = String.Join(",", actual.Select(s=>s.ToString()).ToArray());
				var foundValues = String.Join(",", expected.Select(s=>s.ToString()).ToArray());
				var message = "\nexpected: {0}\nfound: {1}".ToFormat(expectedValues, foundValues);
				Assert.Fail(message);
			}
		}

		public static XmlElement AttributeShouldEqual(this XmlElement element, string attributeName, object expected)
		{
			Assert.IsNotNull(element, "The Element is null");

			string actual = element.GetAttribute(attributeName);
			Assert.AreEqual(expected, actual);
			return element;
		}

		public static XmlElement AttributeShouldEqual(this XmlNode node, string attributeName, object expected)
		{
			var xmlElement = node as XmlElement;

			Assert.IsNotNull(xmlElement, "The Element is null");

			string actual = xmlElement.GetAttribute(attributeName);
			Assert.AreEqual(expected, actual);
			return xmlElement;
		}

		public static XmlElement ShouldHaveChild(this XmlElement element, string xpath)
		{
			XmlElement child = element.SelectSingleNode(xpath) as XmlElement;
			Assert.IsNotNull(child, "Should have a child element matching " + xpath);

			return child;
		}

		public static void ShouldMatch<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
		{
			var differences = actual.Except(expected).ToList();

			Assert.IsFalse(differences.Any(), "Enumerables have differences: {0}".ToFormat(String.Join(",", differences)));
		}

		public static XmlElement DoesNotHaveAttribute(this XmlElement element, string attributeName)
		{
			Assert.IsNotNull(element, "The Element is null");
			Assert.IsFalse(element.HasAttribute(attributeName), "Element should not have an attribute named " + attributeName);

			return element;
		}

		public static object ShouldNotEqual(this object actual, object expected)
		{
			Assert.AreNotEqual(expected, actual);
			return expected;
		}

		public static void ShouldBeNull(this object anObject)
		{
			Assert.IsNull(anObject);
		}

		public static void ShouldNotBeNull(this object anObject)
		{
			Assert.IsNotNull(anObject);
		}

		public static object ShouldBeTheSameAs(this object actual, object expected)
		{
			Assert.AreSame(expected, actual);
			return expected;
		}

		public static object ShouldNotBeTheSameAs(this object actual, object expected)
		{
			Assert.AreNotSame(expected, actual);
			return expected;
		}

		public static object ShouldBeOfType(this object actual, Type expected)
		{
			Assert.IsInstanceOf(expected, actual);
			return actual;
		}

		public static void ShouldNotBeOfType(this object actual, Type expected)
		{
			Assert.IsNotInstanceOf(expected, actual);
		}

		public static void ShouldContain(this IList actual, object expected)
		{
			Assert.Contains(expected, actual);
		}

		public static void ShouldContain<T>(this IList<T> actual, T expected)
		{
			ShouldContain((IList)actual, expected);
		}

		public static void ShouldContain<T>(this IEnumerable<T> actual, T expected)
		{
			if (actual.Count(t => t.Equals(expected)) == 0)
			{
				Assert.Fail("The item was not found in the sequence.");
			}
		}

		public static void ShouldNotBeEmpty<T>(this IEnumerable<T> actual)
		{
			Assert.Greater(actual.Count(), 0, "The list should have at least one element");
		}

		public static void ShouldNotContain<T>(this IEnumerable<T> actual, T expected)
		{
			if (actual.Count(t => t.Equals(expected)) > 0)
			{
				Assert.Fail("The item was found in the sequence it should not be in.");
			}
		}

		public static IComparable ShouldBeGreaterThan(this IComparable arg1, IComparable arg2)
		{
			Assert.Greater(arg1, arg2);
			return arg2;
		}

		public static IComparable ShouldBeLessThan(this IComparable arg1, IComparable arg2)
		{
			Assert.Less(arg1, arg2);
			return arg2;
		}

		public static void ShouldBeEmpty(this ICollection collection)
		{
			Assert.IsEmpty(collection);
		}

		public static void ShouldBeEmpty(this string aString)
		{
			Assert.IsEmpty(aString);
		}

		public static void ShouldNotBeEmpty(this ICollection collection)
		{
			Assert.IsNotEmpty(collection);
		}

		public static void ShouldNotBeEmpty(this string aString)
		{
			Assert.IsNotEmpty(aString);
		}

		public static void ShouldContain(this string actual, string expected)
		{
			StringAssert.Contains(expected, actual);
		}

		public static string ShouldBeEqualIgnoringCase(this string actual, string expected)
		{
			StringAssert.AreEqualIgnoringCase(expected, actual);
			return expected;
		}

		public static string ShouldBeEqualIgnoringCase(this string actual, string expected, string why)
		{
			StringAssert.AreEqualIgnoringCase(expected, actual, why);
			return expected;
		}

		public static void ShouldEndWith(this string actual, string expected)
		{
			StringAssert.EndsWith(expected, actual);
		}

		public static void ShouldStartWith(this string actual, string expected)
		{
			StringAssert.StartsWith(expected, actual);
		}

		public static void ShouldContainErrorMessage(this Exception exception, string expected)
		{
			StringAssert.Contains(expected, exception.Message);
		}

		public static Exception ShouldBeThrownBy(this Type exceptionType, MethodThatThrows method)
		{
			Exception exception = null;

			try
			{
				method();
			}
			catch (Exception e)
			{
				Assert.AreEqual(exceptionType, e.GetType());
				exception = e;
			}

			if (exception == null)
			{
				Assert.Fail(String.Format("Expected {0} to be thrown.", exceptionType.FullName));
			}

			return exception;
		}

		public static void ShouldEqualSqlDate(this DateTime actual, DateTime expected)
		{
			TimeSpan timeSpan = actual - expected;
			Assert.Less(Math.Abs(timeSpan.TotalMilliseconds), 3);
		}

		public static T IsType<T>(this object actual)
		{
			actual.ShouldNotBeNull();
			actual.ShouldBeOfType(typeof (T));
			return (T) actual;
		}

		public static string SafeString(this object o)
		{
			return o == null ? String.Empty : o.ToString();
		}

		public static void ShouldBeContainedInFile(this string expected, string filePath)
		{
			File.Exists(filePath).ShouldBeTrue();

			var fileContents = File.ReadAllText(filePath);
			fileContents.ShouldContain(expected);
		}
	}
}