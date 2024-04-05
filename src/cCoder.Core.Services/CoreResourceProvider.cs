using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace cCoder.Core
{
    /// <summary>
    /// For use with applications and services that have direct access to the core db
    /// </summary>
    public class CoreResourceProvider : IResourceProvider
    {
        private readonly IResourceService service;

        public CoreResourceProvider(IResourceService resourceService) => service = resourceService;

        public Resource GetResource(string key, string culture)
        {
            string name = key.Split('.').Last();
            string formattedName = Regex.Replace(name.Replace("Id", ""), @"(?<!_)([A-Z])", " $1");

            return new Resource
            {
                Culture = string.Empty,
                Name = name,
                Key = key,
                DisplayName = formattedName,
                ShortDisplayName = formattedName,
                Description = formattedName
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                service.Dispose();
            }
        }
    }
}
