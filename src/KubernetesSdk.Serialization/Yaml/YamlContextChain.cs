using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Kubernetes.Serialization.Yaml;

internal sealed class YamlContextChain : StaticContext
{
    private readonly IEnumerable<StaticContext> _contexts;
    private readonly StaticObjectFactory _factory;
    private readonly TypeInspector _typeInspector;
    private readonly TypeResolver _typeResolver;

    public YamlContextChain(IEnumerable<StaticContext> contexts)
    {
        _contexts = contexts.ToArray();
        _factory = new StaticObjectFactory(_contexts);
        _typeInspector = new TypeInspector(_contexts);
        _typeResolver = new TypeResolver(_contexts);
    }

    public override bool IsKnownType(Type type)
    {
        return _contexts.Any(c => c.IsKnownType(type));
    }

    private sealed class StaticObjectFactory : YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory
    {
        private readonly IEnumerable<StaticContext> _contexts;

        public StaticObjectFactory(IEnumerable<StaticContext> contexts)
        {
            _contexts = contexts;
        }

        private YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory GetFactory(Type type)
        {
            YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory? factory = _contexts
                .FirstOrDefault(c => c.IsKnownType(type))
                ?.GetFactory();

            if (factory == null)
            {
                throw new NotSupportedException("Unsupported type: " + type);
            }

            return factory;
        }

        private YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory GetFactory(object value)
        {
            return GetFactory(value.GetType());
        }

        public override object Create(Type type)
        {
            return GetFactory(type)
                .Create(type);
        }

        public override Array CreateArray(Type type, int count)
        {
            return GetFactory(type)
                .CreateArray(type, count);
        }

        public override bool IsDictionary(Type type)
        {
            return GetFactory(type)
                .IsDictionary(type);
        }

        public override bool IsArray(Type type)
        {
            return GetFactory(type)
                .IsArray(type);
        }

        public override bool IsList(Type type)
        {
            return GetFactory(type)
                .IsList(type);
        }

        public override Type GetKeyType(Type type)
        {
            return GetFactory(type)
                .GetKeyType(type);
        }

        public override Type GetValueType(Type type)
        {
            return GetFactory(type)
                .GetValueType(type);
        }

        public override void ExecuteOnDeserializing(object value)
        {
            GetFactory(value)
                .ExecuteOnDeserializing(value);
        }

        public override void ExecuteOnDeserialized(object value)
        {
            GetFactory(value)
                .ExecuteOnDeserialized(value);
        }

        public override void ExecuteOnSerializing(object value)
        {
            GetFactory(value)
                .ExecuteOnSerializing(value);
        }

        public override void ExecuteOnSerialized(object value)
        {
            GetFactory(value)
                .ExecuteOnSerialized(value);
        }
    }

    private sealed class TypeInspector : ITypeInspector
    {
        private readonly IEnumerable<StaticContext> _contexts;

        public TypeInspector(IEnumerable<StaticContext> contexts)
        {
            _contexts = contexts;
        }

        private ITypeInspector GetTypeInspector(Type type)
        {
            ITypeInspector? typeInspector = _contexts.FirstOrDefault(c => c.IsKnownType(type))
                                                     ?.GetTypeInspector();

            if (typeInspector == null)
            {
                throw new NotSupportedException("Unsupported type: " + type.FullName);
            }

            return typeInspector;
        }

        public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            return GetTypeInspector(type)
                .GetProperties(type, container);
        }

        public IPropertyDescriptor GetProperty(Type type, object? container, string name, bool ignoreUnmatched)
        {
            return GetTypeInspector(type)
                .GetProperty(type, container, name, ignoreUnmatched);
        }
    }

    private sealed class TypeResolver : ITypeResolver
    {
        private readonly IEnumerable<StaticContext> _contexts;

        public TypeResolver(IEnumerable<StaticContext> contexts)
        {
            _contexts = contexts;
        }

        public Type Resolve(Type staticType, object? actualValue)
        {
            ITypeResolver? typeResolver = _contexts.FirstOrDefault(c => c.IsKnownType(staticType))
                                                   ?.GetTypeResolver();

            if (typeResolver == null)
            {
                throw new NotSupportedException("Unsupported type: " + staticType.FullName);
            }

            return typeResolver.Resolve(staticType, actualValue);
        }
    }

    public override YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory GetFactory()
    {
        return _factory;
    }

    public override ITypeInspector GetTypeInspector()
    {
        return _typeInspector;
    }

    public override ITypeResolver GetTypeResolver()
    {
        return _typeResolver;
    }
}