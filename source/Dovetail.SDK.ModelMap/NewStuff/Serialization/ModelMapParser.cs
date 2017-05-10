using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public class ModelMapParser : IModelMapParser
    {
        private readonly IServiceLocator _services;
        private readonly IXElementService _elementService;
        private readonly ILogger _logger;

        public ModelMapParser(IServiceLocator services, IXElementService elementService, ILogger logger)
        {
            _services = services;
            _elementService = elementService;
            _logger = logger;
        }

        public ModelMap Parse(string filePath)
        {
            _logger.LogInfo("Parsing model map config at: " + filePath);

            using (var reader = new StreamReader(filePath))
            {
                var doc = XDocument.Load(reader);
                var report = new ModelMapCompilationReport();

                ModelMap config;
                try
                {
                    config = Parse(doc, report);
                }
                catch (Exception exc)
                {
                    report.AddError(exc.Message);
                    report.ReportTo(_logger);

                    throw;
                }

                return config;
            }
        }

	    public void Parse(ModelMap map, string filePath)
	    {
			_logger.LogInfo("Parsing model map config at: " + filePath);
			using (var reader = new StreamReader(filePath))
			{
				var doc = XDocument.Load(reader);
				var report = new ModelMapCompilationReport();

				try
				{
					parse(map, doc, report);
				}
				catch (Exception exc)
				{
					report.AddError(exc.Message);
					report.ReportTo(_logger);

					throw;
				}
			}
		}

	    public ModelMap Parse(XDocument document, ModelMapCompilationReport report)
        {
            var root = document.Root;
            var name = root.Attribute("name");
            if (name == null)
                throw new InvalidOperationException("No name specified");
            
            var map = new ModelMap(name.Value);
		    parse(map, document, report);

            return map;
        }

	    private void parse(ModelMap map, XDocument document, ModelMapCompilationReport report)
	    {
			var root = document.Root;
			var context = new ParsingContext(_services, report);

			map.AddInstruction(new BeginModelMap(map.Name));
			context.PushObject(map);

			root
				.Elements()
				.Each(_ => _elementService.Visit(_, map, context));

			context.PopObject();
			map.AddInstruction(new EndModelMap());
		}
    }
}