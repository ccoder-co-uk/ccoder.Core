using Core.Objects.Dtos;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.DMS;
using Core.Objects.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Text;
using File = Core.Objects.Entities.DMS.File;

namespace Core.Objects
{
    public static class ContentHelper
    {
        const string tag = @"\[TYPE\[[A-Za-z\d_/-]*\][A-Za-z\d_/-]*\=*\""*-*[A-Za-z\d_/-]*\""*\]";

        public static IMetadataCache MetaCache { get; set; }
        public static ICommonObjectCache ObjectCache { get; set; }

        /// <summary>
        /// Given render params and optionally a PageInfo (page metadata object) the default set of replacements can be constructed.
        /// These are basic string.Replace(old, new) type calls, everyhting else in the tag handling is done with regex
        /// </summary>
        /// <param name="p">the params</param>
        /// <param name="meta">the page meta</param>
        /// <returns>replacement collection</returns>
        public static ICollection<Replacement> DefaultReplacements(RenderParams p, Config config = null)
        {
            p.Culture ??= string.Empty;

            string culture = p.Culture.IsNullOrEmpty()
                ? p.App.DefaultCultureId
                : p.Culture;

            string port = config.Settings.TryGetValue("sslPort", out string value) 
                ? $":{value}"
                : string.Empty;

            List<Replacement> result =
            [
                new Replacement("[[user]]", new { p.User?.Id, p.User?.DefaultCultureId, p.User?.DisplayName, p.User?.Email }.ToJson()),
                new Replacement("[[displayname]]", p.User?.DisplayName),

                new Replacement("[[loginlink]]", p.User?.Id == "Guest"
                    ? "<a href='/Login'>[resource_displayname[Login]]</a>"
                    : "<a name='logout' href=''>[resource_displayname[Logout]]</a>"),

                new Replacement("[[date]]", DateTimeOffset.UtcNow.ToString("dd MMM yyyy")),
                new Replacement("[[culture]]", culture),
                new Replacement("[[lang]]", culture.Split('-').First()),
                new Replacement("[app[name]]", p.App?.Name),
                new Replacement("[app[domain]]", p.App?.Domain),
                new Replacement("[app[root]]", $"https://{p.App?.Domain}{port}/"),
                new Replacement("[app[id]]", p.App?.Id.ToString()),

                new Replacement("[[editlink]]", p.User.Can(p.App?.Id ?? 0, "page_update")
                    ? "<p style='cursor:pointer' onclick=\"setQueryParameter('edit', true)\">Edit</p>"
                    : "")
            ];

            if (config != null)
            {
                result.Add(new Replacement("[api[workflow]]", config.Services["Workflow"]));
                result.Add(new Replacement("[api[root]]", $"https://{p.App?.Domain}{port}/Api/"));
            }

            if (p is ComponentRenderParams crp)
            {
                result.Add(new Replacement("[theme[name]]", crp.Theme));
                IDictionary<string, object> themeDictionary = (IDictionary<string, object>)p.App.Config.Themes;
                result.AddRange(BuildThemeReplacements(themeDictionary[crp.Theme]));
            }

            if (p is TemplateRenderParams)
            {
                result.Add(new Replacement("[theme[name]]", "Default"));
                IDictionary<string, object> themeDictionary = (IDictionary<string, object>)p.App.Config.Themes;
                result.AddRange(BuildThemeReplacements(themeDictionary.First().Value));
            }

            if (p is PageRenderParams prp)
            {
                result.AddRange(
                [
                    new Replacement("[page[title]]", prp.Page.Title(prp.Culture)),
                    new Replacement("[page[description]]", prp.Page.Description(prp.Culture)),
                    new Replacement("[page[keywords]]", prp.Page.Keywords(prp.Culture)),
                    new Replacement("[page[id]]", prp.Page?.Id.ToString()),
                    new Replacement("[page[parentid]]", prp.Page?.ParentId.ToString()),
                    new Replacement("[page[path]]", prp.Page?.Path),
                    new Replacement("[page[url]]", $"https://{p.App?.Domain}/{prp.Page?.Path}")
                ]);
            }

            return result;
        }

        /// <summary>
        /// Processes a string of content replacing tags recursively until the content string is complete.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="content"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        public static string ProcessContentString(string key, RenderParams p, string content, IEnumerable<Replacement> replacements)
        {
            if (content == null)
                return string.Empty;

            DateTimeOffset renderStart = DateTimeOffset.Now;

            key ??= "Default";
            p.Culture ??= string.Empty;

            var validateStartTime = DateTimeOffset.Now - renderStart;

            ValidateRenderParams(p, replacements);

            StringBuilder result = new(content, content.Length * 4);

            if (p is PageRenderParams prp)
            {
                Content(key, result, prp, replacements);
                Nav(result, prp);
            }

            if (p is ComponentRenderParams crp)
                DMS(key, result, crp, replacements);

            Script(key, result, p, replacements);
            result.RegexReplace(tag.Replace("TYPE", "culturelink"), (m) => "?culture=" + m.TagName());
            Component(key, p, replacements, result);
            Meta(result, p.Culture);
            Resource(key, result, p, replacements);

            replacements.ForEach(r => result.Replace(r.Old, r.New));

            string returnString = result.ToString();

            return returnString;

        }

        private static void Nav(StringBuilder result, PageRenderParams prp)
        {
            string BuildMenuItemsFor(Page page, bool expand) =>
                    string.Join("", prp.App.Pages
                        .Where(sub => sub.ParentId == page?.Id && sub.ShowOnMenus)
                        .OrderBy(p => p.Order)
                        .Select(s =>
                        {
                            // can happen if any page is in an invalid state in the db and has no page info rows
                            string selected = s.ParentId != null && page != null && prp.Page.Path.Contains(s.Path) ? "selected" : string.Empty;
                            return expand
                                ? $"<li data-id='{s.Id}' class='item {selected}'><a href='/{s.Path}'>{s.Title(prp.Culture)}</a><ul class='submenu'>{BuildMenuItemsFor(s, expand)}</ul></li>"
                                : $"<li data-id='{s.Id}' class='item {selected}'><a href='/{s.Path}'>{s.Title(prp.Culture)}</a></li>";
                        }));

            string BuildMenuFor(Page page, bool expand)
            {
                string subs = BuildMenuItemsFor(page, expand);
                return $"<nav><ul name='menu' class='menu'>{subs}</ul></nav>";
            }

            result.RegexReplace(tag.Replace("TYPE", "nav"), (m) =>
            {
                _ = int.TryParse(m.TagName(), out int pageId);
                var page = prp.App.Pages.FirstOrDefault(p => p.Id == pageId);
                return BuildMenuFor(page, false);
            });

            result.RegexReplace(tag.Replace("TYPE", "navExpanded"), (m) =>
            {
                _ = int.TryParse(m.TagName(), out int pageId);
                var page = prp.App.Pages.FirstOrDefault(p => p.Id == pageId);
                return BuildMenuFor(page, true);
            });
        }

        private static void ValidateRenderParams(RenderParams p, IEnumerable<Replacement> replacements)
        {
            if (p == null)
                throw new Exception("No render Params given during render operation ?!?");

            if (p?.App == null)
                throw new Exception("No App info given during render operation ?!?");

            if (p?.App?.Resources == null)
                throw new Exception("No Resources provided with app during render operation ?!?");

            if (replacements == null)
                throw new Exception("No Replacement info given during render operation ?!?");
        }

        static (string, string, string[]) SplitMatch(System.Text.RegularExpressions.Match match)
        {
            var tagParts = match.ToString().Split("[", StringSplitOptions.None);
            var tagParts2 = tagParts.Last().Split("]", StringSplitOptions.None);

            return (tagParts[1].ToLower(), tagParts2[0].ToLower(), tagParts2[1].Split("|", StringSplitOptions.RemoveEmptyEntries));
        }

        static void Component(string key, RenderParams p, IEnumerable<Replacement> replacements, StringBuilder result)
        {
            if (p is PageRenderParams prp && prp.Edit)
                return;

            result.RegexReplace(tag.Replace("TYPE", "component"), (m) =>
            {
                (string type, string name, string[] options) tag = SplitMatch(m);

                return ProcessContentString(key,
                    p,
                    Component(
                        p.App?.Components?.FirstOrDefault(c => c.Name.Equals(tag.name, StringComparison.CurrentCultureIgnoreCase)) ?? ObjectCache.Get<Component>($"component|{tag.name}"),
                        p,
                        tag,
                        replacements),
                    replacements);
            });
        }

        private static void Content(string key, StringBuilder source, PageRenderParams prp, IEnumerable<Replacement> replacements)
        {
            source.RegexReplace(tag.Replace("TYPE", "content"), (m) =>
            {
                (string type, string name, string[] options) tag = SplitMatch(m);
                string content = Content(prp.Page.ResourceKey, prp.Page.ContentForCulture(tag.name, prp.Culture), prp, tag, replacements);

                if (prp.Edit)
                    return content;
                else
                    return ProcessContentString(key, prp, content, replacements);
            });
        }


        /// <summary>
        /// Replaces references to scripts with their content
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        static void Script(string key, StringBuilder source, RenderParams p, IEnumerable<Replacement> replacements)
        {
            source.RegexReplace("\\[script\\[[A-Za-z\\d_/. \\-]*\\]\\]", (m) =>
            {
                string name = m.Value
                    .Replace("[script[", "")
                    .Replace("]]", "")
                    .ToLower();

                Script script = ObjectCache.Get<Script>($"script|{name.ToLower()}");

                if (script != null)
                {
                    Script appScript = p?.App?.Scripts?
                        .FirstOrDefault(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

                    return ProcessContentString(key, p, appScript?.Content ?? script.Content, replacements);
                }

                return string.Empty;
            });
        }

        /// <summary>
        /// Replaces references to DMS file paths with their file content
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        static void DMS(string key, StringBuilder source, ComponentRenderParams p, IEnumerable<Replacement> replacements)
        {
            source.RegexReplace("\\[dms\\[[A-Za-z\\d_/. \\-]*\\]\\]", (m) =>
            {
                string path = m.Value
                    .Replace("[dms[", "")
                    .Replace("]]", "")
                    .ToLower();

                File file = p.Db.GetAll<File>(false)
                    .FirstOrDefault(f => f.Folder.AppId == p.App.Id && f.Path == path);

                if (file != null)
                {
                    FileContent content = p.Db.GetAll<FileContent>(false)
                        .Where(f => f.FileId == file.Id)
                        .OrderByDescending(f => f.Version)
                        .First();

                    return content.RawData.Length != 0 
                        ? ProcessContentString(key, p, Encoding.UTF8.GetString(content.RawData), replacements) 
                        : string.Empty;
                }

                return string.Empty;
            });
        }

        /// <summary>
        /// Replaces references to content blocks for a page in a content string
        /// </summary>
        /// <param name="content"></param>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        static string Content(string key, Content content, PageRenderParams p, (string type, string name, string[] options) tag, IEnumerable<Replacement> replacements)
        {
            var contentEditable = p.Edit
                ? "contenteditable"
                : string.Empty;

            var optionalClass = string.Join(" ", tag.options
                .Where(o => o.StartsWith("class="))
                .Select(c => c.Replace("class=", "")));

            return content != null
                ? $@"<section name='{content.Name}' class='content {optionalClass}' data-id='{content.Id}' {contentEditable} {string.Join(" ", tag.options.Where(o => !o.StartsWith("class=")))}>
                        {(p.Edit ? content.Html : ProcessContentString(key, p, content.Html, replacements))}
                    </section>"
                : $"[[Missing Content:{tag.name}]]";
        }



        /// <summary>
        /// Replaces reference to components in a content string
        /// </summary>
        /// <param name="component"></param>
        /// <param name="p"></param>
        /// <param name="name"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        static string Component(Component component, RenderParams p, (string type, string name, string[] options) tag, IEnumerable<Replacement> replacements)
        {
            if (p is PageRenderParams prp && prp.Edit)
                return $"[component[{tag.name}]{tag.options}]";

            if (component == null)
                return $"[[Missing Component:{tag.name}]]";

            var optionalClass = string.Join(" ", tag.options
                .Where(o => o.StartsWith("class="))
                .Select(c => c.Replace("class=", "")));

            var result = component != null
                ? $@"<section name='{component.Name}' class='component {optionalClass}' data-id='{component.Id}' data-resource-key='{component.ResourceKey}' {string.Join(" ", tag.options.Where(o => !o.StartsWith("class=")))}>
                        {ProcessContentString(component.ResourceKey, p, component.Content, replacements)}
                        <script type='text/javascript' defer async>{ProcessContentString(component.ResourceKey, p, component.Script, replacements)}</script>
                    </section>"
                : $"[[Missing Component:{tag.name}]]";

            return ProcessContentString(component.ResourceKey, p, result, replacements);
        }

        /// <summary>
        /// Replaces references to resources in a content string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="p"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        static void Resource(string key, StringBuilder source, RenderParams p, IEnumerable<Replacement> replacements)
        {
            if (p is PageRenderParams prp && prp.Edit)
                return;

            List<Resource> known = [];
            List<string> namesInKey = [];

            // scrape the names in to the list above
            source.RegexMatch(tag.Replace("TYPE", "resource_displayname"), (m) => namesInKey.Add(m.TagName()));
            source.RegexMatch(tag.Replace("TYPE", "resource_shortdisplayname"), (m) => namesInKey.Add(m.TagName()));
            source.RegexMatch(tag.Replace("TYPE", "resource_description"), (m) => namesInKey.Add(m.TagName()));

            if (namesInKey.Count == 0)
                return;

            known.AddRange(p.App.Resources?.SectionForCulture(key, p.Culture ?? string.Empty).ToList() ?? []);

            string lowerKey = key.ToLowerInvariant();
            string lowerCulture = p.Culture.ToLowerInvariant();

            foreach (var resourceName in namesInKey)
            {
                Resource matchedResource = FindResourceInCache(lowerKey, resourceName.ToLowerInvariant(), lowerCulture);

                if (matchedResource != null)
                    known.Add(matchedResource);
            }

            // do the replacement work
            source.RegexReplace(tag.Replace("TYPE", "resource_displayname"), 
                (m) => ProcessContentString(
                    key, 
                    p, 
                    known.FirstOrDefault(r => 
                        r.Name.Equals(m.TagName(), 
                        StringComparison.CurrentCultureIgnoreCase))?.DisplayName ?? m.TagName().ToLower(), 
                    replacements));

            source.RegexReplace(tag.Replace("TYPE", "resource_shortdisplayname"), 
                (m) => ProcessContentString(
                    key, 
                    p, 
                    known.FirstOrDefault(r => 
                        r.Name.Equals(m.TagName(), 
                        StringComparison.CurrentCultureIgnoreCase))?.ShortDisplayName ?? m.TagName().ToLower(), 
                    replacements));

            source.RegexReplace(tag.Replace("TYPE", "resource_description"), 
                (m) => ProcessContentString(
                    key, 
                    p, 
                    known.FirstOrDefault(r => 
                        r.Name.Equals(m.TagName(), 
                        StringComparison.CurrentCultureIgnoreCase))?.Description ?? m.TagName().ToLower(), 
                    replacements));

        }

        private static Resource FindResourceInCache(string key, string name, string culture)
        {
            var existsForCurrentCulture = ObjectCache.Get<Resource>($"resource|{key}-{name}-{culture}");
            if (existsForCurrentCulture != null)
                return existsForCurrentCulture;

            if (culture.Contains('-'))
            {
                string primitiveCulture = culture.Split("-")[0];
                var existsForPrimitiveCulture = ObjectCache.Get<Resource>($"resource|{key}-{name}-{primitiveCulture}");

                if (existsForPrimitiveCulture != null)
                    return existsForPrimitiveCulture;
            }

            return ObjectCache.Get<Resource>($"resource|{key}-{name}-{string.Empty}");
        }

        static void Meta(StringBuilder source, string culture) =>
            source.RegexReplace(tag.Replace("TYPE", "meta"), (m) =>
            {
                string start = m.Value[6..];
                int end = start.IndexOf(']');
                string name = start[..end].ToLowerInvariant();
                return MetaCache.Get(name, culture);
            });

        static IEnumerable<Replacement> BuildThemeReplacements<T>(T model, string prefix = "")
        {
            if (model.GetType().GetInterface("IDynamicMetaObjectProvider") != null && model is not JObject)
                return BuildDynamicThemeReplacements(model, prefix);

            if (model is JObject)
                return BuildJObjectThemeReplacements(model, prefix);

            if (model is string)
                return 
                [
                    new Replacement($"[theme[{prefix}]]", 
                    model.ToString())
                ];

            if (model is not IEnumerable)
                return BuildIEnumerableThemeReplacements(model, prefix);

            return BuildObjectThemeReplacements(model, prefix);
        }

        private static List<Replacement> BuildObjectThemeReplacements<T>(T model, string prefix)
        {
            string bindingExpression = prefix ?? string.Empty;
            List<Replacement> result = [];
            int i = 0;

            foreach (object item in ((IEnumerable)model))
            {
                string itemPrefix = bindingExpression + $"[{i}]";
                result.AddRange(BuildThemeReplacements(item, itemPrefix));
                i++;
            }

            string lengthBinding = bindingExpression.Length == 0 
                ? "Length" 
                : bindingExpression + ".Length";

            result.Add(new Replacement($"[theme[{lengthBinding}]]", i.ToString()));

            return result;
        }

        private static IEnumerable<Replacement> BuildIEnumerableThemeReplacements<T>(T model, string prefix)
        {
            return model.GetType()
                .GetProperties()
                .SelectMany(p =>
                {
                    object v = p.GetValue(model);

                    string bindingExpression = prefix.Length > 0 
                        ? prefix + "." + p.Name 
                        : p.Name;

                    if (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                        return 
                        [
                            new Replacement($"[theme[{prefix}]]", model?.ToString() ?? string.Empty), 
                            new Replacement($"[theme[{bindingExpression}]]", v?.ToString() ?? string.Empty)
                        ];
                    else if (v != null)
                        return BuildThemeReplacements(v, $"{bindingExpression}");
                    else
                        return [];
                })
                .Where(i => i.Old != null && i.New != null);
        }

        private static IEnumerable<Replacement> BuildJObjectThemeReplacements<T>(T model, string prefix)
        {
            IEnumerable<KeyValuePair<string, JToken>> values = ((IEnumerable<KeyValuePair<string, JToken>>)model);
            return values.SelectMany(t =>
            {
                string bindingExpression = prefix.Length > 0 
                    ? prefix + "." + t.Key 
                    : t.Key;

                if (t.Value.GetType() == typeof(JValue))
                    return [new Replacement($"[theme[{bindingExpression}]]", t.Value?.ToString() ?? string.Empty)];
                else if (t.Value != null)
                    return BuildThemeReplacements(t.Value, $"{bindingExpression}");

                return [];
            });
        }

        private static IEnumerable<Replacement> BuildDynamicThemeReplacements<T>(T model, string prefix)
        {
            IDictionary<string, object> dynamicModel = (IDictionary<string, object>)model;

            return dynamicModel.Keys.SelectMany(key =>
            {
                string bindingExpression = prefix.Length > 0 
                    ? prefix + "." + key 
                    : key;

                List<Replacement> results = 
                [
                    new Replacement($"[theme[{bindingExpression}]]", 
                    dynamicModel[key]?.ToString() ?? string.Empty)
                ];

                if (dynamicModel[key] != null && !dynamicModel[key].GetType().IsValueType)
                    results.AddRange(BuildThemeReplacements(dynamicModel[key], bindingExpression));

                return results;
            });
        }
    }
}