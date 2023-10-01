using System;
using System.IO;
using System.Threading.Tasks;
using Scriban;
using Scriban.Runtime;

namespace Kubernetes.Generator;

public class TemplateRenderer
{
    private static readonly string ResourceBaseName = $"Kubernetes.Generator.templates.";
    private readonly string _templatePath;
    private readonly TemplateContext _context = new ();
    private Template? _template;

    public TemplateRenderer(string templatePath)
    {
        _templatePath = ResourceBaseName + templatePath.Replace('\\', '.');
        _context.StrictVariables = true;
        _context.EnableRelaxedMemberAccess = false;
        _context.MemberRenamer = member => member.Name;
    }

    public async Task<string> RenderAsync(object model)
    {
        if (_template == null)
        {
            Stream? templateTextStream = typeof(TemplateRenderer).Assembly.GetManifestResourceStream(_templatePath);
            if (templateTextStream == null)
            {
                throw new FileNotFoundException("Embedded resource not found.", _templatePath);
            }

            using var templateTextReader = new StreamReader(templateTextStream);
            _template = Template.Parse(
                await templateTextReader.ReadToEndAsync()
                                        .ConfigureAwait(false));
        }

        var global = new ScriptObject();
        global.Import(typeof(TemplateFunctions));
        global.Add("Model", model);

        // global.Import(model, _context.MemberFilter, _context.MemberRenamer);
        _context.PushGlobal(global);

        try
        {
            return await _template.RenderAsync(_context)
                                  .ConfigureAwait(false);
        }
        catch (Exception error)
        {
            throw new ApplicationException($"Error rendering template {_templatePath}", error);
        }
        finally
        {
            _context.PopGlobal();
        }
    }
}
