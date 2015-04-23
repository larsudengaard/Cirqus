using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace d60.Cirqus.TsClient.Generation
{
    class ProxyGenerationResult
    {
        static readonly Encoding Encoding = Encoding.UTF8;

        readonly string _filename;
        readonly IWriter _writer;
        readonly string _code;
        readonly string[] _dependencies;

        public ProxyGenerationResult(string filename, IWriter writer, string code, params string[] dependencies)
        {
            _filename = filename;
            _writer = writer;
            _code = code;
            _dependencies = dependencies;
        }

        public void WriteTo(string destinationDirectory)
        {
            var destinationFilePath = Path.Combine(destinationDirectory, _filename);

            if (File.Exists(destinationFilePath) && !HasChanged(destinationFilePath))
            {
                _writer.Print("    No changes - skipping {0}", destinationFilePath);
                return;
            }

            _writer.Print("    Writing {0}", destinationFilePath);
            var header = string.Format(HeaderTemplate, HashPrefix, GetHash());
            
            var output = new StringBuilder();
            output.Append(header);
            output.AppendLine("");
            output.AppendLine("");

            if (_dependencies.Any())
            {
                var imports = _dependencies.Select(x => string.Format("import {0} = require('{0}');", x));

                output.Append(string.Join(Environment.NewLine, imports));
                output.AppendLine("");
                output.AppendLine("");
            }

            output.AppendLine(_code);

            File.WriteAllText(destinationFilePath, output.ToString(), Encoding);
        }

        string GetHash()
        {
            return Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.GetBytes(_code)));
        }

        bool HasChanged(string destinationFilePath)
        {
            return GetHash() != GetHashFromFile(destinationFilePath);
        }

        string GetHashFromFile(string destinationFilePath)
        {
            using (var file = File.OpenRead(destinationFilePath))
            using (var reader = new StreamReader(file, Encoding))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var trimmedLine = line.TrimStart();
                    
                    if (string.IsNullOrWhiteSpace(trimmedLine)) break;

                    if (!trimmedLine.StartsWith(HashPrefix)) continue;
                    
                    var hash = trimmedLine.Substring(HashPrefix.Length);

                    return hash;
                }
            }

            return "";
        }

        const string HeaderTemplate = @"/* 
    Generated with d60.Cirqus.TsClient.exe
    Should not be edited directly - should probably be regenerated instead :)
    {0}{1}
*/";
        const string HashPrefix = "Hash: ";
    }
}