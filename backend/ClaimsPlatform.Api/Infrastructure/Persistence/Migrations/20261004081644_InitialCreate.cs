using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsPlatform.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "claim_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acting_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    claimant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    policy_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    market = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    incident_date = table.Column<DateOnly>(type: "date", nullable: false),
                    incident_location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    reported_loss_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    assessed_loss_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    assigned_officer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    decision_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    settlement_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claims", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "information_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by_officer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    response = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    responded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_information_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_information_requests_claims_claim_id",
                        column: x => x.claim_id,
                        principalTable: "claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    market = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    market = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_claim_history_acting_user_id",
                table: "claim_history",
                column: "acting_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_claim_history_claim_id_occurred_at",
                table: "claim_history",
                columns: new[] { "claim_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_claims_assigned_officer_id",
                table: "claims",
                column: "assigned_officer_id");

            migrationBuilder.CreateIndex(
                name: "ix_claims_claimant_id",
                table: "claims",
                column: "claimant_id");

            migrationBuilder.CreateIndex(
                name: "ux_claims_reference_number",
                table: "claims",
                column: "reference_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_information_requests_claim_id",
                table: "information_requests",
                column: "claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_information_requests_requested_by_officer_id",
                table: "information_requests",
                column: "requested_by_officer_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_manager_id",
                table: "teams",
                column: "manager_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_team_id",
                table: "users",
                column: "team_id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_history_claims_claim_id",
                table: "claim_history",
                column: "claim_id",
                principalTable: "claims",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_history_users_acting_user_id",
                table: "claim_history",
                column: "acting_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claims_users_assigned_officer_id",
                table: "claims",
                column: "assigned_officer_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claims_users_claimant_id",
                table: "claims",
                column: "claimant_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_information_requests_users_requested_by_officer_id",
                table: "information_requests",
                column: "requested_by_officer_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_teams_users_manager_id",
                table: "teams",
                column: "manager_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teams_users_manager_id",
                table: "teams");

            migrationBuilder.DropTable(
                name: "claim_history");

            migrationBuilder.DropTable(
                name: "information_requests");

            migrationBuilder.DropTable(
                name: "claims");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "teams");
        }
    }
}
