using System.Collections;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using cCoder.Data.Models.CMS;


namespace cCoder.Core.Exposures.Formatters;

internal class FormatterExcelFileBuilder(string culture, IEnumerable<Resource> resources)
{
    private readonly string culture = culture;
    private readonly IEnumerable<Resource> resources = resources ?? [];

    public Stream BuildFor(object data)
    {
        MemoryStream result = new();

        using (ZipArchive excelFile = new(result, ZipArchiveMode.Create, true))
        {
            AddRels(excelFile);
            AddDocProps(excelFile);
            AddXl(excelFile);

            if (data is IEnumerable enumerable)
            {
                InsertData(excelFile, enumerable.Cast<object>());
            }
            else
            {
                InsertData(excelFile, [data]);
            }

            excelFile.AddTextFile(
                "[Content_Types].xml",
                @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<Types xmlns='http://schemas.openxmlformats.org/package/2006/content-types'><Default Extension='rels' ContentType='application/vnd.openxmlformats-package.relationships+xml'/><Default Extension='xml' ContentType='application/xml'/><Override PartName='/xl/workbook.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml'/><Override PartName='/xl/worksheets/sheet1.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml'/><Override PartName='/xl/theme/theme1.xml' ContentType='application/vnd.openxmlformats-officedocument.theme+xml'/><Override PartName='/xl/styles.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml'/><Override PartName='/docProps/core.xml' ContentType='application/vnd.openxmlformats-package.core-properties+xml'/><Override PartName='/docProps/app.xml' ContentType='application/vnd.openxmlformats-officedocument.extended-properties+xml'/></Types>".Replace(
                    "'",
                    "\""
                )
            );
        }

        _ = result.Seek(0, SeekOrigin.Begin);
        return result;
    }

    private static void AddRels(ZipArchive excelFile)
    {
        _ = excelFile.CreateEntry("_rels/", CompressionLevel.Optimal);
        excelFile.AddTextFile(
            "_rels/.rels",
            @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='rId3' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties' Target='docProps/app.xml'/><Relationship Id='rId2' Type='http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties' Target='docProps/core.xml'/><Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument' Target='xl/workbook.xml'/></Relationships>".Replace(
                "'",
                "\""
            )
        );
    }

    private static void AddDocProps(ZipArchive excelFile)
    {
        _ = excelFile.CreateEntry("docProps/", CompressionLevel.Optimal);
        excelFile.AddTextFile(
            "docProps/app.xml",
            @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<Properties xmlns='http://schemas.openxmlformats.org/officeDocument/2006/extended-properties' xmlns:vt='http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes'><Application>Microsoft Excel</Application><DocSecurity>0</DocSecurity><ScaleCrop>false</ScaleCrop><HeadingPairs><vt:vector size='2' baseType='variant'><vt:variant><vt:lpstr>Worksheets</vt:lpstr></vt:variant><vt:variant><vt:i4>1</vt:i4></vt:variant></vt:vector></HeadingPairs><TitlesOfParts><vt:vector size='1' baseType='lpstr'><vt:lpstr>Sheet1</vt:lpstr></vt:vector></TitlesOfParts><Company></Company><LinksUpToDate>false</LinksUpToDate><SharedDoc>false</SharedDoc><HyperlinksChanged>false</HyperlinksChanged><AppVersion>16.0300</AppVersion></Properties>".Replace(
                "'",
                "\""
            )
        );
        excelFile.AddTextFile(
            "docProps/core.xml",
            $@"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<cp:coreProperties xmlns:cp='http://schemas.openxmlformats.org/package/2006/metadata/core-properties' xmlns:dc='http://purl.org/dc/elements/1.1/' xmlns:dcterms='http://purl.org/dc/terms/' xmlns:dcmitype='http://purl.org/dc/dcmitype/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><dc:creator>Paul Ward</dc:creator><cp:lastModifiedBy>Paul Ward</cp:lastModifiedBy><dcterms:created xsi:type='dcterms:W3CDTF'>2020-11-01T18:37:26Z</dcterms:created><dcterms:modified xsi:type='dcterms:W3CDTF'>2020-11-01T18:37:34Z</dcterms:modified></cp:coreProperties>".Replace(
                "'",
                "\""
            )
        );
    }

    private void AddXl(ZipArchive excelFile)
    {
        _ = excelFile.CreateEntry("xl/", CompressionLevel.Optimal);
        AddXlFiles(excelFile);

        _ = excelFile.CreateEntry("xl/_rels/", CompressionLevel.Optimal);
        AddXlRelsFiles(excelFile);

        _ = excelFile.CreateEntry("xl/theme/", CompressionLevel.Optimal);
        AddXlThemeFiles(excelFile);

        _ = excelFile.CreateEntry("xl/worksheets/", CompressionLevel.Optimal);
    }

    private void AddXlFiles(ZipArchive excelFile)
    {
        excelFile.AddTextFile("xl/styles.xml", BuildStyles());

        excelFile.AddTextFile(
            "xl/workbook.xml",
            @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<workbook xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships' xmlns:mc='http://schemas.openxmlformats.org/markup-compatibility/2006' mc:Ignorable='x15 xr xr6 xr10 xr2' xmlns:x15='http://schemas.microsoft.com/office/spreadsheetml/2010/11/main' xmlns:xr='http://schemas.microsoft.com/office/spreadsheetml/2014/revision' xmlns:xr6='http://schemas.microsoft.com/office/spreadsheetml/2016/revision6' xmlns:xr10='http://schemas.microsoft.com/office/spreadsheetml/2016/revision10' xmlns:xr2='http://schemas.microsoft.com/office/spreadsheetml/2015/revision2'><fileVersion appName='xl' lastEdited='7' lowestEdited='7' rupBuild='23328'/><workbookPr defaultThemeVersion='166925'/><mc:AlternateContent xmlns:mc='http://schemas.openxmlformats.org/markup-compatibility/2006'><mc:Choice Requires='x15'><x15ac:absPath url='C:\Users\ward_\OneDrive\Desktop\' xmlns:x15ac='http://schemas.microsoft.com/office/spreadsheetml/2010/11/ac'/></mc:Choice></mc:AlternateContent><xr:revisionPtr revIDLastSave='0' documentId='8_{70401B0E-5973-46F7-AC92-E61D8C0489C1}' xr6:coauthVersionLast='45' xr6:coauthVersionMax='45' xr10:uidLastSave='{00000000-0000-0000-0000-000000000000}'/><bookViews><workbookView xWindow='11850' yWindow='7110' windowWidth='32085' windowHeight='18540' xr2:uid='{DAD8344E-BCA9-4DA3-9E6D-31F5FA856CCC}'/></bookViews><sheets><sheet name='Sheet1' sheetId='1' r:id='rId1'/></sheets><calcPr calcId='191029'/><extLst><ext uri='{140A7094-0E35-4892-8432-C4D2E57EDEB5}' xmlns:x15='http://schemas.microsoft.com/office/spreadsheetml/2010/11/main'><x15:workbookPr chartTrackingRefBase='1'/></ext></extLst></workbook>"
        );
    }

    private static void AddXlRelsFiles(ZipArchive excelFile) =>
        excelFile.AddTextFile(
            "xl/_rels/workbook.xml.rels",
            @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='rId3' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles' Target='styles.xml'/><Relationship Id='rId2' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme' Target='theme/theme1.xml'/><Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet' Target='worksheets/sheet1.xml'/></Relationships>".Replace(
                "'",
                "\""
            )
        );

    private static void AddXlThemeFiles(ZipArchive excelFile) =>
        excelFile.AddTextFile(
            "xl/theme/theme1.xml",
            @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<a:theme xmlns:a='http://schemas.openxmlformats.org/drawingml/2006/main' name='Office Theme'><a:themeElements><a:clrScheme name='Office'><a:dk1><a:sysClr val='windowText' lastClr='000000'/></a:dk1><a:lt1><a:sysClr val='window' lastClr='FFFFFF'/></a:lt1><a:dk2><a:srgbClr val='44546A'/></a:dk2><a:lt2><a:srgbClr val='E7E6E6'/></a:lt2><a:accent1><a:srgbClr val='4472C4'/></a:accent1><a:accent2><a:srgbClr val='ED7D31'/></a:accent2><a:accent3><a:srgbClr val='A5A5A5'/></a:accent3><a:accent4><a:srgbClr val='FFC000'/></a:accent4><a:accent5><a:srgbClr val='5B9BD5'/></a:accent5><a:accent6><a:srgbClr val='70AD47'/></a:accent6><a:hlink><a:srgbClr val='0563C1'/></a:hlink><a:folHlink><a:srgbClr val='954F72'/></a:folHlink></a:clrScheme><a:fontScheme name='Office'><a:majorFont><a:latin typeface='Calibri Light' panose='020F0302020204030204'/><a:ea typeface=''/><a:cs typeface=''/><a:font script='Jpan' typeface='????? Light'/><a:font script='Hang' typeface='?? ??'/><a:font script='Hans' typeface='?? Light'/><a:font script='Hant' typeface='????'/><a:font script='Arab' typeface='Times New Roman'/><a:font script='Hebr' typeface='Times New Roman'/><a:font script='Thai' typeface='Tahoma'/><a:font script='Ethi' typeface='Nyala'/><a:font script='Beng' typeface='Vrinda'/><a:font script='Gujr' typeface='Shruti'/><a:font script='Khmr' typeface='MoolBoran'/><a:font script='Knda' typeface='Tunga'/><a:font script='Guru' typeface='Raavi'/><a:font script='Cans' typeface='Euphemia'/><a:font script='Cher' typeface='Plantagenet Cherokee'/><a:font script='Yiii' typeface='Microsoft Yi Baiti'/><a:font script='Tibt' typeface='Microsoft Himalaya'/><a:font script='Thaa' typeface='MV Boli'/><a:font script='Deva' typeface='Mangal'/><a:font script='Telu' typeface='Gautami'/><a:font script='Taml' typeface='Latha'/><a:font script='Syrc' typeface='Estrangelo Edessa'/><a:font script='Orya' typeface='Kalinga'/><a:font script='Mlym' typeface='Kartika'/><a:font script='Laoo' typeface='DokChampa'/><a:font script='Sinh' typeface='Iskoola Pota'/><a:font script='Mong' typeface='Mongolian Baiti'/><a:font script='Viet' typeface='Times New Roman'/><a:font script='Uigh' typeface='Microsoft Uighur'/><a:font script='Geor' typeface='Sylfaen'/><a:font script='Armn' typeface='Arial'/><a:font script='Bugi' typeface='Leelawadee UI'/><a:font script='Bopo' typeface='Microsoft JhengHei'/><a:font script='Java' typeface='Javanese Text'/><a:font script='Lisu' typeface='Segoe UI'/><a:font script='Mymr' typeface='Myanmar Text'/><a:font script='Nkoo' typeface='Ebrima'/><a:font script='Olck' typeface='Nirmala UI'/><a:font script='Osma' typeface='Ebrima'/><a:font script='Phag' typeface='Phagspa'/><a:font script='Syrn' typeface='Estrangelo Edessa'/><a:font script='Syrj' typeface='Estrangelo Edessa'/><a:font script='Syre' typeface='Estrangelo Edessa'/><a:font script='Sora' typeface='Nirmala UI'/><a:font script='Tale' typeface='Microsoft Tai Le'/><a:font script='Talu' typeface='Microsoft New Tai Lue'/><a:font script='Tfng' typeface='Ebrima'/></a:majorFont><a:minorFont><a:latin typeface='Calibri' panose='020F0502020204030204'/><a:ea typeface=''/><a:cs typeface=''/><a:font script='Jpan' typeface='?????'/><a:font script='Hang' typeface='?? ??'/><a:font script='Hans' typeface='??'/><a:font script='Hant' typeface='????'/><a:font script='Arab' typeface='Arial'/><a:font script='Hebr' typeface='Arial'/><a:font script='Thai' typeface='Tahoma'/><a:font script='Ethi' typeface='Nyala'/><a:font script='Beng' typeface='Vrinda'/><a:font script='Gujr' typeface='Shruti'/><a:font script='Khmr' typeface='DaunPenh'/><a:font script='Knda' typeface='Tunga'/><a:font script='Guru' typeface='Raavi'/><a:font script='Cans' typeface='Euphemia'/><a:font script='Cher' typeface='Plantagenet Cherokee'/><a:font script='Yiii' typeface='Microsoft Yi Baiti'/><a:font script='Tibt' typeface='Microsoft Himalaya'/><a:font script='Thaa' typeface='MV Boli'/><a:font script='Deva' typeface='Mangal'/><a:font script='Telu' typeface='Gautami'/><a:font script='Taml' typeface='Latha'/><a:font script='Syrc' typeface='Estrangelo Edessa'/><a:font script='Orya' typeface='Kalinga'/><a:font script='Mlym' typeface='Kartika'/><a:font script='Laoo' typeface='DokChampa'/><a:font script='Sinh' typeface='Iskoola Pota'/><a:font script='Mong' typeface='Mongolian Baiti'/><a:font script='Viet' typeface='Arial'/><a:font script='Uigh' typeface='Microsoft Uighur'/><a:font script='Geor' typeface='Sylfaen'/><a:font script='Armn' typeface='Arial'/><a:font script='Bugi' typeface='Leelawadee UI'/><a:font script='Bopo' typeface='Microsoft JhengHei'/><a:font script='Java' typeface='Javanese Text'/><a:font script='Lisu' typeface='Segoe UI'/><a:font script='Mymr' typeface='Myanmar Text'/><a:font script='Nkoo' typeface='Ebrima'/><a:font script='Olck' typeface='Nirmala UI'/><a:font script='Osma' typeface='Ebrima'/><a:font script='Phag' typeface='Phagspa'/><a:font script='Syrn' typeface='Estrangelo Edessa'/><a:font script='Syrj' typeface='Estrangelo Edessa'/><a:font script='Syre' typeface='Estrangelo Edessa'/><a:font script='Sora' typeface='Nirmala UI'/><a:font script='Tale' typeface='Microsoft Tai Le'/><a:font script='Talu' typeface='Microsoft New Tai Lue'/><a:font script='Tfng' typeface='Ebrima'/></a:minorFont></a:fontScheme><a:fmtScheme name='Office'><a:fillStyleLst><a:solidFill><a:schemeClr val='phClr'/></a:solidFill><a:gradFill rotWithShape='1'><a:gsLst><a:gs pos='0'><a:schemeClr val='phClr'><a:lumMod val='110000'/><a:satMod val='105000'/><a:tint val='67000'/></a:schemeClr></a:gs><a:gs pos='50000'><a:schemeClr val='phClr'><a:lumMod val='105000'/><a:satMod val='103000'/><a:tint val='73000'/></a:schemeClr></a:gs><a:gs pos='100000'><a:schemeClr val='phClr'><a:lumMod val='105000'/><a:satMod val='109000'/><a:tint val='81000'/></a:schemeClr></a:gs></a:gsLst><a:lin ang='5400000' scaled='0'/></a:gradFill><a:gradFill rotWithShape='1'><a:gsLst><a:gs pos='0'><a:schemeClr val='phClr'><a:satMod val='103000'/><a:lumMod val='102000'/><a:tint val='94000'/></a:schemeClr></a:gs><a:gs pos='50000'><a:schemeClr val='phClr'><a:satMod val='110000'/><a:lumMod val='100000'/><a:shade val='100000'/></a:schemeClr></a:gs><a:gs pos='100000'><a:schemeClr val='phClr'><a:lumMod val='99000'/><a:satMod val='120000'/><a:shade val='78000'/></a:schemeClr></a:gs></a:gsLst><a:lin ang='5400000' scaled='0'/></a:gradFill></a:fillStyleLst><a:lnStyleLst><a:ln w='6350' cap='flat' cmpd='sng' algn='ctr'><a:solidFill><a:schemeClr val='phClr'/></a:solidFill><a:prstDash val='solid'/><a:miter lim='800000'/></a:ln><a:ln w='12700' cap='flat' cmpd='sng' algn='ctr'><a:solidFill><a:schemeClr val='phClr'/></a:solidFill><a:prstDash val='solid'/><a:miter lim='800000'/></a:ln><a:ln w='19050' cap='flat' cmpd='sng' algn='ctr'><a:solidFill><a:schemeClr val='phClr'/></a:solidFill><a:prstDash val='solid'/><a:miter lim='800000'/></a:ln></a:lnStyleLst><a:effectStyleLst><a:effectStyle><a:effectLst/></a:effectStyle><a:effectStyle><a:effectLst/></a:effectStyle><a:effectStyle><a:effectLst><a:outerShdw blurRad='57150' dist='19050' dir='5400000' algn='ctr' rotWithShape='0'><a:srgbClr val='000000'><a:alpha val='63000'/></a:srgbClr></a:outerShdw></a:effectLst></a:effectStyle></a:effectStyleLst><a:bgFillStyleLst><a:solidFill><a:schemeClr val='phClr'/></a:solidFill><a:solidFill><a:schemeClr val='phClr'><a:tint val='95000'/><a:satMod val='170000'/></a:schemeClr></a:solidFill><a:gradFill rotWithShape='1'><a:gsLst><a:gs pos='0'><a:schemeClr val='phClr'><a:tint val='93000'/><a:satMod val='150000'/><a:shade val='98000'/><a:lumMod val='102000'/></a:schemeClr></a:gs><a:gs pos='50000'><a:schemeClr val='phClr'><a:tint val='98000'/><a:satMod val='130000'/><a:shade val='90000'/><a:lumMod val='103000'/></a:schemeClr></a:gs><a:gs pos='100000'><a:schemeClr val='phClr'><a:shade val='63000'/><a:satMod val='120000'/></a:schemeClr></a:gs></a:gsLst><a:lin ang='5400000' scaled='0'/></a:gradFill></a:bgFillStyleLst></a:fmtScheme></a:themeElements><a:objectDefaults/><a:extraClrSchemeLst/><a:extLst><a:ext uri='{05A4C25C-085E-4340-85A3-A5531E510DB2}'><thm15:themeFamily xmlns:thm15='http://schemas.microsoft.com/office/thememl/2012/main' name='Office Theme' id='{62F939B6-93AF-4DB8-9C6B-D6C7DFDC589F}' vid='{4A3C46E8-61CC-4603-A589-7422A47A8E4A}'/></a:ext></a:extLst></a:theme>"
        );

    private void InsertData(ZipArchive excelFile, IEnumerable<object> data)
    {
        string[] properties = [];
        object[] items = data.ToArray();

        if (items.Any())
        {
            if (items.First() is IDictionary<string, object> dictionary)
            {
                properties = dictionary
                    .Keys.Where(key => dictionary[key] is not IEnumerable or string)
                    .ToArray();
            }
            else
            {
                properties = items
                    .First()
                    .GetType()
                    .GetProperties()
                    .Where(property =>
                        (property.PropertyType.IsValueType
                            || property.PropertyType == typeof(string))
                        && property.PropertyType is not IEnumerable
                    )
                    .Select(property => property.Name)
                    .ToArray();
            }
        }

        excelFile.AddTextFile(
            "xl/worksheets/sheet1.xml",
            @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<worksheet xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships' xmlns:mc='http://schemas.openxmlformats.org/markup-compatibility/2006' mc:Ignorable='x14ac xr xr2 xr3' xmlns:x14ac='http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac' xmlns:xr='http://schemas.microsoft.com/office/spreadsheetml/2014/revision' xmlns:xr2='http://schemas.microsoft.com/office/spreadsheetml/2015/revision2' xmlns:xr3='http://schemas.microsoft.com/office/spreadsheetml/2016/revision3' xr:uid='{ECBAD190-FA55-4CFD-8AD0-DA1CA5D1EA29}'><dimension ref='A1'/><sheetViews><sheetView tabSelected='1' workbookViewId='0'/></sheetViews><sheetFormatPr defaultRowHeight='15' x14ac:dyDescent='0.25'/><sheetData>DATA</sheetData><pageMargins left='0.7' right='0.7' top='0.75' bottom='0.75' header='0.3' footer='0.3'/></worksheet>"
                .Replace("'", "\"")
                .Replace(
                    "DATA",
                    $"{BuildSheetHeaderRow(properties)}{BuildSheetDataRows(items, properties)}"
                )
        );
    }

    private string BuildStyles()
    {
        string dateFormat =
            resources.ForNameAndCulture("dateformat", culture)?.DisplayName
            ?? "yyyy-MM-ddThh:mm:ss";
        return $@"<?xml version='1.0' encoding='UTF-8'?>
<styleSheet xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main' xmlns:mc='http://schemas.openxmlformats.org/markup-compatibility/2006' mc:Ignorable='x14ac x16r2 xr' xmlns:x14ac='http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac' xmlns:x16r2='http://schemas.microsoft.com/office/spreadsheetml/2015/02/main' xmlns:xr='http://schemas.microsoft.com/office/spreadsheetml/2014/revision'>
  	<numFmts count='1'>
        <numFmt numFmtId='164' formatCode='{dateFormat}'/>
    </numFmts>
    <fonts count='1' x14ac:knownFonts='1'>
    <font>
      <sz val='11'/>
      <color theme='1'/>
      <name val='Calibri'/>
      <family val='2'/>
      <scheme val='minor'/>
    </font>
  </fonts>
  <fills count='2'>
    <fill>
      <patternFill patternType='none'/>
    </fill>
    <fill>
      <patternFill patternType='gray125'/>
    </fill>
  </fills>
  <borders count='1'>
    <border>
      <left/>
      <right/>
      <top/>
      <bottom/>
      <diagonal/>
    </border>
  </borders>
  <cellStyleXfs count='2'>
    <xf numFmtId='0' fontId='0' fillId='0' borderId='0' />
    <xf numFmtId='164' fontId='0' fillId='0' borderId='0' />
  </cellStyleXfs>
  <cellXfs count='3'>
    <xf numFmtId='0' fontId='0' fillId='0' borderId='0' xfId='0'/>
    <xf numFmtId='164' fontId='0' fillId='0' borderId='0' xfId='1'/>
    <xf numFmtId='4' fontId='0' fillId='0' borderId='0' xfId='2' applyNumberFormatting='1'/>
  </cellXfs>
  <cellStyles count='1'>
    <cellStyle name='Normal' xfId='0' builtinId='0'/>
  </cellStyles>
  <dxfs count='0'/>
  <tableStyles count='0' defaultTableStyle='TableStyleMedium2' defaultPivotStyle='PivotStyleLight16'/>
  <extLst>
    <ext uri='{{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}}' xmlns:x14='http://schemas.microsoft.com/office/spreadsheetml/2009/9/main'>
      <x14:slicerStyles defaultSlicerStyle='SlicerStyleLight1'/>
    </ext>
    <ext uri='{{9260A510-F301-46a8-8635-F512D64BE5F5}}' xmlns:x15='http://schemas.microsoft.com/office/spreadsheetml/2010/11/main'>
      <x15:timelineStyles defaultTimelineStyle='TimeSlicerStyleLight1'/>
    </ext>
  </extLst>
</styleSheet>
";
    }

    private string BuildSheetHeaderRow(string[] properties)
    {
        StringBuilder result = new();
        _ = result.Append(
            $"<row r=\"1\" x14ac:dyDescent=\"0.25\" spans=\"1:{Math.Max(1, properties.Length)}\">"
        );

        for (int index = 0; index < properties.Length; index++)
        {
            string displayName =
                resources.ForNameAndCulture(properties[index], culture)?.ShortDisplayName
                ?? properties[index];
            _ = result.Append(
                $"<c r=\"{index.ToExcelColumn()}1\" t=\"inlineStr\"><is><t>{displayName}</t></is></c>"
            );
        }

        _ = result.Append("</row>");
        return result.ToString().Replace("'", "\"");
    }

    private string BuildSheetDataRows(IEnumerable<object> data, string[] properties)
    {
        string dateFormat =
            resources.ForNameAndCulture("dateformat", culture)?.DisplayName
            ?? "yyyy-MM-ddThh:mm:ss";
        string moneyFormat =
            resources.FirstOrDefault(resource => resource.Name == "moneyformat")?.DisplayName
            ?? "n";
        StringBuilder result = new();
        int rowNumber = 2;
        PropertyInfo[] objectProperties =
            data.Any() && data.First() is not IDictionary<string, object>
                ? data.First().GetType().GetProperties()
                : null;

        foreach (object item in data)
        {
            _ = result.Append(
                $"<row r=\"{rowNumber}\" x14ac:dyDescent=\"0.25\" spans=\"1:{properties.Length}\">"
            );

            for (int columnIndex = 0; columnIndex < properties.Length; columnIndex++)
            {
                IDictionary<string, object> dictionary = item as IDictionary<string, object>;
                object propertyValue =
                    dictionary != null
                        ? dictionary[properties[columnIndex]]
                        : objectProperties
                            .First(property => property.Name == properties[columnIndex])
                            .GetValue(item);

                _ = result.Append(
                    propertyValue switch
                    {
                        DateTimeOffset dateTimeOffset =>
                            $"<c r=\"{columnIndex.ToExcelColumn()}{rowNumber}\" t=\"d\" s=\"1\"><v>{dateTimeOffset:s}</v></c>",
                        decimal decimalValue =>
                            $"<c r=\"{columnIndex.ToExcelColumn()}{rowNumber}\" t=\"n\" s=\"2\"><v>{decimalValue}</v></c>",
                        _ =>
                            $"<c r=\"{columnIndex.ToExcelColumn()}{rowNumber}\" t=\"inlineStr\"><is><t>{FormatValue(propertyValue, dateFormat, moneyFormat, culture)}</t></is></c>",
                    }
                );
            }

            _ = result.Append("</row>");
            rowNumber++;
        }

        return result.ToString();
    }

    private static string FormatValue(
        object value,
        string dateFormat,
        string moneyFormat,
        string culture
    ) =>
        value switch
        {
            DateTime dateTime =>
                dateTime.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture)),
            DateTimeOffset dateTimeOffset =>
                dateTimeOffset.ToString(
                    dateFormat,
                    CultureInfo.CreateSpecificCulture(culture)
                ),
            decimal decimalValue =>
                decimalValue.ToString(moneyFormat, CultureInfo.CreateSpecificCulture(culture)),
            string stringValue => stringValue.Replace("&", "&amp;"),
            Guid guid => $"{guid}",
            null => string.Empty,
            _ => value.ToString(),
        };
}

internal static class FormatterIntExtensions
{
    public static string ToExcelColumn(this int index)
    {
        index += 1;
        StringBuilder letters = new();

        while (index > 0)
        {
            int modulo = (index - 1) % 26;
            int character = modulo + 65;

            while (character > 90)
            {
                character -= 26;
            }

            letters.Append(char.ConvertFromUtf32(character));
            index = (index - modulo) / 26;
        }

        return new string(letters.ToString().Reverse().ToArray());
    }
}

