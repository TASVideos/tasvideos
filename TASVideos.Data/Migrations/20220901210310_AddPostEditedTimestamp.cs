using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class AddPostEditedTimestamp : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<DateTime>(
			name: "post_edited_timestamp",
			table: "forum_posts",
			type: "timestamp without time zone",
			nullable: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "post_edited_timestamp",
			table: "forum_posts");
	}
}
