using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OficinaMecanica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPhase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    document = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_parts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    sku = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_parts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_services_catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    estimated_hours = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_services_catalog", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    brand = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    model = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_vehicles_tb_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "tb_customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_roles_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_roles_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_roles_claims_tb_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "tb_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_users_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_users_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_users_claims_tb_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_users_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_users_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_tb_users_logins_tb_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_users_roles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_users_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_tb_users_roles_tb_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "tb_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_users_roles_tb_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_users_tokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_users_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "FK_tb_users_tokens_tb_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_service_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    customer_request = table.Column<List<string>>(type: "text[]", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    diagnostic_started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    budget_sent_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    execution_started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finished_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_service_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_service_orders_tb_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "tb_customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_service_orders_tb_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "tb_vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_service_order_part_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    part_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    is_additional = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    additional_request_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_service_order_part_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_service_order_part_items_tb_service_orders_service_order~",
                        column: x => x.service_order_id,
                        principalTable: "tb_service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_service_order_service_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_catalog_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    estimated_hours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    is_additional = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    additional_request_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_service_order_service_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_service_order_service_items_tb_service_orders_service_or~",
                        column: x => x.service_order_id,
                        principalTable: "tb_service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_customers_document",
                table: "tb_customers",
                column: "document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_outbox_messages_processed_at_utc_created_at_utc",
                table: "tb_outbox_messages",
                columns: new[] { "processed_at_utc", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_parts_sku",
                table: "tb_parts",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "tb_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_roles_claims_role_id",
                table: "tb_roles_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_service_order_part_items_service_order_id",
                table: "tb_service_order_part_items",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_service_order_service_items_service_order_id",
                table: "tb_service_order_service_items",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_service_orders_customer_id",
                table: "tb_service_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_service_orders_number",
                table: "tb_service_orders",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_service_orders_vehicle_id",
                table: "tb_service_orders",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "tb_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "tb_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_users_claims_user_id",
                table: "tb_users_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_users_logins_user_id",
                table: "tb_users_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_users_roles_role_id",
                table: "tb_users_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_vehicles_customer_id",
                table: "tb_vehicles",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_vehicles_plate",
                table: "tb_vehicles",
                column: "plate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_outbox_messages");

            migrationBuilder.DropTable(
                name: "tb_parts");

            migrationBuilder.DropTable(
                name: "tb_roles_claims");

            migrationBuilder.DropTable(
                name: "tb_service_order_part_items");

            migrationBuilder.DropTable(
                name: "tb_service_order_service_items");

            migrationBuilder.DropTable(
                name: "tb_services_catalog");

            migrationBuilder.DropTable(
                name: "tb_users_claims");

            migrationBuilder.DropTable(
                name: "tb_users_logins");

            migrationBuilder.DropTable(
                name: "tb_users_roles");

            migrationBuilder.DropTable(
                name: "tb_users_tokens");

            migrationBuilder.DropTable(
                name: "tb_service_orders");

            migrationBuilder.DropTable(
                name: "tb_roles");

            migrationBuilder.DropTable(
                name: "tb_users");

            migrationBuilder.DropTable(
                name: "tb_vehicles");

            migrationBuilder.DropTable(
                name: "tb_customers");
        }
    }
}
