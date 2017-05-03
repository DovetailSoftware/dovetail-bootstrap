using System.Collections.Generic;
using System.Xml.Linq;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public class ParsingContext
    {
        private readonly XElement _query;
        private readonly IServiceLocator _services;
        private readonly ModelMapCompilationReport _report;
        private readonly Stack<XElement> _elements;
        private readonly Stack<object> _objects;

        public ParsingContext(XElement query, IServiceLocator services, ModelMapCompilationReport report)
        {
            _query = query;
            _services = services;
            _report = report;
            _elements = new Stack<XElement>();
            _objects = new Stack<object>();
        }

        public ModelMapCompilationReport Report
        {
            get { return _report; }
        }

        public ISchemaCache Schema
        {
            get { return Service<ISchemaCache>(); }
        }

        public XElement Query
        {
            get { return _query; }
        }

        public XElement CurrentElement
        {
            get { return _elements.Count == 0 ? Query : _elements.Peek(); }
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