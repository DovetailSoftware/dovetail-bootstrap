//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Reflection;
//using Dovetail.Commons;
//using Dovetail.Hub.Mapping.Registration;
//using FubuCore.Util;
//using FubuValidation;
//using FubuValidation.Registration;
//using StructureMap;

namespace Dovetail.Hub.Mapping
{
	//TODO integrate with FubuValidation
	//public class ModelMapSchemaValidationSource : IValidationSource
	//{
	//    private readonly IContainer _container;
	//    private readonly MaximumSchemaFieldLengthValidationVisitor _maximumSchemaFieldLengthValidationVisitor;
	//    private readonly Cache<Type, IEnumerable<IValidationRule>> _rulesCache = new Cache<Type, IEnumerable<IValidationRule>>();

	//    public ModelMapSchemaValidationSource(IContainer container, MaximumSchemaFieldLengthValidationVisitor maximumSchemaFieldLengthValidationVisitor)
	//    {
	//        _container = container;
	//        _maximumSchemaFieldLengthValidationVisitor = maximumSchemaFieldLengthValidationVisitor;
	//        _rulesCache.OnMissing = type => GetRulesForGetType(type);
	//    }

	//    public IEnumerable<IValidationRule> RulesFor(Type type)
	//    {
	//        return _rulesCache[type];
	//    }

	//    private IEnumerable<IValidationRule> GetRulesForGetType(Type type)
	//    {
	//        var modelMapGenericType = typeof (ModelMap<>).MakeGenericType(type);

	//        var modelMap = _container.TryGetInstance(modelMapGenericType);

	//        if (modelMap == null) return new IValidationRule[0];

	//        modelMapGenericType.InvokeMember("Accept", BindingFlags.InvokeMethod |BindingFlags.Instance |BindingFlags.Public, null, modelMap, new[] {_maximumSchemaFieldLengthValidationVisitor});

	//        return _maximumSchemaFieldLengthValidationVisitor.Rules;
	//    }
	//}
}