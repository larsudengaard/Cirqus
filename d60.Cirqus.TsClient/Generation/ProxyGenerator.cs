using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using d60.Cirqus.TsClient.Configuration;
using d60.Cirqus.TsClient.Model;

namespace d60.Cirqus.TsClient.Generation
{
    class ProxyGenerator
    {
        readonly IWriter _writer;
        readonly string _sourceDll;

        public ProxyGenerator(string sourceDll, IWriter writer)
        {
            _writer = writer;
            _sourceDll = sourceDll;
        }

        public List<ProxyGenerationResult> Generate()
        {
            return GetProxyGenerationResults().ToList();
        }

        IEnumerable<ProxyGenerationResult> GetProxyGenerationResults()
        {
            var assembly = LoadAssembly(_sourceDll);
            var allTypes = GetTypes(assembly);

            var configurators = allTypes.Where(x => typeof (TsClientConfigurator).IsAssignableFrom(x)).ToList();
            if (configurators.Count > 1)
            {
                throw new PrettyException("Found multiple configurations in command assembly, only one is supported.");
            }

            var configuration = new Configuration.Configuration();
            if (configurators.Count == 1)
            {
                var configuratorType = configurators.Single();
                var configurator = (TsClientConfigurator) Activator.CreateInstance(configuratorType);
                configuration = configurator.Configure();
            }

            var commandTypes = allTypes.Where(ProxyGeneratorContext.IsCommand).ToList();
            var viewTypes = allTypes.Where(ProxyGeneratorContext.IsView).ToList();

            _writer.Print("Found {0} command types", commandTypes.Count);
            _writer.Print("Found {0} view types", viewTypes.Count);

            var context = new ProxyGeneratorContext(commandTypes.Concat(viewTypes), configuration);
            
            var commandCode = context.GetDefinitions(TypeType.Command);
            yield return new ProxyGenerationResult("commands.ts", _writer, commandCode);

            var viewCode = context.GetDefinitions(TypeType.View);
            yield return new ProxyGenerationResult("views.ts", _writer, viewCode);

            var commonCode = context.GetDefinitions(TypeType.Other, TypeType.Primitive);
            yield return new ProxyGenerationResult("common.ts", _writer, commonCode);

            var commandProcessorCode = context.GetCommandProcessorDefinitation();
            yield return new ProxyGenerationResult("commandProcessor.ts", _writer, commandProcessorCode);
        }

        Type[] GetTypes(Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes();

                return types;
            }
            catch (ReflectionTypeLoadException exception)
            {
                var loaderExceptions = exception.LoaderExceptions;

                var message = string.Format(@"Could not load types from {0} - got the following loader exceptions: {1}", assembly, string.Join(Environment.NewLine, loaderExceptions.Select(e => e.ToString())));

                throw new ApplicationException(message);
            }
        }

        Assembly LoadAssembly(string filePath)
        {
            try
            {
                _writer.Print("Loading DLL {0}", filePath);
                return Assembly.LoadFrom(Path.GetFullPath(filePath));
            }
            catch (BadImageFormatException exception)
            {
                throw new BadImageFormatException(string.Format("Could not load {0}", filePath), exception);
            }
        }
    }
}