using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SKP.OS.Backend;

public class XmlDocsOperationTransformer : IOpenApiOperationTransformer
{
    private readonly XDocument _xmlDocs;

    public XmlDocsOperationTransformer(IWebHostEnvironment environment)
    {
        var xmlPath = Path.Combine(
            environment.ContentRootPath,
            $"{typeof(Program).Assembly.GetName().Name}.xml");
        if (!File.Exists(xmlPath))
        {
            xmlPath = Path.Combine(
                AppContext.BaseDirectory,
                $"{typeof(Program).Assembly.GetName().Name}.xml");
        }
        _xmlDocs = File.Exists(xmlPath) ? XDocument.Load(xmlPath) : new XDocument();
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            var member = FindMember(actionDescriptor.MethodInfo);
            if (member == null)
            {
                return Task.CompletedTask;
            }

            var summaryEl = member.Element("summary");
            if (summaryEl != null)
            {
                var summary = FlattenText(summaryEl);
                if (!string.IsNullOrEmpty(summary))
                {
                    operation.Summary = summary;
                }
            }

            var remarksEl = member.Element("remarks");
            if (remarksEl != null)
            {
                var description = FlattenParagraphs(remarksEl);
                if (!string.IsNullOrEmpty(description))
                {
                    operation.Description = description;
                }
            }

            foreach (var parameter in operation.Parameters ?? [])
            {
                var actionParameter = (actionDescriptor.Parameters ?? [])
                    .FirstOrDefault(p => string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));
                if (actionParameter is null)
                {
                    continue;
                }

                var paramDoc = member
                    .Elements("param")
                    .FirstOrDefault(e => string.Equals(e.Attribute("name")?.Value, actionParameter.Name, StringComparison.Ordinal));
                if (paramDoc != null && !string.IsNullOrEmpty(paramDoc.Value.Trim()))
                {
                    parameter.Description = paramDoc.Value.Trim();
                }
            }
        }

        return Task.CompletedTask;
    }

    private static string FlattenText(XContainer container)
    {
        var sb = new StringBuilder();
        AppendNodes(container, sb, flattenParagraphs: false);
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }

    private static string FlattenParagraphs(XContainer container)
    {
        var sb = new StringBuilder();
        AppendNodes(container, sb, flattenParagraphs: true);
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }

    private static void AppendNodes(XContainer container, StringBuilder sb, bool flattenParagraphs)
    {
        foreach (var node in container.Nodes())
        {
            if (node is XElement el)
            {
                if (el.Name.LocalName == "para")
                {
                    if (flattenParagraphs)
                    {
                        sb.AppendLine();
                    }
                    AppendNodes(el, sb, flattenParagraphs);
                    sb.Append(' ');
                }
                else
                {
                    AppendElement(el, sb);
                }
            }
            else if (node is XText text)
            {
                sb.Append(text.Value);
            }
        }
    }

    private static void AppendElement(XElement el, StringBuilder sb)
    {
        switch (el.Name.LocalName)
        {
            case "see":
            case "paramref":
                var cref = el.Attribute("cref")?.Value;
                sb.Append(cref != null ? LastSegment(cref) : el.Value);
                break;
            case "para":
                AppendNodes(el, sb, flattenParagraphs: true);
                sb.Append(' ');
                break;
            default:
                AppendNodes(el, sb, flattenParagraphs: true);
                break;
        }
    }

    private static string LastSegment(string cref) =>
        cref.Contains('.') ? cref[(cref.LastIndexOf('.') + 1)..] : cref;

    private XElement? FindMember(MethodInfo method)
    {
        if (method.DeclaringType is null)
        {
            return null;
        }

        var typeFullName = method.DeclaringType.FullName;
        var signature = $"{method.Name}({string.Join(",", method.GetParameters().Select(p => GetTypeName(p.ParameterType)))})";

        return _xmlDocs.Descendants("member")
            .FirstOrDefault(m =>
            {
                var name = m.Attribute("name")?.Value ?? string.Empty;
                if (!name.StartsWith($"M:{typeFullName}.", StringComparison.Ordinal))
                {
                    return false;
                }
                return name[($"M:{typeFullName}.").Length..].StartsWith(signature, StringComparison.Ordinal);
            });
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericName = type.GetGenericTypeDefinition().FullName!;
            genericName = genericName[..genericName.IndexOf('`')];
            var args = string.Join(",", type.GetGenericArguments().Select(GetTypeName));
            return $"{genericName}{{{args}}}";
        }
        if (type.IsArray)
        {
            return $"{GetTypeName(type.GetElementType()!)}[]";
        }
        return type.FullName ?? type.Name;
    }
}
