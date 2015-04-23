using System;

namespace d60.Cirqus.TsClient.Model
{
    class BuiltInTypeDef : TypeDef
    {
        readonly string _code;
        readonly string _fullyQualifiedTsTypeName;

        public BuiltInTypeDef(Type type, string code, string fullyQualifiedTsTypeName, bool isSystemType = true) 
            : base(new QualifiedClassName(type), TypeType.Primitive, isSystemType)
        {
            Type = type;
            _code = code;
            _fullyQualifiedTsTypeName = fullyQualifiedTsTypeName;
        }

        public override string FullyQualifiedTsTypeName
        {
            get
            {
                return IsSystemType
                    ? _fullyQualifiedTsTypeName
                    : "common." + _fullyQualifiedTsTypeName;
            }
        }

        public override string GetCode(ProxyGeneratorContext context)
        {
            return _code;
        }
    }
}