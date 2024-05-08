using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace cCoder.Core.Migrations;

public partial class ScheduledTasksReWork : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ScheduledTasks_Schedules_ScheduleId",
            schema: "Tasks",
            table: "ScheduledTasks");

        migrationBuilder.DropTable(
            name: "ScheduledTaskDataItems",
            schema: "Tasks");

        migrationBuilder.DropTable(
            name: "Schedules",
            schema: "Tasks");

        migrationBuilder.DropIndex(
            name: "IX_ScheduledTasks_ScheduleId",
            schema: "Tasks",
            table: "ScheduledTasks");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "schedule_admin");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "schedule_create");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "schedule_delete");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "schedule_read");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "schedule_update");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "scheduledtaskdataitem_admin");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "scheduledtaskdataitem_create");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "scheduledtaskdataitem_delete");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "scheduledtaskdataitem_read");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "scheduledtaskdataitem_update");

        migrationBuilder.DropColumn(
            name: "HandlingEndpointUrl",
            schema: "Tasks",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "ScheduleId",
            schema: "Tasks",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "Type",
            schema: "Tasks",
            table: "ScheduledTasks");

        migrationBuilder.RenameTable(
            name: "ScheduledTasks",
            schema: "Tasks",
            newName: "ScheduledTasks",
            newSchema: "Planning");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LastExecuted",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: true,
            oldClrType: typeof(DateTimeOffset));

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "Created",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ExecuteAs",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ExecutionArgs",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "FlowId",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastUpdated",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "NextExecution",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: true);

        migrationBuilder.AddColumn<TimeSpan>(
            name: "Schedule",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            schema: "Planning",
            table: "ScheduledTasks",
            nullable: true);

        migrationBuilder.InsertData(
            schema: "Security",
            table: "Privileges",
            columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
            values: new object[] { "scheduledtask_execute", "Allows users to call Execute on ScheduledTasks.", "Execute", false, "ScheduledTask" });

        migrationBuilder.InsertData(
            schema: "Security",
            table: "Privileges",
            columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
            values: new object[] { "flowdefinition_execute", "Allows users to call Execute on FlowDefinitions.", "Execute", false, "FlowDefinition" });

        migrationBuilder.InsertData(
            schema: "Security",
            table: "Privileges",
            columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
            values: new object[] { "flowdefinition_resume", "Allows users to call Resume on FlowDefinitions.", "Resume", false, "FlowDefinition" });

        migrationBuilder.CreateIndex(
            name: "IX_ScheduledTasks_FlowId",
            schema: "Planning",
            table: "ScheduledTasks",
            column: "FlowId");

        migrationBuilder.AddForeignKey(
            name: "FK_ScheduledTasks_WorkFlows_FlowId",
            schema: "Planning",
            table: "ScheduledTasks",
            column: "FlowId",
            principalSchema: "Workflow",
            principalTable: "WorkFlows",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ScheduledTasks_WorkFlows_FlowId",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropIndex(
            name: "IX_ScheduledTasks_FlowId",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "flowdefinition_execute");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "flowdefinition_resume");

        migrationBuilder.DeleteData(
            schema: "Security",
            table: "Privileges",
            keyColumn: "Id",
            keyValue: "scheduledtask_execute");

        migrationBuilder.DropColumn(
            name: "Created",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "ExecuteAs",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "ExecutionArgs",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "FlowId",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "LastUpdated",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "NextExecution",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "Schedule",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            schema: "Planning",
            table: "ScheduledTasks");

        migrationBuilder.EnsureSchema(
            name: "Tasks");

        migrationBuilder.RenameTable(
            name: "ScheduledTasks",
            schema: "Planning",
            newName: "ScheduledTasks",
            newSchema: "Tasks");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LastExecuted",
            schema: "Tasks",
            table: "ScheduledTasks",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "HandlingEndpointUrl",
            schema: "Tasks",
            table: "ScheduledTasks",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "ScheduleId",
            schema: "Tasks",
            table: "ScheduledTasks",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Type",
            schema: "Tasks",
            table: "ScheduledTasks",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "ScheduledTaskDataItems",
            schema: "Tasks",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Key = table.Column<string>(nullable: false),
                ScheduledTaskId = table.Column<int>(nullable: false),
                Value = table.Column<string>(nullable: false)
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

        migrationBuilder.InsertData(
            schema: "Security",
            table: "Privileges",
            columns: new[] { "Id", "Description", "Operation", "PortalAdminsOnly", "Type" },
            values: new object[,]
            {
                { "schedule_create", "Allows users to Create Schedules.", "Create", false, "Schedule" },
                { "schedule_read", "Allows users to Read Schedules.", "Read", false, "Schedule" },
                { "schedule_update", "Allows users to Update Schedules.", "Update", false, "Schedule" },
                { "schedule_delete", "Allows users to Delete Schedules.", "Delete", false, "Schedule" },
                { "schedule_admin", "Allows users to Administer Schedules.", "Admin", false, "Schedule" },
                { "scheduledtaskdataitem_create", "Allows users to Create ScheduledTaskDataItems.", "Create", false, "ScheduledTaskDataItem" },
                { "scheduledtaskdataitem_read", "Allows users to Read ScheduledTaskDataItems.", "Read", false, "ScheduledTaskDataItem" },
                { "scheduledtaskdataitem_update", "Allows users to Update ScheduledTaskDataItems.", "Update", false, "ScheduledTaskDataItem" },
                { "scheduledtaskdataitem_delete", "Allows users to Delete ScheduledTaskDataItems.", "Delete", false, "ScheduledTaskDataItem" },
                { "scheduledtaskdataitem_admin", "Allows users to Administer ScheduledTaskDataItems.", "Admin", false, "ScheduledTaskDataItem" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_ScheduledTasks_ScheduleId",
            schema: "Tasks",
            table: "ScheduledTasks",
            column: "ScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ScheduledTaskDataItems_ScheduledTaskId",
            schema: "Tasks",
            table: "ScheduledTaskDataItems",
            column: "ScheduledTaskId");

        migrationBuilder.CreateIndex(
            name: "IX_Schedules_CalendarId",
            schema: "Tasks",
            table: "Schedules",
            column: "CalendarId");

        migrationBuilder.AddForeignKey(
            name: "FK_ScheduledTasks_Schedules_ScheduleId",
            schema: "Tasks",
            table: "ScheduledTasks",
            column: "ScheduleId",
            principalSchema: "Tasks",
            principalTable: "Schedules",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
