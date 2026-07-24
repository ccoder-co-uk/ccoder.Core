// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Security;
using cCoder.Data.Models.Workflow;
using FileEntity = cCoder.Data.Models.DMS.File;

using Web.AcceptanceTests.Infrastructure;
namespace Web.AcceptanceTests.Infrastructure;

internal static class CoreDataContextTypedTestExtensions
{
    public static async Task<App> AddAppAsync(this CoreDataContext core, App app)
    {
        App entity = (await core.Apps.AddAsync(entity: app)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<Role> AddRoleAsync(this CoreDataContext core, Role role)
    {
        Role entity = (await core.Roles.AddAsync(entity: role)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<UserRole> AddUserRoleAsync(this CoreDataContext core, UserRole userRole)
    {
        UserRole entity = (await core.UserRoles.AddAsync(entity: userRole)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<User> AddUserAsync(this CoreDataContext core, User user)
    {
        User entity = (await core.Users.AddAsync(entity: user)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<Privilege> AddPrivilegeAsync(this CoreDataContext core, Privilege privilege)
    {
        Privilege entity = (await core.Privileges.AddAsync(entity: privilege)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<Culture> AddCultureAsync(this CoreDataContext core, Culture culture)
    {
        Culture entity = (await core.Cultures.AddAsync(entity: culture)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<AppCulture> AddAppCultureAsync(this CoreDataContext core, AppCulture appCulture)
    {
        AppCulture entity = (await core.AppCultures.AddAsync(entity: appCulture)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<CommonObject> AddCommonObjectAsync(this CoreDataContext core, CommonObject commonObject)
    {
        CommonObject entity = (await core.CommonObjects.AddAsync(entity: commonObject)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<Layout> AddLayoutAsync(this CoreDataContext core, Layout layout)
    {
        Layout entity = (await core.Layouts.AddAsync(entity: layout)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<Page> AddPageAsync(this CoreDataContext core, Page page)
    {
        Page entity = (await core.Pages.AddAsync(entity: page)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<PageInfo> AddPageInfoAsync(this CoreDataContext core, PageInfo pageInfo)
    {
        PageInfo entity = (await core.PageInfo.AddAsync(entity: pageInfo)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<Content> AddContentAsync(this CoreDataContext core, Content content)
    {
        Content entity = (await core.Contents.AddAsync(entity: content)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<PageRole> AddPageRoleAsync(this CoreDataContext core, PageRole pageRole)
    {
        PageRole entity = (await core.PageRoles.AddAsync(entity: pageRole)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<Folder> AddFolderAsync(this CoreDataContext core, Folder folder)
    {
        Folder entity = (await core.Folders.AddAsync(entity: folder)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<FolderRole> AddFolderRoleAsync(this CoreDataContext core, FolderRole folderRole)
    {
        FolderRole entity = (await core.FolderRoles.AddAsync(entity: folderRole)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<FileEntity> AddFileAsync(this CoreDataContext core, FileEntity file)
    {
        FileEntity entity = (await core.Files.AddAsync(entity: file)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static Task<FileEntity> AddDmsFileAsync(this CoreDataContext core, FileEntity file) =>
        core.AddFileAsync(file: file);

    public static async Task<FileContent> AddFileContentAsync(this CoreDataContext core, FileContent fileContent)
    {
        FileContent entity = (await core.FileContents.AddAsync(entity: fileContent)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<MailServer> AddMailServerAsync(this CoreDataContext core, MailServer mailServer)
    {
        MailServer entity = (await core.MailServers.AddAsync(entity: mailServer)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<QueuedEmail> AddQueuedEmailAsync(this CoreDataContext core, QueuedEmail queuedEmail)
    {
        QueuedEmail entity = (await core.QueuedMail.AddAsync(entity: queuedEmail)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<SentEmail> AddSentEmailAsync(this CoreDataContext core, SentEmail sentEmail)
    {
        SentEmail entity = (await core.SentMail.AddAsync(entity: sentEmail)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<ScheduledTask> AddScheduledTaskAsync(this CoreDataContext core, ScheduledTask scheduledTask)
    {
        ScheduledTask entity = (await core.ScheduledTasks.AddAsync(entity: scheduledTask)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<Calendar> AddCalendarAsync(this CoreDataContext core, Calendar calendar)
    {
        Calendar entity = (await core.Calendars.AddAsync(entity: calendar)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static Task<Calendar> AddPlanningCalendarAsync(this CoreDataContext core, Calendar calendar) =>
        core.AddCalendarAsync(calendar: calendar);

    public static async Task<CalendarEvent> AddCalendarEventAsync(this CoreDataContext core, CalendarEvent calendarEvent)
    {
        CalendarEvent entity = (await core.Events.AddAsync(entity: calendarEvent)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<FlowDefinition> AddFlowDefinitionAsync(this CoreDataContext core, FlowDefinition flowDefinition)
    {
        FlowDefinition entity = (await core.FlowDefinitions.AddAsync(entity: flowDefinition)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static Task<FlowDefinition> AddAppFlowDefinitionAsync(this CoreDataContext core, FlowDefinition flowDefinition) =>
        core.AddFlowDefinitionAsync(flowDefinition: flowDefinition);

    public static async Task<FlowInstanceData> AddFlowInstanceDataAsync(this CoreDataContext core, FlowInstanceData flowInstanceData)
    {
        FlowInstanceData entity = (await core.FlowInstances.AddAsync(entity: flowInstanceData)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task<WorkflowEvent> AddWorkflowEventAsync(this CoreDataContext core, WorkflowEvent workflowEvent)
    {
        WorkflowEvent entity = (await core.WorflowEvents.AddAsync(entity: workflowEvent)).Entity;
        _ = await core.SaveChangesAsync();
        return entity;
    }

    public static async Task DeleteAsync(this CoreDataContext core, App app)
    {
        core.Apps.Remove(entity: app);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, Role role)
    {
        core.Roles.Remove(entity: role);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, User user)
    {
        core.Users.Remove(entity: user);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, Privilege privilege)
    {
        core.Privileges.Remove(entity: privilege);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, Layout layout)
    {
        core.Layouts.Remove(entity: layout);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, Page page)
    {
        core.Pages.Remove(entity: page);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, Folder folder)
    {
        core.Folders.Remove(entity: folder);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, FileEntity file)
    {
        core.Files.Remove(entity: file);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, FlowDefinition flowDefinition)
    {
        core.FlowDefinitions.Remove(entity: flowDefinition);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAsync(this CoreDataContext core, Calendar calendar)
    {
        core.Calendars.Remove(entity: calendar);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<UserRole> userRoles)
    {
        UserRole[] items = userRoles.ToArray();
        core.UserRoles.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<Role> roles)
    {
        Role[] items = roles.ToArray();
        core.Roles.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<AppCulture> appCultures)
    {
        AppCulture[] items = appCultures.ToArray();
        core.AppCultures.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<Culture> cultures)
    {
        Culture[] items = cultures.ToArray();
        core.Cultures.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<CommonObject> commonObjects)
    {
        CommonObject[] items = commonObjects.ToArray();
        core.CommonObjects.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<Content> contents)
    {
        Content[] items = contents.ToArray();
        core.Contents.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<PageInfo> pageInfos)
    {
        PageInfo[] items = pageInfos.ToArray();
        core.PageInfo.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<PageRole> pageRoles)
    {
        PageRole[] items = pageRoles.ToArray();
        core.PageRoles.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<Page> pages)
    {
        Page[] items = pages.ToArray();
        core.Pages.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<FolderRole> folderRoles)
    {
        FolderRole[] items = folderRoles.ToArray();
        core.FolderRoles.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<FileContent> fileContents)
    {
        FileContent[] items = fileContents.ToArray();
        core.FileContents.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<FileEntity> files)
    {
        FileEntity[] items = files.ToArray();
        core.Files.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<Folder> folders)
    {
        Folder[] items = folders.ToArray();
        core.Folders.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<MailServer> mailServers)
    {
        MailServer[] items = mailServers.ToArray();
        core.MailServers.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<EmailSendFailure> sendFailures)
    {
        EmailSendFailure[] items = sendFailures.ToArray();
        core.SendFailures.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<QueuedEmail> queuedEmails)
    {
        QueuedEmail[] items = queuedEmails.ToArray();
        core.QueuedMail.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<SentEmail> sentEmails)
    {
        SentEmail[] items = sentEmails.ToArray();
        core.SentMail.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<CalendarEvent> calendarEvents)
    {
        CalendarEvent[] items = calendarEvents.ToArray();
        core.Events.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<Calendar> calendars)
    {
        Calendar[] items = calendars.ToArray();
        core.Calendars.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<ScheduledTask> scheduledTasks)
    {
        ScheduledTask[] items = scheduledTasks.ToArray();
        core.ScheduledTasks.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<FlowInstanceData> flowInstances)
    {
        FlowInstanceData[] items = flowInstances.ToArray();
        core.FlowInstances.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<FlowDefinition> flowDefinitions)
    {
        FlowDefinition[] items = flowDefinitions.ToArray();
        core.FlowDefinitions.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }

    public static async Task DeleteAllAsync(this CoreDataContext core, IEnumerable<WorkflowEvent> workflowEvents)
    {
        WorkflowEvent[] items = workflowEvents.ToArray();
        core.WorflowEvents.RemoveRange(entities: items);
        _ = await core.SaveChangesAsync();
    }
}