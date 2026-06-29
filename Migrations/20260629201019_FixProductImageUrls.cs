using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.Migrations
{
    /// <inheritdoc />
    public partial class FixProductImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update by ID (products added with explicit IDs)
            var byId = new (int Id, string Url)[]
            {
                (1001, "/uploads/products/Whey_Protein_1kg.jpeg"),
                (1002, "/uploads/products/Pre-Workout_Energizant.webp"),
                (1003, "/uploads/products/BCAA_300g.jpeg"),
                (1004, "/uploads/products/Creatina_Monohidrat_500g.jpeg"),
                (1005, "/uploads/products/Batoane_Proteice_12buc.webp"),
                (1006, "/uploads/products/Omega-3_Fish_Oil_90_caps.webp"),
                (1007, "/uploads/products/Multivitamine_Sport_60_tabs.jpeg"),
                (1008, "/uploads/products/Manusi_Fitness.jpeg"),
                (1009, "/uploads/products/Set_Benzi_Elastice_3_nivele.webp"),
                (1010, "/uploads/products/Coarda_de_Sarit.jpeg"),
                (1011, "/uploads/products/Foam_Roller_Pro.jpeg"),
                (1012, "/uploads/products/Set_Gantere_Reglabile_2x10kg.webp"),
                (1013, "/uploads/products/Bidon_Sport_BPA-Free_750ml.jpeg"),
                (1014, "/uploads/products/Geanta_Sport_40L.jpeg"),
                (1015, "/uploads/products/Shaker_Proteic_700ml.jpeg"),
                (1016, "/uploads/products/Saltea_Yoga_183x61cm.webp"),
                (2001, "/uploads/products/Tricou_Fitness_Dry-Fit.jpeg"),
                (2002, "/uploads/products/Leggings_Sport_Femei.jpeg"),
                (2003, "/uploads/products/Shorts_Antrenament_Barbati.jpeg"),
                (2004, "/uploads/products/Top_Sport_Femei.jpeg"),
                (2005, "/uploads/products/Hanorac_Oversize_Unisex.jpeg"),
                (2006, "/uploads/products/Jambiere_Compresie_Barbati.jpeg"),
                (4,    "/uploads/products/creatina_pura.webp"),
            };
            foreach (var (id, url) in byId)
                migrationBuilder.Sql($"UPDATE Products SET ImageUrl = '{url}' WHERE Id = {id}");

            // Update by Name (products added by seeder without explicit IDs)
            var byName = new (string Name, string Url)[]
            {
                ("Whey Protein 1kg",             "/uploads/products/Whey_Protein_1kg.jpeg"),
                ("Pre-Workout Energizant",        "/uploads/products/Pre-Workout_Energizant.webp"),
                ("BCAA 300g",                     "/uploads/products/BCAA_300g.jpeg"),
                ("Creată Monohidrat 500g",  "/uploads/products/Creatina_Monohidrat_500g.jpeg"),
                ("Creatina Monohidrat",           "/uploads/products/creatina_pura.webp"),
                ("Batoane Proteice (12 buc)",     "/uploads/products/Batoane_Proteice_12buc.webp"),
                ("Omega-3 Fish Oil 90 caps",      "/uploads/products/Omega-3_Fish_Oil_90_caps.webp"),
                ("Multivitamine Sport 60 tabs",   "/uploads/products/Multivitamine_Sport_60_tabs.jpeg"),
                ("Mănuși Fitness",      "/uploads/products/Manusi_Fitness.jpeg"),
                ("Set Benzi Elastice (3 nivele)", "/uploads/products/Set_Benzi_Elastice_3_nivele.webp"),
                ("Coarda de Sărit",          "/uploads/products/Coarda_de_Sarit.jpeg"),
                ("Foam Roller Pro",               "/uploads/products/Foam_Roller_Pro.jpeg"),
                ("Set Gantere Reglabile 2×10kg", "/uploads/products/Set_Gantere_Reglabile_2x10kg.webp"),
                ("Bidon Sport BPA-Free 750ml",    "/uploads/products/Bidon_Sport_BPA-Free_750ml.jpeg"),
                ("Geanţă Sport 40L",    "/uploads/products/Geanta_Sport_40L.jpeg"),
                ("Geantă Sport 40L",         "/uploads/products/Geanta_Sport_40L.jpeg"),
                ("Shaker Proteic 700ml",          "/uploads/products/Shaker_Proteic_700ml.jpeg"),
                ("Saltea Yoga 183×61cm",     "/uploads/products/Saltea_Yoga_183x61cm.webp"),
                ("Tricou Fitness Dry-Fit",        "/uploads/products/Tricou_Fitness_Dry-Fit.jpeg"),
                ("Leggings Sport Femei",          "/uploads/products/Leggings_Sport_Femei.jpeg"),
                ("Shorts Antrenament Bărbăţi", "/uploads/products/Shorts_Antrenament_Barbati.jpeg"),
                ("Shorts Antrenament Bărbăți", "/uploads/products/Shorts_Antrenament_Barbati.jpeg"),
                ("Top Sport Femei",               "/uploads/products/Top_Sport_Femei.jpeg"),
                ("Hanorac Oversize Unisex",       "/uploads/products/Hanorac_Oversize_Unisex.jpeg"),
                ("Jambiere Compresie Bărbăţi", "/uploads/products/Jambiere_Compresie_Barbati.jpeg"),
                ("Jambiere Compresie Bărbăți", "/uploads/products/Jambiere_Compresie_Barbati.jpeg"),
            };
            foreach (var (name, url) in byName)
                migrationBuilder.Sql($"UPDATE Products SET ImageUrl = '{url}' WHERE Name = N'{name}'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
