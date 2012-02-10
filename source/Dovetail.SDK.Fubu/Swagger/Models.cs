using System;
using Newtonsoft.Json;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class ResourceDiscovery
    {
        public string basePath { get; set; }
        public string swaggerVersion { get; set; }
        public string apiVersion { get; set; }
        public ResourceAPI[] apis { get; set; }
    }

    public class ResourceAPI
    {
        public string path { get; set; }
        public string description { get; set; }
    }

    public class Resource
    {
        public string basePath { get; set; }
        public string resourcePath { get; set; }
        public string swaggerVersion { get; set; }
        public string apiVersion { get; set; }
        public API[] apis { get; set; }


        [JsonConverter(typeof(ToJsonSchemaConverter))]
        public Type[] models { get; set; }
    }

    public class API
    {
        public string path { get; set; }
        public string description { get; set; }
        public Operation[] operations { get; set; }
    }

    public class Operation
    {
        public Parameter[] parameters { get; set; }
        public ErrorResponses[] errorResponses { get; set; }
        public string httpMethod { get; set; }
        public string notes { get; set; }
        public string responseTypeInternal { get; set; }
        public string responseClass { get; set; }
        public string nickname { get; set; }
        public string parameterType { get; set; }

    }

    public class Parameter
    {
        public string path { get; set; }
        public string defaultValue { get; set; }
        public string description { get; set; }
        public string dataType { get; set; }
        public AllowableValues[] allowableValues { get; set; }
        public string allowMultiple { get; set; }
        public string parameterType { get; set; }
    }

    public class AllowableValues
    {
        public string max { get; set; }
        public string min { get; set; }
        public string[] values { get; set; }
        public string valueType { get; set; }
    }

    public class ErrorResponses
    {
        public string reason { get; set; }
        public int code { get; set; }
    }

    public class Model
    {
        public string id { get; set; }
        public ModelProperty[] properties { get; set; }
    }

    public class ModelProperty
    {
        public string path { get; set; }
        public string defaultValue { get; set; }
        public string description { get; set; }
        public string dataType { get; set; }
        public AllowableValues[] allowableValues { get; set; }
        public string allowMultiple { get; set; }
        public string parameterType { get; set; }
    }
}