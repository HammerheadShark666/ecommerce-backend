using System.Reflection;
using ECommerce.Application.Abstractions.Email;
using ECommerce.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Email;

public sealed class EmailTemplateService(IOptions<UrlOptions> options) : IEmailTemplateService
{   
    public async Task<string> RenderAsync(string templateName,
                                          Dictionary<string, string> values)
    {
        string template = await GetHtmlTemplate(templateName);
        template = PopulateValues(template, values);

        return template;
    }      

    private static async Task<string> GetHtmlTemplate(string templateName)
    {
        Assembly assembly = typeof(EmailTemplateService).Assembly;

        string resourceName =
            $"ECommerce.Infrastructure.Email.Templates.{templateName}.html";

        await using Stream stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException(resourceName);

        using StreamReader reader = new(stream);

        return await reader.ReadToEndAsync(); 
    }

    private string PopulateValues(string template, Dictionary<string, string> values)
    {        
        values.Add("Year", DateTime.Now.Year.ToString());
        values.Add("BaseUrl", options.Value.BaseUrl);    

        foreach (KeyValuePair<string, string> value in values)
        {
            template = template.Replace(
                $"{{{{{value.Key}}}}}",
                value.Value ?? string.Empty);
        }

        return template;
    }
}
