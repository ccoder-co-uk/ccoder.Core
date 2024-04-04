using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.Data
{
    public static class Cultures
    {
        public static Culture[] Known => new[] {
            new Culture { Id = "", Name = "Default" },
            new Culture { Id = "en-GB", Name = "English (British)" },
            new Culture { Id = "fr-FR", Name = "French (France)" }
        };
    }
}