using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Exposures.Setup;

public static partial class UIBaseline
{
    static Package Pages => new()
    {
        Name = "Core Review Pages",
        Category = "CoreReview",
        Description = "Unresolved Core page baseline items.",
        SourceApi = "https://ccoder.co.uk/Api/",
        Items =
        [
            new PackageItem
            {
                Type = "Core/Page",
                Data = """
{
  "Path": "Clients",
  "Name": "Clients",
  "ResourceKey": "",
  "ShowOnMenus": true,
  "Order": 6,
  "LastUpdated": "2024-04-04T15:47:12.0886434+01:00",
  "Layout": "Default",
  "Contents": [
    {
      "CultureId": "",
      "Name": "body",
      "Html": "[component[TenantManagement]]"
    }
  ],
  "PageInfo": [
    {
      "CultureId": "",
      "Description": "Clients",
      "Keywords": "Clients",
      "Title": "Clients"
    }
  ]
}
"""
            },
            new PackageItem
            {
                Type = "Core/Page",
                Data = """
{
  "Path": "Clients/Client",
  "Name": "Client",
  "ResourceKey": "",
  "ShowOnMenus": false,
  "Order": 11,
  "LastUpdated": "2024-04-04T15:47:12.1042793+01:00",
  "Layout": "Default",
  "Contents": [
    {
      "CultureId": "",
      "Name": "body",
      "Html": "[component[Client]]"
    }
  ],
  "PageInfo": [
    {
      "CultureId": "",
      "Description": "Client",
      "Keywords": "Client",
      "Title": "Client"
    }
  ]
}
"""
            }
        ]
    };
}