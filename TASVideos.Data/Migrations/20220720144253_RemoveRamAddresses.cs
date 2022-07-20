using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RemoveRamAddresses : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "game_ram_addresses");

		migrationBuilder.DropTable(
			name: "game_ram_address_domains");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "game_ram_address_domains",
			columns: table => new
			{
				id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				game_system_id = table.Column<int>(type: "integer", nullable: true),
				name = table.Column<string>(type: "citext", maxLength: 255, nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_game_ram_address_domains", x => x.id);
				table.ForeignKey(
					name: "fk_game_ram_address_domains_game_systems_game_system_id",
					column: x => x.game_system_id,
					principalTable: "game_systems",
					principalColumn: "id");
			});

		migrationBuilder.CreateTable(
			name: "game_ram_addresses",
			columns: table => new
			{
				id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				game_id = table.Column<int>(type: "integer", nullable: true),
				game_ram_address_domain_id = table.Column<int>(type: "integer", nullable: false),
				system_id = table.Column<int>(type: "integer", nullable: false),
				address = table.Column<long>(type: "bigint", nullable: false),
				description = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
				endian = table.Column<int>(type: "integer", nullable: false),
				legacy_game_name = table.Column<string>(type: "citext", maxLength: 255, nullable: true),
				legacy_set_id = table.Column<int>(type: "integer", nullable: false),
				signed = table.Column<int>(type: "integer", nullable: false),
				type = table.Column<int>(type: "integer", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_game_ram_addresses", x => x.id);
				table.ForeignKey(
					name: "fk_game_ram_addresses_game_ram_address_domains_game_ram_addres",
					column: x => x.game_ram_address_domain_id,
					principalTable: "game_ram_address_domains",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_game_ram_addresses_game_systems_system_id",
					column: x => x.system_id,
					principalTable: "game_systems",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_game_ram_addresses_games_game_id",
					column: x => x.game_id,
					principalTable: "games",
					principalColumn: "id");
			});

		migrationBuilder.CreateIndex(
			name: "ix_game_ram_address_domains_game_system_id",
			table: "game_ram_address_domains",
			column: "game_system_id");

		migrationBuilder.CreateIndex(
			name: "ix_game_ram_addresses_game_id",
			table: "game_ram_addresses",
			column: "game_id");

		migrationBuilder.CreateIndex(
			name: "ix_game_ram_addresses_game_ram_address_domain_id",
			table: "game_ram_addresses",
			column: "game_ram_address_domain_id");

		migrationBuilder.CreateIndex(
			name: "ix_game_ram_addresses_system_id",
			table: "game_ram_addresses",
			column: "system_id");
	}
}
