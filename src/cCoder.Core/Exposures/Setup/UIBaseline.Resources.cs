// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Exposures.Setup;

public static partial class UIBaseline
{
    private static Package CreateResourcesPackage() =>
        new()
        {
        Name = "Core Review Resources",
        Category = "CoreReview",
        Description = "Unresolved Core resource baseline items.",
        SourceApi = "https://ccoder.co.uk/Api/",
        Items =
        [
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "add",
  "DisplayName": "Add",
  "ShortDisplayName": "Add",
  "Description": "Add",
  "LastUpdated": "2022-03-18T10:41:54.1909813+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "created",
  "DisplayName": "Created",
  "ShortDisplayName": "Created",
  "Description": "Created",
  "LastUpdated": "2022-03-18T10:41:54.1909863+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "lastupdated",
  "DisplayName": "Last Updated",
  "ShortDisplayName": "Last Updated",
  "Description": "Last Updated",
  "LastUpdated": "2022-03-18T10:41:54.1909914+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "state",
  "DisplayName": "State",
  "ShortDisplayName": "State",
  "Description": "State",
  "LastUpdated": "2022-03-18T10:41:54.190998+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "clientname",
  "DisplayName": "Client Name",
  "ShortDisplayName": "Client Name",
  "Description": "Client Name",
  "LastUpdated": "2022-03-18T10:41:54.1910031+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "close",
  "DisplayName": "Close",
  "ShortDisplayName": "Close",
  "Description": "Close",
  "LastUpdated": "2022-03-18T10:41:54.1910081+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "name",
  "DisplayName": "Name",
  "ShortDisplayName": "Name",
  "Description": "Name",
  "LastUpdated": "2022-03-18T10:41:54.191013+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "view",
  "DisplayName": "View",
  "ShortDisplayName": "View",
  "Description": "View",
  "LastUpdated": "2022-03-18T10:41:54.1910181+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "remove",
  "DisplayName": "Remove",
  "ShortDisplayName": "Remove",
  "Description": "Remove",
  "LastUpdated": "2022-03-18T10:41:54.1910231+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "scanclient",
  "DisplayName": "Scan Client",
  "ShortDisplayName": "Scan Client",
  "Description": "Scan Client",
  "LastUpdated": "2022-03-18T10:41:54.1910281+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "save",
  "DisplayName": "Save",
  "ShortDisplayName": "Save",
  "Description": "Save",
  "LastUpdated": "2022-03-18T10:41:54.1910332+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "editfiles",
  "DisplayName": "Edit Files",
  "ShortDisplayName": "Edit Files",
  "Description": "Edit Files",
  "LastUpdated": "2022-03-18T10:41:54.1910398+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "apps",
  "DisplayName": "Apps",
  "ShortDisplayName": "Apps",
  "Description": "Apps",
  "LastUpdated": "2022-03-18T10:41:54.1910449+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "environment",
  "DisplayName": "Environment",
  "ShortDisplayName": "Environment",
  "Description": "Environment",
  "LastUpdated": "2022-03-18T10:41:54.1910499+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "domain",
  "DisplayName": "Domain",
  "ShortDisplayName": "Domain",
  "Description": "Domain",
  "LastUpdated": "2022-03-18T10:41:54.191055+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "apptype",
  "DisplayName": "App Type",
  "ShortDisplayName": "App Type",
  "Description": "App Type",
  "LastUpdated": "2022-03-18T10:41:54.1910601+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "content",
  "DisplayName": "Content",
  "ShortDisplayName": "Content",
  "Description": "Content",
  "LastUpdated": "2022-03-18T10:41:54.1910651+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "themes",
  "DisplayName": "Themes",
  "ShortDisplayName": "Themes",
  "Description": "Themes",
  "LastUpdated": "2022-03-18T10:41:54.1910702+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "contactinfo",
  "DisplayName": "Contact Info",
  "ShortDisplayName": "Contact Info",
  "Description": "Contact Info",
  "LastUpdated": "2022-03-18T10:41:54.191077+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "history",
  "DisplayName": "History",
  "ShortDisplayName": "History",
  "Description": "History",
  "LastUpdated": "2022-03-18T10:41:54.1910821+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "companyname",
  "DisplayName": "Company Name",
  "ShortDisplayName": "Company Name",
  "Description": "Company Name",
  "LastUpdated": "2022-03-18T10:41:54.1910872+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "technicalphone",
  "DisplayName": "Technical Phone",
  "ShortDisplayName": "Technical Phone",
  "Description": "Technical Phone",
  "LastUpdated": "2022-03-18T10:41:54.1910922+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "technicalemail",
  "DisplayName": "Technical Email",
  "ShortDisplayName": "Technical Email",
  "Description": "Technical Email",
  "LastUpdated": "2022-03-18T10:41:54.1910972+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "salesphone",
  "DisplayName": "Sales Phone",
  "ShortDisplayName": "Sales Phone",
  "Description": "Sales Phone",
  "LastUpdated": "2022-03-18T10:41:54.1911023+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "salesemail",
  "DisplayName": "Sales Email",
  "ShortDisplayName": "Sales Email",
  "Description": "Sales Email",
  "LastUpdated": "2022-03-18T10:41:54.1911073+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "projectmanagerphone",
  "DisplayName": "Project Manager Phone",
  "ShortDisplayName": "Project Manager Phone",
  "Description": "Project Manager Phone",
  "LastUpdated": "2022-03-18T10:41:54.1911137+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "projectmanageremail",
  "DisplayName": "Project Manager Email",
  "ShortDisplayName": "Project Manager Email",
  "Description": "Project Manager Email",
  "LastUpdated": "2022-03-18T10:41:54.1911189+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "buyerportal",
  "DisplayName": "Buyer Portal",
  "ShortDisplayName": "Buyer Portal",
  "Description": "Buyer Portal",
  "LastUpdated": "2022-03-18T10:41:54.191124+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "einvoicingportal",
  "DisplayName": "E-Invoicing Portal",
  "ShortDisplayName": "E-Invoicing Portal",
  "Description": "E-Invoicing Portal",
  "LastUpdated": "2022-03-18T10:41:54.191129+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "fundmanagementsystem",
  "DisplayName": "Fund Management System",
  "ShortDisplayName": "Fund Management System",
  "Description": "Fund Management System",
  "LastUpdated": "2022-03-18T10:41:54.191134+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "addapp",
  "DisplayName": "Add App",
  "ShortDisplayName": "Add App",
  "Description": "Add App",
  "LastUpdated": "2022-03-18T10:41:54.1911391+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "visit",
  "DisplayName": "Visit",
  "ShortDisplayName": "Visit",
  "Description": "Visit",
  "LastUpdated": "2022-03-18T10:41:54.1911442+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "applytheming",
  "DisplayName": "Apply Theming",
  "ShortDisplayName": "Apply Theming",
  "Description": "Apply Theming",
  "LastUpdated": "2022-03-18T10:41:54.1911492+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "delete",
  "DisplayName": "Delete",
  "ShortDisplayName": "Delete",
  "Description": "Delete",
  "LastUpdated": "2022-03-18T10:41:54.1911558+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "saved",
  "DisplayName": "Saved",
  "ShortDisplayName": "Saved",
  "Description": "Saved",
  "LastUpdated": "2022-03-18T10:41:54.1911608+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "slideshow",
  "DisplayName": "Slideshow",
  "ShortDisplayName": "Slideshow",
  "Description": "Slideshow",
  "LastUpdated": "2022-03-18T10:41:54.1911658+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "flags",
  "DisplayName": "Flags",
  "ShortDisplayName": "Flags",
  "Description": "Flags",
  "LastUpdated": "2022-03-18T10:41:54.1911709+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "addtheme",
  "DisplayName": "Add Theme",
  "ShortDisplayName": "Add Theme",
  "Description": "Add Theme",
  "LastUpdated": "2022-03-18T10:41:54.1911759+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "edit",
  "DisplayName": "Edit",
  "ShortDisplayName": "Edit",
  "Description": "Edit",
  "LastUpdated": "2022-03-18T10:41:54.1911809+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "edittheme",
  "DisplayName": "Edit Theme",
  "ShortDisplayName": "Edit Theme",
  "Description": "Edit Theme",
  "LastUpdated": "2022-03-18T10:41:54.1911859+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "description",
  "DisplayName": "Description",
  "ShortDisplayName": "Description",
  "Description": "Description",
  "LastUpdated": "2022-08-15T14:05:05.1355053+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "clienttype",
  "DisplayName": "Client Type",
  "ShortDisplayName": "Client Type",
  "Description": "Client Type",
  "LastUpdated": "2022-08-15T14:05:51.358952+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "buyer",
  "DisplayName": "Buyer",
  "ShortDisplayName": "Buyer",
  "Description": "Buyer",
  "LastUpdated": "2022-08-15T14:06:11.8953815+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "supplier",
  "DisplayName": "Supplier",
  "ShortDisplayName": "Supplier",
  "Description": "Supplier",
  "LastUpdated": "2022-08-15T14:06:52.5214722+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "funder",
  "DisplayName": "Funder",
  "ShortDisplayName": "Funder",
  "Description": "Funder",
  "LastUpdated": "2022-08-15T14:07:04.755367+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "theme",
  "DisplayName": "Theme",
  "ShortDisplayName": "Theme",
  "Description": "Theme",
  "LastUpdated": "2022-08-16T22:11:45.58726+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "primary",
  "DisplayName": "Primary",
  "ShortDisplayName": "Primary",
  "Description": "Primary",
  "LastUpdated": "2022-08-17T11:23:03.9979433+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "secondary",
  "DisplayName": "Secondary",
  "ShortDisplayName": "Secondary",
  "Description": "Secondary",
  "LastUpdated": "2022-08-17T11:23:27.3451889+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "uploadcompanylogo",
  "DisplayName": "Upload Company Logo",
  "ShortDisplayName": "Upload Company Logo",
  "Description": "This will appear on the top left of the portal",
  "LastUpdated": "2022-08-17T11:24:07.4045406+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "uploadbrandlogo",
  "DisplayName": "Upload Brand Logo",
  "ShortDisplayName": "Upload Brand Logo",
  "Description": "This will appear on the top right of your portal",
  "LastUpdated": "2022-08-17T11:24:47.6756602+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "uploadslideshowimages",
  "DisplayName": "Upload Slideshow Images",
  "ShortDisplayName": "Upload Slideshow Images",
  "Description": "Upload Slideshow Images",
  "LastUpdated": "2022-08-17T11:25:41.5674445+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "details",
  "DisplayName": "Details",
  "ShortDisplayName": "Details",
  "Description": "Details",
  "LastUpdated": "2022-08-25T13:12:03.339284+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "vatreference",
  "DisplayName": "VAT Reference",
  "ShortDisplayName": "VAT Ref",
  "Description": "VAT Reference",
  "LastUpdated": "2022-09-27T12:14:46.5800825+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "internalreference",
  "DisplayName": "Internal Reference",
  "ShortDisplayName": "Internal Ref",
  "Description": "Internal Reference",
  "LastUpdated": "2022-09-27T12:15:11.9576322+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "country",
  "DisplayName": "Country",
  "ShortDisplayName": "Country",
  "Description": "Country",
  "LastUpdated": "2022-09-27T12:15:28.2287827+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "id",
  "DisplayName": "Id",
  "ShortDisplayName": "Id",
  "Description": "Id",
  "LastUpdated": "2023-01-04T12:55:43.693014+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "create",
  "DisplayName": "Create",
  "ShortDisplayName": "Create",
  "Description": "Create",
  "LastUpdated": "2023-01-04T13:45:02.6684616+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "fr-FR",
  "Key": "CRM",
  "Name": "create",
  "DisplayName": "Créer",
  "ShortDisplayName": "Créer",
  "Description": "Créer",
  "LastUpdated": "2023-01-04T13:45:40.2842417+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "fr-FR",
  "Key": "CRM",
  "Name": "name",
  "DisplayName": "Nom",
  "ShortDisplayName": "Nom",
  "Description": "Nom",
  "LastUpdated": "2023-01-04T13:46:33.7231641+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "fr-FR",
  "Key": "CRM",
  "Name": "add",
  "DisplayName": "Ajouter",
  "ShortDisplayName": "Ajouter",
  "Description": "Ajouter",
  "LastUpdated": "2023-01-04T13:47:10.5301625+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "fr-FR",
  "Key": "CRM",
  "Name": "country",
  "DisplayName": "Pays",
  "ShortDisplayName": "Pays",
  "Description": "Pays",
  "LastUpdated": "2023-01-04T13:47:39.2732407+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "references",
  "DisplayName": "References",
  "ShortDisplayName": "References",
  "Description": "References",
  "LastUpdated": "2023-01-04T13:48:11.2675817+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "fr-FR",
  "Key": "CRM",
  "Name": "references",
  "DisplayName": "Références",
  "ShortDisplayName": "Références",
  "Description": "Références",
  "LastUpdated": "2023-01-04T13:48:23.8638027+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "newtenant",
  "DisplayName": "New Tenant",
  "ShortDisplayName": "New Tenant",
  "Description": "New Tenant",
  "LastUpdated": "2023-01-04T13:50:06.7855372+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "fr-FR",
  "Key": "CRM",
  "Name": "newtenant",
  "DisplayName": "Nouvelle Cliente",
  "ShortDisplayName": "Nouvelle Cliente",
  "Description": "Nouvelle Cliente",
  "LastUpdated": "2023-01-04T13:50:32.6684242+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "systemid",
  "DisplayName": "System Id",
  "ShortDisplayName": "System Id",
  "Description": "System Id",
  "LastUpdated": "2023-01-04T14:32:02.4805633+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "fr-FR",
  "Key": "CRM",
  "Name": "systemid",
  "DisplayName": "ID système",
  "ShortDisplayName": "ID système",
  "Description": "ID système",
  "LastUpdated": "2023-01-04T14:32:24.1723301+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "value",
  "DisplayName": "Value",
  "ShortDisplayName": "Value",
  "Description": "Value",
  "LastUpdated": "2023-01-04T14:33:02.577155+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "fr-FR",
  "Key": "CRM",
  "Name": "value",
  "DisplayName": "Référence",
  "ShortDisplayName": "Référence",
  "Description": "Référence",
  "LastUpdated": "2023-01-04T14:33:12.7365187+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "brandlogo",
  "DisplayName": "Brand Logo",
  "ShortDisplayName": "Brand Logo",
  "Description": "Brand Logo",
  "LastUpdated": "2023-03-13T15:28:16.2486039+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "projectlogo",
  "DisplayName": "Project Logo",
  "ShortDisplayName": "Project Logo",
  "Description": "Project Logo",
  "LastUpdated": "2023-03-13T15:29:51.8342716+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "animation",
  "DisplayName": "Animation",
  "ShortDisplayName": "Animation",
  "Description": "Animation",
  "LastUpdated": "2023-03-13T15:31:40.4033275+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "added",
  "DisplayName": "Added",
  "ShortDisplayName": "Added",
  "Description": "Added",
  "LastUpdated": "2023-05-15T12:06:17.5231745+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "sourceappid",
  "DisplayName": "Source App",
  "ShortDisplayName": "Source App",
  "Description": "Source App`",
  "LastUpdated": "2023-05-15T12:09:58.7914935+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "password",
  "DisplayName": "Password",
  "ShortDisplayName": "Password",
  "Description": "Password",
  "LastUpdated": "2023-05-15T12:10:21.2984173+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "type",
  "DisplayName": "Type",
  "ShortDisplayName": "Type",
  "Description": "Type",
  "LastUpdated": "2023-08-01T17:33:58.2959184+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "creatingapp",
  "DisplayName": "Creating App",
  "ShortDisplayName": "Creating App",
  "Description": "Creating App",
  "LastUpdated": "2023-09-26T16:06:13.2155438+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "appdeleted",
  "DisplayName": "App Deleted",
  "ShortDisplayName": "App Deleted",
  "Description": "App Deleted",
  "LastUpdated": "2023-09-26T16:06:35.8297146+01:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "adminusercreated",
  "DisplayName": "Admin User Created",
  "ShortDisplayName": "Admin User Created",
  "Description": "Admin User Created",
  "LastUpdated": "2023-11-15T14:26:07.2981944+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "tenantdeleted",
  "DisplayName": "Tenant Deleted",
  "ShortDisplayName": "Tenant Deleted",
  "Description": "Tenant Deleted",
  "LastUpdated": "2023-11-15T14:27:21.8913123+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "chartcolour1",
  "DisplayName": "Chart Colour 1",
  "ShortDisplayName": "Chart Colour 1",
  "Description": "Chart Colour 1",
  "LastUpdated": "2023-11-23T12:57:39.9746715+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "chartcolour2",
  "DisplayName": "Chart Colour 2",
  "ShortDisplayName": "Chart Colour 2",
  "Description": "Chart Colour 2",
  "LastUpdated": "2023-11-23T13:00:01.0325603+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "chartcolour3",
  "DisplayName": "Chart Colour 3",
  "ShortDisplayName": "Chart Colour 3",
  "Description": "Chart Colour 3",
  "LastUpdated": "2023-11-23T13:00:15.7001223+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "chartcolour4",
  "DisplayName": "Chart Colour 4",
  "ShortDisplayName": "Chart Colour 4",
  "Description": "Chart Colour 4",
  "LastUpdated": "2023-11-23T13:00:29.9937071+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "uploadbackgroundanimation",
  "DisplayName": "Upload Background Animation",
  "ShortDisplayName": "Upload Background Animation",
  "Description": "Upload Background Animation",
  "LastUpdated": "2023-11-24T12:40:11.8237361+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "uploadprojectlogo",
  "DisplayName": "Upload Project Logo",
  "ShortDisplayName": "Upload Project Logo",
  "Description": "Upload Project Logo",
  "LastUpdated": "2023-11-24T12:40:32.4944444+00:00"
}
"""
            },
            new PackageItem
            {
                Type = "Core/Resource",
                Data = """
{
  "Culture": "",
  "Key": "CRM",
  "Name": "noSourceApp",
  "DisplayName": "No Source App",
  "ShortDisplayName": "No Source App",
  "Description": "",
  "LastUpdated": "2025-12-04T12:58:47.4016033+00:00"
}
"""
            }
        ]
        };
}