using Microsoft.EntityFrameworkCore.Migrations;
using WeddingShare.Enums;

namespace WeddingShare.EntityFramework.Migrations
{
    public partial class InitializeDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// SETTINGS
            
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(maxLength: 1000, nullable: false),
                    Value = table.Column<string>(maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Key",
                table: "Settings",
                column: "Key",
                unique: true);

            /// USERS
            
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(maxLength: 10, nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 200, nullable: false),
                    Firstname = table.Column<string>(maxLength: 50, nullable: false),
                    Lastname = table.Column<string>(maxLength: 50, nullable: false),
                    Password = table.Column<string>(maxLength: 500, nullable: false),
                    MultiFactorAuthToken = table.Column<string>(maxLength: 2000, nullable: true),
                    ActionAuthCode = table.Column<string>(maxLength: 2000, nullable: true),
                    Level = table.Column<UserLevel?>(nullable: false),
                    State = table.Column<AccountState?>(nullable: false),
                    FailedLoginCount = table.Column<int>(nullable: false),
                    LockoutUntil = table.Column<long?>(nullable: true),
                    CreatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                table: "Users",
                column: "EmailAddress",
                unique: true);

            /// GALLERIES

            migrationBuilder.CreateTable(
                name: "Galleries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    Identifier = table.Column<string>(maxLength: 32, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    SecretKey = table.Column<string>(maxLength: 500, nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    IsSecure = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_Identifier",
                table: "Galleries",
                column: "Identifier",
                unique: true);

            /// GALLERY SETTINGS

            migrationBuilder.CreateTable(
                name: "GallerySettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    GalleryId = table.Column<int>(nullable: false),
                    SettingId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
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

            /// GALLERY ITEMS

            migrationBuilder.CreateTable(
                name: "GalleryItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    GalleryId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 100, nullable: false),
                    UploadedBy = table.Column<string>(maxLength: 100, nullable: false),
                    Checksum = table.Column<string>(maxLength: 1000, nullable: true),
                    FileSize = table.Column<int>(nullable: false),
                    State = table.Column<GalleryItemState>(nullable: false),
                    Type = table.Column<MediaType>(nullable: false),
                    Orientation = table.Column<ImageOrientation>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
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

            /// GALLERY LIKES

            migrationBuilder.CreateTable(
                name: "GalleryLikes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    GalleryId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
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
                    table.ForeignKey(
                        name: "FK_GalleryItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            /// CUSTOM RESOURCES

            migrationBuilder.CreateTable(
                name: "CustomResources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    Filename = table.Column<string>(maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomResources_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            /// AUDIT LOGS

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    Message = table.Column<string>(maxLength: 2000, nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Severity = table.Column<AuditSeverity>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "CustomResources");
            migrationBuilder.DropTable(name: "GalleryLikes");
            migrationBuilder.DropTable(name: "GalleryItems");
            migrationBuilder.DropTable(name: "GallerySettings");
            migrationBuilder.DropTable(name: "Galleries");
            migrationBuilder.DropTable(name: "Users");
            migrationBuilder.DropTable(name: "Settings");
        }
    }
}