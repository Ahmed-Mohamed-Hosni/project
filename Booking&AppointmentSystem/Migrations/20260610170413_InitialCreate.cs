using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _4._Booking_AppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProviderSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderSchedules_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BookingCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeSlot = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProviderServices",
                columns: table => new
                {
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderServices", x => new { x.ProviderId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_ProviderServices_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Icon", "Name" },
                values: new object[,]
                {
                    { 1, "Professional clinical and medical consultations", "heart-pulse", "Healthcare & Clinics" },
                    { 2, "Premium styling, cosmetics, and relaxation therapies", "scissors", "Beauty Salons & Spa" },
                    { 3, "Professional strategy meetings and career coaching", "briefcase", "Business Consulting" },
                    { 4, "Therapeutic bodywork, yoga, and meditation sessions", "flower1", "Holistic Wellness" }
                });

            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Bio", "Email", "ImagePath", "Name", "Phone", "Rating", "Title" },
                values: new object[,]
                {
                    { 1, "Dr. Sarah holds a PhD in Cardiovascular Sciences from Johns Hopkins, boasting over 12 years of clinical research and practice.", "dr.sarah@bookingsystem.com", "/images/provider-1.jpg", "Dr. Sarah Jenkins", "+1 555-0101", 4.9000000000000004, "Senior Cardiologist" },
                    { 2, "Dr. Ahmed specializes in advanced implantology and aesthetic smile makeovers with over 8 years in digital dentistry.", "dr.ahmed@bookingsystem.com", "/images/provider-2.jpg", "Dr. Ahmed Mansour", "+1 555-0102", 4.7999999999999998, "Specialist Cosmetic Dentist" },
                    { 3, "Maria is a certified master colorist, trained in Paris, renowned for creating personalized shades and modern haircuts.", "maria.l@bookingsystem.com", "/images/provider-3.jpg", "Maria Lopez", "+1 555-0201", 4.7000000000000002, "Master Hair Stylist & Colorist" },
                    { 4, "John is an ICF-certified coach helping professionals scale business startups and unlock their career potentials.", "john.d@bookingsystem.com", "/images/provider-4.jpg", "John Davis", "+1 555-0301", 5.0, "Executive Life & Business Coach" },
                    { 5, "Yasmine has studied eastern relaxation therapies in Bali, specializing in trigger point release and hot stone body wellness.", "yasmine.e@bookingsystem.com", "/images/provider-5.jpg", "Yasmine El-Amin", "+1 555-0401", 4.9000000000000004, "Holistic Therapist & Masseur" }
                });

            migrationBuilder.InsertData(
                table: "ProviderSchedules",
                columns: new[] { "Id", "DayOfWeek", "EndTime", "IsActive", "ProviderId", "StartTime" },
                values: new object[,]
                {
                    { 1, 1, new TimeSpan(0, 17, 0, 0, 0), true, 1, new TimeSpan(0, 9, 0, 0, 0) },
                    { 2, 2, new TimeSpan(0, 17, 0, 0, 0), true, 1, new TimeSpan(0, 9, 0, 0, 0) },
                    { 3, 3, new TimeSpan(0, 17, 0, 0, 0), true, 1, new TimeSpan(0, 9, 0, 0, 0) },
                    { 4, 4, new TimeSpan(0, 17, 0, 0, 0), true, 1, new TimeSpan(0, 9, 0, 0, 0) },
                    { 5, 5, new TimeSpan(0, 17, 0, 0, 0), true, 1, new TimeSpan(0, 9, 0, 0, 0) },
                    { 6, 1, new TimeSpan(0, 17, 0, 0, 0), true, 2, new TimeSpan(0, 9, 0, 0, 0) },
                    { 7, 2, new TimeSpan(0, 17, 0, 0, 0), true, 2, new TimeSpan(0, 9, 0, 0, 0) },
                    { 8, 3, new TimeSpan(0, 17, 0, 0, 0), true, 2, new TimeSpan(0, 9, 0, 0, 0) },
                    { 9, 4, new TimeSpan(0, 17, 0, 0, 0), true, 2, new TimeSpan(0, 9, 0, 0, 0) },
                    { 10, 5, new TimeSpan(0, 17, 0, 0, 0), true, 2, new TimeSpan(0, 9, 0, 0, 0) },
                    { 11, 1, new TimeSpan(0, 17, 0, 0, 0), true, 3, new TimeSpan(0, 9, 0, 0, 0) },
                    { 12, 2, new TimeSpan(0, 17, 0, 0, 0), true, 3, new TimeSpan(0, 9, 0, 0, 0) },
                    { 13, 3, new TimeSpan(0, 17, 0, 0, 0), true, 3, new TimeSpan(0, 9, 0, 0, 0) },
                    { 14, 4, new TimeSpan(0, 17, 0, 0, 0), true, 3, new TimeSpan(0, 9, 0, 0, 0) },
                    { 15, 5, new TimeSpan(0, 17, 0, 0, 0), true, 3, new TimeSpan(0, 9, 0, 0, 0) },
                    { 16, 1, new TimeSpan(0, 17, 0, 0, 0), true, 4, new TimeSpan(0, 9, 0, 0, 0) },
                    { 17, 2, new TimeSpan(0, 17, 0, 0, 0), true, 4, new TimeSpan(0, 9, 0, 0, 0) },
                    { 18, 3, new TimeSpan(0, 17, 0, 0, 0), true, 4, new TimeSpan(0, 9, 0, 0, 0) },
                    { 19, 4, new TimeSpan(0, 17, 0, 0, 0), true, 4, new TimeSpan(0, 9, 0, 0, 0) },
                    { 20, 5, new TimeSpan(0, 17, 0, 0, 0), true, 4, new TimeSpan(0, 9, 0, 0, 0) },
                    { 21, 1, new TimeSpan(0, 17, 0, 0, 0), true, 5, new TimeSpan(0, 9, 0, 0, 0) },
                    { 22, 2, new TimeSpan(0, 17, 0, 0, 0), true, 5, new TimeSpan(0, 9, 0, 0, 0) },
                    { 23, 3, new TimeSpan(0, 17, 0, 0, 0), true, 5, new TimeSpan(0, 9, 0, 0, 0) },
                    { 24, 4, new TimeSpan(0, 17, 0, 0, 0), true, 5, new TimeSpan(0, 9, 0, 0, 0) },
                    { 25, 5, new TimeSpan(0, 17, 0, 0, 0), true, 5, new TimeSpan(0, 9, 0, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "CategoryId", "Description", "DurationMinutes", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, "Comprehensive medical consultation and general checkup.", 30, "General Physician Checkup", 60.00m },
                    { 2, 1, "Plaque removal, teeth polishing, and deep hygiene assessment.", 45, "Professional Dental Cleaning", 90.00m },
                    { 3, 1, "Advanced ECG test and cardiologist specialist evaluation.", 60, "Cardiovascular Consultation", 180.00m },
                    { 4, 2, "Custom haircut, wash, styling, and hot towel finish.", 30, "Signature Men's Haircut", 35.00m },
                    { 5, 2, "High-end hair coloring, toning, deep conditioning, and blow dry.", 120, "Women's Balayage & Styling", 150.00m },
                    { 6, 2, "Deep cleansing, exfoliation, and hydration infusion.", 60, "HydraFacial Skin Treatment", 95.00m },
                    { 7, 3, "1-on-1 strategic growth and scale planning session.", 60, "Corporate Strategy Consult", 250.00m },
                    { 8, 3, "Resume audit, interview Prep, and leadership path coaching.", 45, "Executive Career Coaching", 120.00m },
                    { 9, 4, "Focus on releasing chronic muscle tension and body knots.", 60, "Deep Tissue Muscle Massage", 85.00m },
                    { 10, 4, "Heated basalt stones massage to melt away stress and fatigue.", 75, "Premium Hot Stone Therapy", 110.00m }
                });

            migrationBuilder.InsertData(
                table: "ProviderServices",
                columns: new[] { "ProviderId", "ServiceId" },
                values: new object[,]
                {
                    { 1, 3 },
                    { 2, 1 },
                    { 2, 2 },
                    { 3, 4 },
                    { 3, 5 },
                    { 3, 6 },
                    { 4, 7 },
                    { 4, 8 },
                    { 5, 9 },
                    { 5, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ProviderId",
                table: "Appointments",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSchedules_ProviderId",
                table: "ProviderSchedules",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderServices_ServiceId",
                table: "ProviderServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CategoryId",
                table: "Services",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "ProviderSchedules");

            migrationBuilder.DropTable(
                name: "ProviderServices");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
