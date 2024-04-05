using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace cCoder.Core.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Audit");

            migrationBuilder.EnsureSchema(
                name: "CMS");

            migrationBuilder.EnsureSchema(
                name: "Security");

            migrationBuilder.EnsureSchema(
                name: "DMS");

            migrationBuilder.EnsureSchema(
                name: "Logging");

            migrationBuilder.EnsureSchema(
                name: "Mail");

            migrationBuilder.EnsureSchema(
                name: "Packaging");

            migrationBuilder.EnsureSchema(
                name: "Planning");

            migrationBuilder.EnsureSchema(
                name: "Tasks");

            migrationBuilder.EnsureSchema(
                name: "Workflow");

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                schema: "Audit",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EntityId = table.Column<string>(nullable: true),
                    EventId = table.Column<Guid>(nullable: false),
                    EntityType = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(nullable: false),
                    Event = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Detail = table.Column<string>(nullable: true),
                    TimeOfEvent = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Apps",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DefaultCultureId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Domain = table.Column<string>(nullable: false),
                    DefaultTheme = table.Column<string>(nullable: false),
                    ConfigJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cultures",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cultures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                schema: "Logging",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Level = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: false),
                    AppName = table.Column<string>(nullable: false),
                    TypeName = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueuedEmails",
                schema: "Mail",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Subject = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    From = table.Column<string>(nullable: false),
                    To = table.Column<string>(nullable: false),
                    CC = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SentEmails",
                schema: "Mail",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Subject = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    From = table.Column<string>(nullable: false),
                    To = table.Column<string>(nullable: false),
                    CC = table.Column<string>(nullable: true),
                    SentOn = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                schema: "Packaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: false),
                    Category = table.Column<string>(maxLength: 100, nullable: false),
                    SourceApi = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Privileges",
                schema: "Security",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 200, nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: false),
                    PortalAdminsOnly = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privileges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditDataItems",
                schema: "Audit",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuditEntryId = table.Column<int>(nullable: false),
                    PropertyName = table.Column<string>(nullable: true),
                    PreviousValue = table.Column<string>(nullable: true),
                    NewValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditDataItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditDataItems_AuditEntries_AuditEntryId",
                        column: x => x.AuditEntryId,
                        principalSchema: "Audit",
                        principalTable: "AuditEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Components",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    ResourceKey = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Script = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Components_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AppId = table.Column<int>(nullable: false),
                    ResourceKey = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    RootMetaItem = table.Column<string>(nullable: true),
                    FieldsetTemplate = table.Column<string>(nullable: true),
                    FieldTemplate = table.Column<string>(nullable: true),
                    RawMetaJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forms_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Layouts",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    HeaderHtml = table.Column<string>(nullable: true),
                    Html = table.Column<string>(nullable: true),
                    Script = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Layouts_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<int>(nullable: true),
                    AppId = table.Column<int>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    ShowOnMenus = table.Column<bool>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    ResourceKey = table.Column<string>(nullable: true),
                    Layout = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pages_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pages_Pages_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "CMS",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppId = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Culture = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    DisplayName = table.Column<string>(nullable: false),
                    ShortDisplayName = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    ResourceKey = table.Column<string>(nullable: true),
                    RawString = table.Column<string>(nullable: true),
                    AppId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templates_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                schema: "DMS",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AppId = table.Column<int>(nullable: false),
                    ParentId = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "DMS",
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Calendars",
                schema: "Planning",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendars_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Security",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AppId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Privs = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BusinessProcesses",
                schema: "Workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AppId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Category = table.Column<string>(maxLength: 100, nullable: false),
                    DefinitionJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessProcesses_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlows",
                schema: "Workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AppId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    DefinitionJson = table.Column<string>(nullable: true),
                    ConfigJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlows_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppCultures",
                schema: "CMS",
                columns: table => new
                {
                    AppId = table.Column<int>(nullable: false),
                    CultureId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCultures", x => new { x.AppId, x.CultureId });
                    table.ForeignKey(
                        name: "FK_AppCultures_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppCultures_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "CMS",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MetaItems",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CultureId = table.Column<string>(nullable: true),
                    Context = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Operation = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaItems_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "CMS",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Security",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DefaultCultureId = table.Column<string>(nullable: false),
                    DisplayName = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Cultures_DefaultCultureId",
                        column: x => x.DefaultCultureId,
                        principalSchema: "CMS",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogDataItems",
                schema: "Logging",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LogEntryId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogDataItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogDataItems_LogEntries_LogEntryId",
                        column: x => x.LogEntryId,
                        principalSchema: "Logging",
                        principalTable: "LogEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailSendFailures",
                schema: "Mail",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmailId = table.Column<int>(nullable: false),
                    AttemptedOn = table.Column<DateTimeOffset>(nullable: false),
                    FailureReason = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSendFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSendFailures_QueuedEmails_EmailId",
                        column: x => x.EmailId,
                        principalSchema: "Mail",
                        principalTable: "QueuedEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageItems",
                schema: "Packaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PackageId = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageItems_Packages_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "Packaging",
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FormId = table.Column<Guid>(nullable: false),
                    DataJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Forms_FormId",
                        column: x => x.FormId,
                        principalSchema: "CMS",
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contents",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PageId = table.Column<int>(nullable: false),
                    CultureId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Html = table.Column<string>(nullable: true),
                    Script = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contents_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "CMS",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contents_Pages_PageId",
                        column: x => x.PageId,
                        principalSchema: "CMS",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PageInfo",
                schema: "CMS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PageId = table.Column<int>(nullable: false),
                    CultureId = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Keywords = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageInfo_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "CMS",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageInfo_Pages_PageId",
                        column: x => x.PageId,
                        principalSchema: "CMS",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                schema: "DMS",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FolderId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    MimeType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Folders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "DMS",
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "Planning",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CalendarId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Start = table.Column<DateTimeOffset>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalSchema: "Planning",
                        principalTable: "Calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                schema: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CalendarId = table.Column<int>(nullable: false),
                    EventName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalSchema: "Planning",
                        principalTable: "Calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FolderRoles",
                schema: "Security",
                columns: table => new
                {
                    FolderId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderRoles", x => new { x.FolderId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_FolderRoles_Folders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "DMS",
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FolderRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Security",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PageRoles",
                schema: "Security",
                columns: table => new
                {
                    PageId = table.Column<int>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageRoles", x => new { x.PageId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_PageRoles_Pages_PageId",
                        column: x => x.PageId,
                        principalSchema: "CMS",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Security",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BusinessProcessWorkflows",
                schema: "Workflow",
                columns: table => new
                {
                    FlowId = table.Column<Guid>(nullable: false),
                    BusinessProcessId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProcessWorkflows", x => new { x.FlowId, x.BusinessProcessId });
                    table.ForeignKey(
                        name: "FK_BusinessProcessWorkflows_BusinessProcesses_BusinessProcessId",
                        column: x => x.BusinessProcessId,
                        principalSchema: "Workflow",
                        principalTable: "BusinessProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessProcessWorkflows_WorkFlows_FlowId",
                        column: x => x.FlowId,
                        principalSchema: "Workflow",
                        principalTable: "WorkFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlowInstances",
                schema: "Workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FlowDefinitionId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ContextJson = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowInstances_WorkFlows_FlowDefinitionId",
                        column: x => x.FlowDefinitionId,
                        principalSchema: "Workflow",
                        principalTable: "WorkFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowEvents",
                schema: "Workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    EventContext = table.Column<string>(nullable: true),
                    ProcessId = table.Column<Guid>(nullable: false),
                    FlowId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowEvents_WorkFlows_FlowId",
                        column: x => x.FlowId,
                        principalSchema: "Workflow",
                        principalTable: "WorkFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowEvents_BusinessProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalSchema: "Workflow",
                        principalTable: "BusinessProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "Security",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.RoleId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Security",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileContents",
                schema: "DMS",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    Version = table.Column<int>(nullable: false),
                    RawData = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileContents_Files_FileId",
                        column: x => x.FileId,
                        principalSchema: "DMS",
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTasks",
                schema: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AppId = table.Column<int>(nullable: false),
                    ScheduleId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    HandlingEndpointUrl = table.Column<string>(nullable: false),
                    LastExecuted = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledTasks_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "CMS",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledTasks_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "Tasks",
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTaskDataItems",
                schema: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false),
                    ScheduledTaskId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTaskDataItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledTaskDataItems_ScheduledTasks_ScheduledTaskId",
                        column: x => x.ScheduledTaskId,
                        principalSchema: "Tasks",
                        principalTable: "ScheduledTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "CMS",
                table: "Cultures",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { "", "Default" },
                    { "en", "English" },
                    { "en-GB", "English (British)" },
                    { "fr-FR", "French" }
                });

            migrationBuilder.InsertData(
                schema: "Security",
                table: "Privileges",
                columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
                values: new object[,]
                {
                    { "businessprocess_create", "Allows users to Create BusinessProcess objects.", "Create", false, "BusinessProcess" },
                    { "businessprocess_read", "Allows users to Read BusinessProcess objects.", "Read", false, "BusinessProcess" },
                    { "businessprocess_update", "Allows users to Update BusinessProcess objects.", "Update", false, "BusinessProcess" },
                    { "businessprocess_delete", "Allows users to Delete BusinessProcess objects.", "Delete", false, "BusinessProcess" },
                    { "businessprocess_admin", "Allows users to Administer BusinessProcess objects.", "Delete", false, "BusinessProcess" },
                    { "workflowevent_create", "Allows users to Create WorkflowEvent objects.", "Create", false, "WorkflowEvent" },
                    { "workflowevent_read", "Allows users to Read WorkflowEvent objects.", "Read", false, "WorkflowEvent" },
                    { "workflowevent_update", "Allows users to Update WorkflowEvent objects.", "Update", false, "WorkflowEvent" },
                    { "workflowevent_delete", "Allows users to Delete WorkflowEvent objects.", "Delete", false, "WorkflowEvent" },
                    { "workflowevent_admin", "Allows users to Administer WorkflowEvent objects.", "Delete", false, "WorkflowEvent" },
                    { "flowdefinition_read", "Allows users to Read FlowDefinition objects.", "Read", false, "FlowDefinition" },
                    { "scheduledtaskdataitem_admin", "Allows users to Administer ScheduledTaskDataItem objects.", "Delete", false, "ScheduledTaskDataItem" },
                    { "flowdefinition_update", "Allows users to Update FlowDefinition objects.", "Update", false, "FlowDefinition" },
                    { "flowdefinition_delete", "Allows users to Delete FlowDefinition objects.", "Delete", false, "FlowDefinition" },
                    { "flowdefinition_admin", "Allows users to Administer FlowDefinition objects.", "Delete", false, "FlowDefinition" },
                    { "flowdefinition_getflow", "Allows users to call GetFlow on FlowDefinition objects.", "GetFlow", false, "FlowDefinition" },
                    { "flowdefinition_getconfig", "Allows users to call GetConfig on FlowDefinition objects.", "GetConfig", false, "FlowDefinition" },
                    { "flowinstancedata_create", "Allows users to Create FlowInstanceData objects.", "Create", false, "FlowInstanceData" },
                    { "flowinstancedata_read", "Allows users to Read FlowInstanceData objects.", "Read", false, "FlowInstanceData" },
                    { "flowinstancedata_update", "Allows users to Update FlowInstanceData objects.", "Update", false, "FlowInstanceData" },
                    { "flowdefinition_create", "Allows users to Create FlowDefinition objects.", "Create", false, "FlowDefinition" },
                    { "scheduledtaskdataitem_delete", "Allows users to Delete ScheduledTaskDataItem objects.", "Delete", false, "ScheduledTaskDataItem" },
                    { "scheduledtaskdataitem_read", "Allows users to Read ScheduledTaskDataItem objects.", "Read", false, "ScheduledTaskDataItem" },
                    { "flowinstancedata_delete", "Allows users to Delete FlowInstanceData objects.", "Delete", false, "FlowInstanceData" },
                    { "sentemail_read", "Allows users to Read SentEmail objects.", "Read", false, "SentEmail" },
                    { "sentemail_update", "Allows users to Update SentEmail objects.", "Update", false, "SentEmail" },
                    { "sentemail_delete", "Allows users to Delete SentEmail objects.", "Delete", false, "SentEmail" },
                    { "sentemail_admin", "Allows users to Administer SentEmail objects.", "Delete", false, "SentEmail" },
                    { "emailsendfailure_create", "Allows users to Create EmailSendFailure objects.", "Create", false, "EmailSendFailure" },
                    { "emailsendfailure_read", "Allows users to Read EmailSendFailure objects.", "Read", false, "EmailSendFailure" },
                    { "emailsendfailure_update", "Allows users to Update EmailSendFailure objects.", "Update", false, "EmailSendFailure" },
                    { "emailsendfailure_delete", "Allows users to Delete EmailSendFailure objects.", "Delete", false, "EmailSendFailure" },
                    { "emailsendfailure_admin", "Allows users to Administer EmailSendFailure objects.", "Delete", false, "EmailSendFailure" },
                    { "scheduledtaskdataitem_update", "Allows users to Update ScheduledTaskDataItem objects.", "Update", false, "ScheduledTaskDataItem" },
                    { "schedule_create", "Allows users to Create Schedule objects.", "Create", false, "Schedule" },
                    { "schedule_update", "Allows users to Update Schedule objects.", "Update", false, "Schedule" },
                    { "schedule_delete", "Allows users to Delete Schedule objects.", "Delete", false, "Schedule" },
                    { "schedule_admin", "Allows users to Administer Schedule objects.", "Delete", false, "Schedule" },
                    { "scheduledtask_create", "Allows users to Create ScheduledTask objects.", "Create", false, "ScheduledTask" },
                    { "scheduledtask_read", "Allows users to Read ScheduledTask objects.", "Read", false, "ScheduledTask" },
                    { "scheduledtask_update", "Allows users to Update ScheduledTask objects.", "Update", false, "ScheduledTask" },
                    { "scheduledtask_delete", "Allows users to Delete ScheduledTask objects.", "Delete", false, "ScheduledTask" },
                    { "scheduledtask_admin", "Allows users to Administer ScheduledTask objects.", "Delete", false, "ScheduledTask" },
                    { "scheduledtaskdataitem_create", "Allows users to Create ScheduledTaskDataItem objects.", "Create", false, "ScheduledTaskDataItem" },
                    { "schedule_read", "Allows users to Read Schedule objects.", "Read", false, "Schedule" },
                    { "flowinstancedata_admin", "Allows users to Administer FlowInstanceData objects.", "Delete", false, "FlowInstanceData" },
                    { "package_read", "Allows users to Read Package objects.", "Read", false, "Package" },
                    { "sentemail_create", "Allows users to Create SentEmail objects.", "Create", false, "SentEmail" },
                    { "auditdataitem_create", "Allows users to Create AuditDataItem objects.", "Create", false, "AuditDataItem" },
                    { "auditdataitem_read", "Allows users to Read AuditDataItem objects.", "Read", false, "AuditDataItem" },
                    { "auditdataitem_update", "Allows users to Update AuditDataItem objects.", "Update", false, "AuditDataItem" },
                    { "auditdataitem_delete", "Allows users to Delete AuditDataItem objects.", "Delete", false, "AuditDataItem" },
                    { "auditdataitem_admin", "Allows users to Administer AuditDataItem objects.", "Delete", false, "AuditDataItem" },
                    { "logentry_create", "Allows users to Create LogEntry objects.", "Create", false, "LogEntry" },
                    { "logentry_read", "Allows users to Read LogEntry objects.", "Read", false, "LogEntry" },
                    { "logentry_update", "Allows users to Update LogEntry objects.", "Update", false, "LogEntry" },
                    { "logentry_delete", "Allows users to Delete LogEntry objects.", "Delete", false, "LogEntry" },
                    { "auditentry_admin", "Allows users to Administer AuditEntry objects.", "Delete", false, "AuditEntry" },
                    { "logentry_admin", "Allows users to Administer LogEntry objects.", "Delete", false, "LogEntry" },
                    { "logdataitem_read", "Allows users to Read LogDataItem objects.", "Read", false, "LogDataItem" },
                    { "logdataitem_update", "Allows users to Update LogDataItem objects.", "Update", false, "LogDataItem" },
                    { "logdataitem_delete", "Allows users to Delete LogDataItem objects.", "Delete", false, "LogDataItem" },
                    { "logdataitem_admin", "Allows users to Administer LogDataItem objects.", "Delete", false, "LogDataItem" },
                    { "user_create", "Allows users to Create User objects.", "Create", false, "User" },
                    { "user_read", "Allows users to Read User objects.", "Read", false, "User" },
                    { "user_update", "Allows users to Update User objects.", "Update", false, "User" },
                    { "user_delete", "Allows users to Delete User objects.", "Delete", false, "User" },
                    { "user_admin", "Allows users to Administer User objects.", "Delete", false, "User" },
                    { "logdataitem_create", "Allows users to Create LogDataItem objects.", "Create", false, "LogDataItem" },
                    { "package_create", "Allows users to Create Package objects.", "Create", false, "Package" },
                    { "auditentry_delete", "Allows users to Delete AuditEntry objects.", "Delete", false, "AuditEntry" },
                    { "auditentry_read", "Allows users to Read AuditEntry objects.", "Read", false, "AuditEntry" },
                    { "package_update", "Allows users to Update Package objects.", "Update", false, "Package" },
                    { "package_delete", "Allows users to Delete Package objects.", "Delete", false, "Package" },
                    { "package_admin", "Allows users to Administer Package objects.", "Delete", false, "Package" },
                    { "packageitem_create", "Allows users to Create PackageItem objects.", "Create", false, "PackageItem" },
                    { "packageitem_read", "Allows users to Read PackageItem objects.", "Read", false, "PackageItem" },
                    { "packageitem_update", "Allows users to Update PackageItem objects.", "Update", false, "PackageItem" },
                    { "packageitem_delete", "Allows users to Delete PackageItem objects.", "Delete", false, "PackageItem" },
                    { "packageitem_admin", "Allows users to Administer PackageItem objects.", "Delete", false, "PackageItem" },
                    { "packageitem_unpack", "Allows users to call Unpack on PackageItem objects.", "Unpack", false, "PackageItem" },
                    { "auditentry_update", "Allows users to Update AuditEntry objects.", "Update", false, "AuditEntry" },
                    { "role_create", "Allows users to Create Role objects.", "Create", false, "Role" },
                    { "role_update", "Allows users to Update Role objects.", "Update", false, "Role" },
                    { "role_delete", "Allows users to Delete Role objects.", "Delete", false, "Role" },
                    { "role_admin", "Allows users to Administer Role objects.", "Delete", false, "Role" },
                    { "privilege_create", "Allows users to Create Privilege objects.", "Create", false, "Privilege" },
                    { "privilege_read", "Allows users to Read Privilege objects.", "Read", false, "Privilege" },
                    { "privilege_update", "Allows users to Update Privilege objects.", "Update", false, "Privilege" },
                    { "privilege_delete", "Allows users to Delete Privilege objects.", "Delete", false, "Privilege" },
                    { "privilege_admin", "Allows users to Administer Privilege objects.", "Delete", false, "Privilege" },
                    { "auditentry_create", "Allows users to Create AuditEntry objects.", "Create", false, "AuditEntry" },
                    { "role_read", "Allows users to Read Role objects.", "Read", false, "Role" },
                    { "queuedemail_admin", "Allows users to Administer QueuedEmail objects.", "Delete", false, "QueuedEmail" },
                    { "queuedemail_update", "Allows users to Update QueuedEmail objects.", "Update", false, "QueuedEmail" },
                    { "user_isadminofapp", "Allows users to call IsAdminOfApp on User objects.", "IsAdminOfApp", false, "User" },
                    { "pageinfo_admin", "Allows users to Administer PageInfo objects.", "Delete", false, "PageInfo" },
                    { "content_create", "Allows users to Create Content objects.", "Create", false, "Content" },
                    { "content_read", "Allows users to Read Content objects.", "Read", false, "Content" },
                    { "content_update", "Allows users to Update Content objects.", "Update", false, "Content" },
                    { "content_delete", "Allows users to Delete Content objects.", "Delete", false, "Content" },
                    { "content_admin", "Allows users to Administer Content objects.", "Delete", false, "Content" },
                    { "component_create", "Allows users to Create Component objects.", "Create", false, "Component" },
                    { "component_read", "Allows users to Read Component objects.", "Read", false, "Component" },
                    { "component_update", "Allows users to Update Component objects.", "Update", false, "Component" },
                    { "pageinfo_delete", "Allows users to Delete PageInfo objects.", "Delete", false, "PageInfo" },
                    { "component_delete", "Allows users to Delete Component objects.", "Delete", false, "Component" },
                    { "component_render", "Allows users to call Render on Component objects.", "Render", false, "Component" },
                    { "resource_create", "Allows users to Create Resource objects.", "Create", false, "Resource" },
                    { "resource_read", "Allows users to Read Resource objects.", "Read", false, "Resource" },
                    { "resource_update", "Allows users to Update Resource objects.", "Update", false, "Resource" },
                    { "resource_delete", "Allows users to Delete Resource objects.", "Delete", false, "Resource" },
                    { "resource_admin", "Allows users to Administer Resource objects.", "Delete", false, "Resource" },
                    { "culture_create", "Allows users to Create Culture objects.", "Create", false, "Culture" },
                    { "culture_read", "Allows users to Read Culture objects.", "Read", false, "Culture" },
                    { "culture_update", "Allows users to Update Culture objects.", "Update", false, "Culture" },
                    { "component_admin", "Allows users to Administer Component objects.", "Delete", false, "Component" },
                    { "culture_delete", "Allows users to Delete Culture objects.", "Delete", false, "Culture" },
                    { "pageinfo_update", "Allows users to Update PageInfo objects.", "Update", false, "PageInfo" },
                    { "pageinfo_create", "Allows users to Create PageInfo objects.", "Create", false, "PageInfo" },
                    { "layout_create", "Allows users to Create Layout objects.", "Create", false, "Layout" },
                    { "layout_read", "Allows users to Read Layout objects.", "Read", false, "Layout" },
                    { "layout_update", "Allows users to Update Layout objects.", "Update", false, "Layout" },
                    { "layout_delete", "Allows users to Delete Layout objects.", "Delete", false, "Layout" },
                    { "layout_admin", "Allows users to Administer Layout objects.", "Delete", false, "Layout" },
                    { "app_create", "Allows users to Create App objects.", "Create", false, "App" },
                    { "app_read", "Allows users to Read App objects.", "Read", false, "App" },
                    { "app_update", "Allows users to Update App objects.", "Update", false, "App" },
                    { "app_delete", "Allows users to Delete App objects.", "Delete", false, "App" },
                    { "pageinfo_read", "Allows users to Read PageInfo objects.", "Read", false, "PageInfo" },
                    { "app_admin", "Allows users to Administer App objects.", "Delete", false, "App" },
                    { "page_read", "Allows users to Read Page objects.", "Read", false, "Page" },
                    { "page_update", "Allows users to Update Page objects.", "Update", false, "Page" },
                    { "page_delete", "Allows users to Delete Page objects.", "Delete", false, "Page" },
                    { "page_admin", "Allows users to Administer Page objects.", "Delete", false, "Page" },
                    { "page_setcontent", "Allows users to call SetContent on Page objects.", "SetContent", false, "Page" },
                    { "page_infoforculture", "Allows users to call InfoForCulture on Page objects.", "InfoForCulture", false, "Page" },
                    { "page_contentforculture", "Allows users to call ContentForCulture on Page objects.", "ContentForCulture", false, "Page" },
                    { "page_torenderresult", "Allows users to call ToRenderResult on Page objects.", "ToRenderResult", false, "Page" },
                    { "page_computepermissions", "Allows users to call ComputePermissions on Page objects.", "ComputePermissions", false, "Page" },
                    { "page_create", "Allows users to Create Page objects.", "Create", false, "Page" },
                    { "queuedemail_delete", "Allows users to Delete QueuedEmail objects.", "Delete", false, "QueuedEmail" },
                    { "culture_admin", "Allows users to Administer Culture objects.", "Delete", false, "Culture" },
                    { "form_read", "Allows users to Read Form objects.", "Read", false, "Form" },
                    { "file_delete", "Allows users to Delete File objects.", "Delete", false, "File" },
                    { "file_admin", "Allows users to Administer File objects.", "Delete", false, "File" },
                    { "file_getcontent", "Allows users to call GetContent on File objects.", "GetContent", false, "File" },
                    { "filecontent_create", "Allows users to Create FileContent objects.", "Create", false, "FileContent" },
                    { "filecontent_read", "Allows users to Read FileContent objects.", "Read", false, "FileContent" },
                    { "filecontent_update", "Allows users to Update FileContent objects.", "Update", false, "FileContent" },
                    { "filecontent_delete", "Allows users to Delete FileContent objects.", "Delete", false, "FileContent" },
                    { "filecontent_admin", "Allows users to Administer FileContent objects.", "Delete", false, "FileContent" },
                    { "calendar_create", "Allows users to Create Calendar objects.", "Create", false, "Calendar" },
                    { "file_update", "Allows users to Update File objects.", "Update", false, "File" },
                    { "calendar_read", "Allows users to Read Calendar objects.", "Read", false, "Calendar" },
                    { "calendar_delete", "Allows users to Delete Calendar objects.", "Delete", false, "Calendar" },
                    { "calendar_admin", "Allows users to Administer Calendar objects.", "Delete", false, "Calendar" },
                    { "event_create", "Allows users to Create Event objects.", "Create", false, "Event" },
                    { "event_read", "Allows users to Read Event objects.", "Read", false, "Event" },
                    { "event_update", "Allows users to Update Event objects.", "Update", false, "Event" },
                    { "event_delete", "Allows users to Delete Event objects.", "Delete", false, "Event" },
                    { "event_admin", "Allows users to Administer Event objects.", "Delete", false, "Event" },
                    { "queuedemail_create", "Allows users to Create QueuedEmail objects.", "Create", false, "QueuedEmail" },
                    { "queuedemail_read", "Allows users to Read QueuedEmail objects.", "Read", false, "QueuedEmail" },
                    { "calendar_update", "Allows users to Update Calendar objects.", "Update", false, "Calendar" },
                    { "form_create", "Allows users to Create Form objects.", "Create", false, "Form" },
                    { "file_read", "Allows users to Read File objects.", "Read", false, "File" },
                    { "folder_admin", "Allows users to Administer Folder objects.", "Delete", false, "Folder" },
                    { "form_update", "Allows users to Update Form objects.", "Update", false, "Form" },
                    { "form_delete", "Allows users to Delete Form objects.", "Delete", false, "Form" },
                    { "form_admin", "Allows users to Administer Form objects.", "Delete", false, "Form" },
                    { "form_render", "Allows users to call Render on Form objects.", "Render", false, "Form" },
                    { "template_create", "Allows users to Create Template objects.", "Create", false, "Template" },
                    { "template_read", "Allows users to Read Template objects.", "Read", false, "Template" },
                    { "template_update", "Allows users to Update Template objects.", "Update", false, "Template" },
                    { "template_delete", "Allows users to Delete Template objects.", "Delete", false, "Template" },
                    { "template_admin", "Allows users to Administer Template objects.", "Delete", false, "Template" },
                    { "file_create", "Allows users to Create File objects.", "Create", false, "File" },
                    { "template_buildemailto", "Allows users to call BuildEmailTo on Template objects.", "BuildEmailTo", false, "Template" },
                    { "metaitem_create", "Allows users to Create MetaItem objects.", "Create", false, "MetaItem" },
                    { "metaitem_read", "Allows users to Read MetaItem objects.", "Read", false, "MetaItem" },
                    { "metaitem_update", "Allows users to Update MetaItem objects.", "Update", false, "MetaItem" },
                    { "metaitem_delete", "Allows users to Delete MetaItem objects.", "Delete", false, "MetaItem" },
                    { "metaitem_admin", "Allows users to Administer MetaItem objects.", "Delete", false, "MetaItem" },
                    { "folder_create", "Allows users to Create Folder objects.", "Create", false, "Folder" },
                    { "folder_read", "Allows users to Read Folder objects.", "Read", false, "Folder" },
                    { "folder_update", "Allows users to Update Folder objects.", "Update", false, "Folder" },
                    { "folder_delete", "Allows users to Delete Folder objects.", "Delete", false, "Folder" },
                    { "template_render", "Allows users to call Render on Template objects.", "Render", false, "Template" },
                    { "user_isuserofapp", "Allows users to call IsUserOfApp on User objects.", "IsUserOfApp", false, "User" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditDataItems_AuditEntryId",
                schema: "Audit",
                table: "AuditDataItems",
                column: "AuditEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCultures_CultureId",
                schema: "CMS",
                table: "AppCultures",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_AppId",
                schema: "CMS",
                table: "Components",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CultureId",
                schema: "CMS",
                table: "Contents",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_PageId",
                schema: "CMS",
                table: "Contents",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_AppId",
                schema: "CMS",
                table: "Forms",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Layouts_AppId",
                schema: "CMS",
                table: "Layouts",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaItems_CultureId",
                schema: "CMS",
                table: "MetaItems",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_PageInfo_CultureId",
                schema: "CMS",
                table: "PageInfo",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_PageInfo_PageId",
                schema: "CMS",
                table: "PageInfo",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_AppId",
                schema: "CMS",
                table: "Pages",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ParentId",
                schema: "CMS",
                table: "Pages",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_AppId",
                schema: "CMS",
                table: "Resources",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_FormId",
                schema: "CMS",
                table: "Submissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_AppId",
                schema: "CMS",
                table: "Templates",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_FileContents_FileId",
                schema: "DMS",
                table: "FileContents",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderId",
                schema: "DMS",
                table: "Files",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_AppId",
                schema: "DMS",
                table: "Folders",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentId",
                schema: "DMS",
                table: "Folders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_LogDataItems_LogEntryId",
                schema: "Logging",
                table: "LogDataItems",
                column: "LogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSendFailures_EmailId",
                schema: "Mail",
                table: "EmailSendFailures",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_PackageId",
                schema: "Packaging",
                table: "PackageItems",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_AppId",
                schema: "Planning",
                table: "Calendars",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CalendarId",
                schema: "Planning",
                table: "Events",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderRoles_RoleId",
                schema: "Security",
                table: "FolderRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PageRoles_RoleId",
                schema: "Security",
                table: "PageRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_AppId",
                schema: "Security",
                table: "Roles",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                schema: "Security",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DefaultCultureId",
                schema: "Security",
                table: "Users",
                column: "DefaultCultureId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskDataItems_ScheduledTaskId",
                schema: "Tasks",
                table: "ScheduledTaskDataItems",
                column: "ScheduledTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTasks_AppId",
                schema: "Tasks",
                table: "ScheduledTasks",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTasks_ScheduleId",
                schema: "Tasks",
                table: "ScheduledTasks",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CalendarId",
                schema: "Tasks",
                table: "Schedules",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProcesses_AppId",
                schema: "Workflow",
                table: "BusinessProcesses",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProcessWorkflows_BusinessProcessId",
                schema: "Workflow",
                table: "BusinessProcessWorkflows",
                column: "BusinessProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowInstances_FlowDefinitionId",
                schema: "Workflow",
                table: "FlowInstances",
                column: "FlowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEvents_FlowId",
                schema: "Workflow",
                table: "WorkflowEvents",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEvents_ProcessId",
                schema: "Workflow",
                table: "WorkflowEvents",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_AppId",
                schema: "Workflow",
                table: "WorkFlows",
                column: "AppId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditDataItems",
                schema: "Audit");

            migrationBuilder.DropTable(
                name: "AppCultures",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Components",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Contents",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Layouts",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "MetaItems",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "PageInfo",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Resources",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Submissions",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Templates",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "FileContents",
                schema: "DMS");

            migrationBuilder.DropTable(
                name: "LogDataItems",
                schema: "Logging");

            migrationBuilder.DropTable(
                name: "EmailSendFailures",
                schema: "Mail");

            migrationBuilder.DropTable(
                name: "SentEmails",
                schema: "Mail");

            migrationBuilder.DropTable(
                name: "PackageItems",
                schema: "Packaging");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "Planning");

            migrationBuilder.DropTable(
                name: "FolderRoles",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "PageRoles",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "Privileges",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "ScheduledTaskDataItems",
                schema: "Tasks");

            migrationBuilder.DropTable(
                name: "BusinessProcessWorkflows",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "FlowInstances",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "WorkflowEvents",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "AuditEntries",
                schema: "Audit");

            migrationBuilder.DropTable(
                name: "Forms",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Files",
                schema: "DMS");

            migrationBuilder.DropTable(
                name: "LogEntries",
                schema: "Logging");

            migrationBuilder.DropTable(
                name: "QueuedEmails",
                schema: "Mail");

            migrationBuilder.DropTable(
                name: "Packages",
                schema: "Packaging");

            migrationBuilder.DropTable(
                name: "Pages",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "ScheduledTasks",
                schema: "Tasks");

            migrationBuilder.DropTable(
                name: "WorkFlows",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "BusinessProcesses",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "Folders",
                schema: "DMS");

            migrationBuilder.DropTable(
                name: "Cultures",
                schema: "CMS");

            migrationBuilder.DropTable(
                name: "Schedules",
                schema: "Tasks");

            migrationBuilder.DropTable(
                name: "Calendars",
                schema: "Planning");

            migrationBuilder.DropTable(
                name: "Apps",
                schema: "CMS");
        }
    }
}
