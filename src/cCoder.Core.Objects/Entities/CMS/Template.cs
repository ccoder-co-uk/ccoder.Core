using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.CMS;

[Table("Templates", Schema = "CMS")]
public class Template : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public string ResourceKey { get; set; }

    public string RawString { get; set; }

    [ForeignKey("App")]
    public int AppId { get; set; }

    public virtual App App { get; set; }

    public QueuedEmail BuildEmailTo(string receiver, string subject, RenderParams r, object model, MailServer serverInfo, Config config = null, ILogger log = null)
    {
        QueuedEmail result = new()
        {
            MailServerName = serverInfo.Name,
            To = receiver,
            Subject = subject,
            Content = Render(model, r, config, log),
            IsBodyHtml = true,
            AppId = AppId
        };

        result.Content = result.Content.Replace("[email[subject]]", subject);
        result.Content = result.Content.Replace("[email[from]]", serverInfo.User);
        result.Content = result.Content.Replace("[email[to]]", receiver);

        return result;
    }

    public string Render(object model, RenderParams r, Config config = null, ILogger log = null)
    {
        List<Replacement> replacements = ContentHelper.DefaultReplacements(r, config).ToList();
        replacements.Add(new Replacement("[model]", model.ToJson()));
        replacements.AddRange(BuildModelReplacements(model));

        if (log is not null)
            log.LogDebug($"replacements: {replacements.ToArray().ToJson()}");

        string result = ContentHelper.ProcessContentString(ResourceKey, r, RawString, replacements);
        return result;
    }

    internal IEnumerable<Replacement> BuildModelReplacements(object model, string prefix = "")
    {
        if (model is string)
            return [new Replacement($"[theme[{prefix}]]", model.ToString())];
        else if (model is JObject)
            return BuildModelReplacementsForJObject(model, prefix);
        if (model is JArray)
            return BuildModelReplacementsForCollection(model, prefix);
        else if (model.GetType().GetInterface("IDynamicMetaObjectProvider") != null)
            return BuildModelReplacementsForDynamicObject(model, prefix);
        else if (model is IEnumerable)
            return BuildModelReplacementsForCollection(model, prefix);
        else
            return BuildModelReplacementsForObject(model, prefix);
    }

    private IEnumerable<Replacement> BuildModelReplacementsForCollection(object model, string prefix)
    {
        string bindingExpression = prefix ?? string.Empty;
        List<Replacement> result = [];
        int i = 0;

        foreach (object item in (IEnumerable)model)
        {
            string itemPrefix = bindingExpression + $"[{i}]";
            result.AddRange(BuildModelReplacements(item, itemPrefix));
            i++;
        }

        string lengthBinding = bindingExpression.Length == 0 ? "Length" : bindingExpression + ".Length";
        result.Add(new Replacement($"[model[{lengthBinding}]]", i.ToString()));

        return result;
    }

    private IEnumerable<Replacement> BuildModelReplacementsForObject(object model, string prefix)
        => model.GetType()
            .GetProperties()
            .SelectMany(p =>
            {
                object v = p.GetValue(model);
                string bindingExpression = prefix.Length > 0 ? prefix + "." + p.Name : p.Name;

                if (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                    return
                    [
                        new Replacement($"[model[{prefix}]]", model?.ToString() ?? string.Empty),
                        new Replacement($"[model[{bindingExpression}]]", v?.ToString() ?? string.Empty)
                    ];
                else if (v != null)
                    return BuildModelReplacements(v, $"{bindingExpression}");

                return [];
            })
            .Where(i => i.Old != null && i.New != null)
            .ToList();

    private IEnumerable<Replacement> BuildModelReplacementsForJObject(object model, string prefix)
    {
        IEnumerable<KeyValuePair<string, JToken>> values = (IEnumerable<KeyValuePair<string, JToken>>)model;
        return values.SelectMany(t =>
        {
            string bindingExpression = prefix.Length > 0 ? prefix + "." + t.Key : t.Key;

            if (t.Value.GetType() == typeof(JValue))
                return [ new Replacement($"[model[{bindingExpression}]]", t.Value?.ToString() ?? string.Empty) ];
            else if (t.Value != null)
                return BuildModelReplacements(t.Value, $"{bindingExpression}");

            return [];
        })
        .ToList();
    }

    private IEnumerable<Replacement> BuildModelReplacementsForDynamicObject(object model, string prefix)
    {
        IDictionary<string, object> dynamicModel = (IDictionary<string, object>)model;
        return dynamicModel.Keys.SelectMany(key =>
        {
            string bindingExpression = prefix.Length > 0 ? prefix + "." + key : key;
            List<Replacement> results = new() { new Replacement($"[model[{bindingExpression}]]", dynamicModel[key]?.ToString() ?? string.Empty) };

            if (dynamicModel[key] != null && !dynamicModel[key].GetType().IsValueType)
                results.AddRange(BuildModelReplacements(dynamicModel[key], bindingExpression));

            return results;
        })
        .ToList();
    }
}