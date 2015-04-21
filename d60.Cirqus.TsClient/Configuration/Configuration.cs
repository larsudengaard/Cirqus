﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace d60.Cirqus.TsClient.Configuration
{
    public class Configuration
    {
        public List<IgnoredPropertyConfiguration> IgnoredProperties { get; private set; }
        public List<BuiltInTypeConfiguration> BuiltInTypes { get; private set; }

        internal Configuration()
        {
            IgnoredProperties = new List<IgnoredPropertyConfiguration>();
            BuiltInTypes = new List<BuiltInTypeConfiguration>();
        }

        public class BuiltInTypeConfiguration
        {
            readonly Func<Type, bool> _predicate;

            public string TsType { get; private set; }

            public BuiltInTypeConfiguration(Func<Type, bool> predicate, string tsType)
            {
                TsType = tsType;
                _predicate = predicate;
            }

            public bool IsForType(Type type)
            {
                return _predicate(type);
            }
        }

        public class IgnoredPropertyConfiguration
        {
            public Type DeclaringType { get; private set; }
            public string PropertyName { get; private set; }

            public IgnoredPropertyConfiguration(Type declaringType, string propertyName)
            {
                DeclaringType = declaringType;
                PropertyName = propertyName;
            }

            public bool IsForType(Type type)
            {
                return DeclaringType.IsAssignableFrom(type);
            }
        }
    }
}