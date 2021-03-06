﻿using System.Collections.Generic;
using System.Xml.Linq;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class ParsingContext
	{
		private readonly IServiceLocator _services;
		private readonly Stack<XElement> _elements;
		private readonly Stack<object> _objects;

		public ParsingContext(IServiceLocator services)
		{
			_services = services;
			_elements = new Stack<XElement>();
			_objects = new Stack<object>();
		}

		public ISchemaCache Schema
		{
			get { return Service<ISchemaCache>(); }
		}

		public IXElementSerializer Serializer
		{
			get { return Service<IXElementSerializer>();  }
		}

		public XElement CurrentElement
		{
			get { return _elements.Count == 0 ? null : _elements.Peek(); }
		}

		public TService Service<TService>()
		{
			return _services.GetInstance<TService>();
		}

		public void PushElement(XElement element)
		{
			_elements.Push(element);
		}

		public XElement PopElement()
		{
			return _elements.Pop();
		}

		public TObject CurrentObject<TObject>()
		{
			return _objects.Peek().As<TObject>();
		}

		public bool IsCurrent<TObject>() where TObject : class
		{
			return _objects.Count != 0 && _objects.Peek() is TObject;
		}

		public void PushObject(object value)
		{
			_objects.Push(value);
		}

		public object PopObject()
		{
			return _objects.Pop();
		}
	}
}