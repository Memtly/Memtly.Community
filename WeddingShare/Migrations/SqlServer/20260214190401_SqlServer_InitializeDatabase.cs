using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingShare.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class SqlServer_InitializeDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Firstname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MultiFactorAuthToken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ActionAuthCode = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    State = table.Column<int>(type: "int", nullable: true, defaultValue: 2),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LockoutUntil = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: true, defaultValue: 2),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CustomResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filename = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomResources_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Galleries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identifier = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecretKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Galleries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GalleryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GalleryId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Checksum = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    State = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Orientation = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryItems_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GallerySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GalleryId = table.Column<int>(type: "int", nullable: true),
                    SettingId = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GallerySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GallerySettings_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GallerySettings_Settings_SettingId",
                        column: x => x.SettingId,
                        principalTable: "Settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GalleryLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GalleryItemId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryLikes_GalleryItems_GalleryItemId",
                        column: x => x.GalleryItemId,
                        principalTable: "GalleryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomResources_UserId",
                table: "CustomResources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_Identifier",
                table: "Galleries",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_UserId",
                table: "Galleries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItems_GalleryId",
                table: "GalleryItems",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryLikes_GalleryItemId",
                table: "GalleryLikes",
                column: "GalleryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryLikes_UserId",
                table: "GalleryLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GallerySettings_GalleryId",
                table: "GallerySettings",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_GallerySettings_SettingId",
                table: "GallerySettings",
                column: "SettingId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Key",
                table: "Settings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                table: "Users",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CustomResources");

            migrationBuilder.DropTable(
                name: "GalleryLikes");

            migrationBuilder.DropTable(
                name: "GallerySettings");

            migrationBuilder.DropTable(
                name: "GalleryItems");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Galleries");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
