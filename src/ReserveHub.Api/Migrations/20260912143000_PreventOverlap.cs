using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace ReserveHub.Api.Migrations;
[DbContext(typeof(AppDb))]
[Migration("20260912143000_PreventOverlap")]
public class PreventOverlap:Migration {
 protected override void Up(MigrationBuilder migrationBuilder) {
  migrationBuilder.Sql("""
   CREATE EXTENSION IF NOT EXISTS btree_gist;
   ALTER TABLE "Bookings" ADD CONSTRAINT no_overlapping_bookings
   EXCLUDE USING gist ("RoomId" WITH =, tstzrange("Start", "End", '[)') WITH &&)
   WHERE (NOT "Cancelled");
   """);
 }
 protected override void Down(MigrationBuilder migrationBuilder) {
  migrationBuilder.Sql("ALTER TABLE \"Bookings\" DROP CONSTRAINT no_overlapping_bookings;");
 }
}
