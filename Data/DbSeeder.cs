using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            foreach (var role in new[] { "admin", "trainer", "member" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await SeedAdmin(userManager);
            await SeedTrainers(userManager, context);
            await SeedPlans(context);
            await SeedEvents(context);
            await SeedGroupClasses(context);
            await SeedShop(context);
        }

        private static async Task SeedAdmin(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByEmailAsync("admin@fitnessclub.ro") != null) return;

            var admin = new ApplicationUser
            {
                UserName = "admin@fitnessclub.ro",
                Email = "admin@fitnessclub.ro",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "admin");
        }

        private static async Task SeedTrainers(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            var trainersData = new[]
            {
                new { Email = "ion.popescu@fitnessclub.ro",    First = "Ion",    Last = "Popescu",   Expertise = "Fitness & Forță",               Desc = "Specialist în antrenamente de forță și hipertrofie cu peste 8 ani experiență. Lucrează cu sportivi de toate nivelurile.", Years = 8,  Price = 150m, Days = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday } },
                new { Email = "maria.ionescu@fitnessclub.ro",  First = "Maria",  Last = "Ionescu",   Expertise = "Yoga & Pilates",                 Desc = "Instructor certificat de yoga și pilates, pasionată de echilibrul minte-corp și recuperare activă.",                    Years = 5,  Price = 120m, Days = new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Saturday } },
                new { Email = "alex.dumitru@fitnessclub.ro",   First = "Alex",   Last = "Dumitru",   Expertise = "Cardio & HIIT",                  Desc = "Antrenor specializat în antrenamente cardio de intensitate ridicată și programe de slăbire.",                           Years = 6,  Price = 130m, Days = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday } },
                new { Email = "ioana.constantin@fitnessclub.ro", First = "Ioana", Last = "Constantin", Expertise = "Zumba & Dans Fitness",          Desc = "Instructor de dans fitness și zumba cu energie contagioasă. Clasele ei sunt mereu pline de entuziasm și muzică bună.", Years = 4,  Price = 110m, Days = new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Saturday } },
                new { Email = "radu.marian@fitnessclub.ro",    First = "Radu",   Last = "Marian",    Expertise = "CrossFit & Antrenament Funcțional", Desc = "Antrenor CrossFit certificat Level 2. Specializat în mișcări funcționale, mobilitate și condiție fizică generală.",  Years = 7,  Price = 140m, Days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday } }
            };

            foreach (var t in trainersData)
            {
                if (await userManager.FindByEmailAsync(t.Email) != null) continue;

                var user = new ApplicationUser
                {
                    UserName = t.Email,
                    Email = t.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Trainer123!");
                if (!result.Succeeded) continue;

                await userManager.AddToRoleAsync(user, "trainer");

                var trainer = new Trainer
                {
                    UserId = user.Id,
                    FirstName = t.First,
                    LastName = t.Last,
                    Expertise = t.Expertise,
                    Description = t.Desc,
                    YearsOfExperience = t.Years,
                    SessionPrice = t.Price,
                    IsActive = true
                };
                context.Trainers.Add(trainer);
                await context.SaveChangesAsync();

                foreach (var day in t.Days)
                {
                    context.TrainerAvailabilities.Add(new TrainerAvailability
                    {
                        TrainerId = trainer.Id,
                        DayOfWeek = day,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(17, 0)
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPlans(AppDbContext context)
        {
            var plans = new[]
            {
                new SubscriptionPlan
                {
                    Name = "Basic",
                    Price = 99,
                    DurationDays = 30,
                    Description = "Acces la sală în intervalul orelor de program. Ideal pentru antrenamente individuale.",
                    IncludesGroupClasses = false,
                    IsActive = true,
                    Type = SubscriptionPlanType.Subscription
                },
                new SubscriptionPlan
                {
                    Name = "Standard",
                    Price = 179,
                    DurationDays = 30,
                    Description = "Acces nelimitat la sală + participare la toate clasele de grup incluse. Cea mai populară alegere.",
                    IncludesGroupClasses = true,
                    IsActive = true,
                    Type = SubscriptionPlanType.Subscription
                },
                new SubscriptionPlan
                {
                    Name = "Premium",
                    Price = 299,
                    DurationDays = 30,
                    Description = "Tot ce include Standard + o sesiune gratuită cu antrenor personal pe lună + prioritate la rezervări.",
                    IncludesGroupClasses = true,
                    IsActive = true,
                    Type = SubscriptionPlanType.Subscription
                },
                
                
                new SubscriptionPlan
                {
                    Name = "Student Basic",
                    Price = 69,
                    DurationDays = 30,
                    Description = "Acces lunar la sală la preț redus pentru studenți. Este necesar carnetul de student valabil — prezintă-l la recepție la prima vizită.",
                    IncludesGroupClasses = false,
                    IsActive = true,
                    Type = SubscriptionPlanType.Subscription
                },
                new SubscriptionPlan
                {
                    Name = "Student Standard",
                    Price = 129,
                    DurationDays = 30,
                    Description = "Acces nelimitat + clase de grup incluse, la preț special pentru studenți. Prezintă carnetul de student valabil la recepție pentru activarea abonamentului.",
                    IncludesGroupClasses = true,
                    IsActive = true,
                    Type = SubscriptionPlanType.Subscription
                }
            };

            foreach (var plan in plans)
            {
                if (!await context.SubscriptionPlans.AnyAsync(p => p.Name == plan.Name))
                    context.SubscriptionPlans.Add(plan);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedEvents(AppDbContext context)
        {
            var eventNames = new[]
            {
                "Fitness Challenge 2026", "Yoga Retreat Weekend", "Seminar Nutriție Sportivă",
                "CrossFit Open Day", "Workshop Mindfulness & Fitness", "Maraton Spinning",
                "Ziua Analizei Corporale", "Workshop Meal Prep & Nutriție"
            };

            foreach (var name in eventNames)
            {
                if (await context.Events.AnyAsync(e => e.Name == name)) continue;

                var ev = name switch
                {
                    "Fitness Challenge 2026" => new Event
                    {
                        Name = name,
                        Description = "Competiție de fitness deschisă tuturor membrilor. Probe de forță, rezistență și agilitate. Premii pentru primii 3 clasați.",
                        Date = DateTime.Today.AddDays(14), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(18, 0),
                        Location = "Sala Principală", Price = 50, MaxParticipants = 30, IsActive = true, Status = EventStatus.Upcoming
                    },
                    "Yoga Retreat Weekend" => new Event
                    {
                        Name = name,
                        Description = "Weekend de relaxare și reconectare. Sesiuni de yoga, meditație și nutriție sănătoasă. Include prânz organic.",
                        Date = DateTime.Today.AddDays(21), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0),
                        Location = "Studio Yoga", Price = 120, MaxParticipants = 15, IsActive = true, Status = EventStatus.Upcoming
                    },
                    "Seminar Nutriție Sportivă" => new Event
                    {
                        Name = name,
                        Description = "Seminar interactiv cu nutriționist certificat. Află cum să îți optimizezi alimentația pentru performanță maximă.",
                        Date = DateTime.Today.AddDays(7), StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(17, 0),
                        Location = "Sala de Conferințe", Price = 30, MaxParticipants = 50, IsActive = true, Status = EventStatus.Upcoming
                    },
                    "CrossFit Open Day" => new Event
                    {
                        Name = name,
                        Description = "Zi deschisă dedicată CrossFit-ului. Încearcă WOD-uri pentru toate nivelurile, ghidat de antrenorii noștri certificați. Echipament inclus.",
                        Date = DateTime.Today.AddDays(10), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0),
                        Location = "Sala Funcțională", Price = 20, MaxParticipants = 25, IsActive = true, Status = EventStatus.Upcoming
                    },
                    "Workshop Mindfulness & Fitness" => new Event
                    {
                        Name = name,
                        Description = "Combină meditația cu mișcarea. Workshop de 3 ore despre cum să integrezi mindfulness-ul în rutina de antrenament pentru rezultate mai bune.",
                        Date = DateTime.Today.AddDays(28), StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(14, 0),
                        Location = "Studio Yoga", Price = 45, MaxParticipants = 20, IsActive = true, Status = EventStatus.Upcoming
                    },
                    "Maraton Spinning" => new Event
                    {
                        Name = name,
                        Description = "6 ore de cycling non-stop cu 4 instructori în rotație. Muzică live, premii și multă adrenalină. Strângem fonduri pentru echipament nou.",
                        Date = DateTime.Today.AddDays(35), StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(14, 0),
                        Location = "Sala de Cycling", Price = 60, MaxParticipants = 20, IsActive = true, Status = EventStatus.Upcoming
                    },
                    "Ziua Analizei Corporale" => new Event
                    {
                        Name = name,
                        Description = "Analiză completă a compoziției corporale (grăsime, masă musculară, apă) cu aparatură profesională InBody. Include consultație cu antrenorul.",
                        Date = DateTime.Today.AddDays(5), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(16, 0),
                        Location = "Recepție", Price = 35, MaxParticipants = 40, IsActive = true, Status = EventStatus.Upcoming
                    },
                    "Workshop Meal Prep & Nutriție" => new Event
                    {
                        Name = name,
                        Description = "Află cum să îți pregătești mesele pentru o săptămână întreagă în 2 ore. Rețete fitness delicioase, calcul macronutrienți și sfaturi practice.",
                        Date = DateTime.Today.AddDays(42), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 30),
                        Location = "Sala de Conferințe", Price = 40, MaxParticipants = 30, IsActive = true, Status = EventStatus.Upcoming
                    },
                    _ => null
                };

                if (ev != null)
                {
                    context.Events.Add(ev);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedGroupClasses(AppDbContext context)
        {
            var trainers = await context.Trainers.ToListAsync();
            if (trainers.Count == 0) return;

            Trainer? TrainerByEmail(string email) => trainers.FirstOrDefault(t =>
                context.Users.Any(u => u.Id == t.UserId && u.Email == email));

            var classesData = new[]
            {
                new { Name = "HIIT Power",        TrainerEmail = "ion.popescu@fitnessclub.ro",       Duration = 45, Included = true,  Desc = "Antrenament de intensitate ridicată, 45 minute de ars calorii și creștere musculară." },
                new { Name = "Yoga Flow",          TrainerEmail = "maria.ionescu@fitnessclub.ro",     Duration = 60, Included = true,  Desc = "Yoga pentru toate nivelurile. Focus pe flexibilitate, echilibru și respirație conștientă." },
                new { Name = "Cardio Blast",       TrainerEmail = "alex.dumitru@fitnessclub.ro",      Duration = 50, Included = true,  Desc = "Clasă energică de cardio cu muzică motivațională. Arzi până la 600 kcal pe ședință." },
                new { Name = "Pilates Core",       TrainerEmail = "maria.ionescu@fitnessclub.ro",     Duration = 55, Included = true,  Desc = "Pilates focusat pe zona core. Îmbunătățești postura, reduci durerile de spate și tonifiezi abdomenul." },
                new { Name = "Zumba Party",        TrainerEmail = "ioana.constantin@fitnessclub.ro",  Duration = 50, Included = true,  Desc = "Dansezi și arzi calorii fără să simți că faci sport. Energie maximă garantată!" },
                new { Name = "CrossFit Basics",    TrainerEmail = "radu.marian@fitnessclub.ro",       Duration = 60, Included = false, Desc = "Introducere în mișcările fundamentale CrossFit: squat, deadlift, pull-up, kettlebell." },
                new { Name = "Body Pump",          TrainerEmail = "ion.popescu@fitnessclub.ro",       Duration = 55, Included = true,  Desc = "Antrenament cu greutăți pe muzică. Tonifiere completă corp, 800 repetări pe ședință." },
                new { Name = "Stretch & Recover",  TrainerEmail = "maria.ionescu@fitnessclub.ro",     Duration = 45, Included = true,  Desc = "Clasă de stretching și recuperare activă. Esențială după antrenamente intense, reduce DOMS." }
            };

            var startHours = new[] { 9, 10, 11, 17, 18, 19, 9, 10 };

            for (int i = 0; i < classesData.Length; i++)
            {
                var c = classesData[i];
                if (await context.GroupClasses.AnyAsync(g => g.Name == c.Name)) continue;

                var trainer = trainers.FirstOrDefault() ?? trainers[0];
                var trainerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == c.TrainerEmail);
                if (trainerUser != null)
                {
                    var found = trainers.FirstOrDefault(t => t.UserId == trainerUser.Id);
                    if (found != null) trainer = found;
                }

                var groupClass = new GroupClass
                {
                    TrainerId = trainer.Id,
                    Name = c.Name,
                    Description = c.Desc,
                    DurationMinutes = c.Duration,
                    IncludedInSubscription = c.Included,
                    IsActive = true
                };
                context.GroupClasses.Add(groupClass);
                await context.SaveChangesAsync();

                var locations = new[] { "Sala Principală", "Studio Yoga", "Sala Principală", "Studio Yoga", "Sala Principală", "Sala Funcțională", "Sala Principală", "Studio Yoga" };
                for (int week = 0; week < 3; week++)
                {
                    context.GroupClassSchedules.Add(new GroupClassSchedule
                    {
                        GroupClassId = groupClass.Id,
                        Date = DateTime.Today.AddDays((i % 5) + 1 + week * 7),
                        StartTime = new TimeOnly(startHours[i], 0),
                        Location = locations[i],
                        MaxParticipants = 20,
                        CurrentParticipants = 0,
                        Status = ScheduleStatus.Scheduled
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedShop(AppDbContext context)
        {
            foreach (var catName in new[] { "Suplimente", "Echipament", "Accesorii", "Îmbrăcăminte" })
            {
                if (!await context.ProductCategories.AnyAsync(c => c.Name == catName))
                    context.ProductCategories.Add(new ProductCategory { Name = catName });
            }
            await context.SaveChangesAsync();

            var supl = await context.ProductCategories.FirstAsync(c => c.Name == "Suplimente");
            var ech  = await context.ProductCategories.FirstAsync(c => c.Name == "Echipament");
            var acc  = await context.ProductCategories.FirstAsync(c => c.Name == "Accesorii");
            var imb  = await context.ProductCategories.FirstAsync(c => c.Name == "Îmbrăcăminte");

            var imageMap = new Dictionary<string, string>
            {
                { "Whey Protein 1kg",              "/uploads/products/Whey_Protein_1kg.jpeg" },
                { "Pre-Workout Energizant",         "/uploads/products/Pre-Workout_Energizant.webp" },
                { "BCAA 300g",                     "/uploads/products/BCAA_300g.jpeg" },
                { "Creatină Monohidrat 500g",       "/uploads/products/Creatina_Monohidrat_500g.jpeg" },
                { "Batoane Proteice (12 buc)",      "/uploads/products/Batoane_Proteice_12buc.webp" },
                { "Omega-3 Fish Oil 90 caps",       "/uploads/products/Omega-3_Fish_Oil_90_caps.webp" },
                { "Multivitamine Sport 60 tabs",    "/uploads/products/Multivitamine_Sport_60_tabs.jpeg" },
                { "Mănuși Fitness",                 "/uploads/products/Manusi_Fitness.jpeg" },
                { "Set Benzi Elastice (3 nivele)",  "/uploads/products/Set_Benzi_Elastice_3_nivele.webp" },
                { "Coarda de Sărit",                "/uploads/products/Coarda_de_Sarit.jpeg" },
                { "Foam Roller Pro",                "/uploads/products/Foam_Roller_Pro.jpeg" },
                { "Set Gantere Reglabile 2×10kg",   "/uploads/products/Set_Gantere_Reglabile_2x10kg.webp" },
                { "Bidon Sport BPA-Free 750ml",     "/uploads/products/Bidon_Sport_BPA-Free_750ml.jpeg" },
                { "Geantă Sport 40L",               "/uploads/products/Geanta_Sport_40L.jpeg" },
                { "Shaker Proteic 700ml",           "/uploads/products/Shaker_Proteic_700ml.jpeg" },
                { "Saltea Yoga 183×61cm",           "/uploads/products/Saltea_Yoga_183x61cm.webp" },
                { "Tricou Fitness Dry-Fit",         "/uploads/products/Tricou_Fitness_Dry-Fit.jpeg" },
                { "Leggings Sport Femei",           "/uploads/products/Leggings_Sport_Femei.jpeg" },
                { "Shorts Antrenament Bărbați",     "/uploads/products/Shorts_Antrenament_Barbati.jpeg" },
                { "Top Sport Femei",                "/uploads/products/Top_Sport_Femei.jpeg" },
                { "Hanorac Oversize Unisex",        "/uploads/products/Hanorac_Oversize_Unisex.jpeg" },
                { "Jambiere Compresie Bărbați",     "/uploads/products/Jambiere_Compresie_Barbati.jpeg" },
            };

            var products = new[]
            {
                new Product { CategoryId = supl.Id, Name = "Whey Protein 1kg",            Description = "Proteină din zer de înaltă calitate, 24g proteină per porție. Aromă ciocolată. Digestie rapidă, ideală post-antrenament.",  Price = 120, Stock = 50, IsActive = true },
                new Product { CategoryId = supl.Id, Name = "Pre-Workout Energizant",       Description = "Formula avansată cu cafeină, beta-alanină și citrulină. Crește forța, rezistența și focusul mental. 30 de porții.",           Price = 85,  Stock = 40, IsActive = true },
                new Product { CategoryId = supl.Id, Name = "BCAA 300g",                   Description = "Aminoacizi esențiali cu lanț ramificat în raport 2:1:1. Reduce oboseala musculară și susține recuperarea. Aromă fructe.",       Price = 70,  Stock = 60, IsActive = true },
                new Product { CategoryId = supl.Id, Name = "Creatină Monohidrat 500g",     Description = "Creatină pură micronizată, fără arome sau aditivi. Crește forța maximă și volumul muscular. 100 de porții.",                  Price = 60,  Stock = 45, IsActive = true },
                new Product { CategoryId = supl.Id, Name = "Batoane Proteice (12 buc)",    Description = "Batoane cu 20g proteină, gust de ciocolată cu caramel. Fără zahăr adăugat, bogate în fibre. Perfect ca gustare sănătoasă.",   Price = 55,  Stock = 80, IsActive = true },
                new Product { CategoryId = supl.Id, Name = "Omega-3 Fish Oil 90 caps",     Description = "Ulei de pește purificat cu EPA și DHA. Susține sănătatea cardiovasculară, articulațiile și funcția creierului.",              Price = 50,  Stock = 55, IsActive = true },
                new Product { CategoryId = supl.Id, Name = "Multivitamine Sport 60 tabs",  Description = "Complex complet de vitamine și minerale formulat pentru sportivi. Include vitaminele A, C, D, E, B-complex și zinc.",          Price = 45,  Stock = 70, IsActive = true },

                new Product { CategoryId = ech.Id,  Name = "Mănuși Fitness",               Description = "Mănuși cu protecție palmă din piele sintetică. Ventilate, cu velcro reglabil. Potrivite pentru sala de forță.",              Price = 45,  Stock = 35, IsActive = true },
                new Product { CategoryId = ech.Id,  Name = "Set Benzi Elastice (3 nivele)", Description = "Set de 3 benzi de rezistență (ușoară, medie, puternică). Latex natural, durabile. Perfecte pentru acasă sau sală.",           Price = 65,  Stock = 50, IsActive = true },
                new Product { CategoryId = ech.Id,  Name = "Coarda de Sărit",               Description = "Coarda cu mânere ergonomice și rulmenți pentru rotație rapidă. Lungime reglabilă. Ideală pentru cardio și box.",              Price = 30,  Stock = 40, IsActive = true },
                new Product { CategoryId = ech.Id,  Name = "Foam Roller Pro",               Description = "Roller de spumă dură pentru masaj miofascial și recuperare musculară. Suprafață cu reliefuri pentru stimulare mai profundă.", Price = 90,  Stock = 25, IsActive = true },
                new Product { CategoryId = ech.Id,  Name = "Set Gantere Reglabile 2×10kg",  Description = "Set de gantere ajustabile de la 2 la 10 kg fiecare. Construcție solidă din fontă. Include suport de depozitare.",            Price = 280, Stock = 15, IsActive = true },

                new Product { CategoryId = acc.Id,  Name = "Bidon Sport BPA-Free 750ml",   Description = "Bidon sport din tritan fără BPA. Capac răsucibil cu sorbetă, imprimat cu scală de măsurare. Rezistent la căderi.",             Price = 35,  Stock = 60, IsActive = true },
                new Product { CategoryId = acc.Id,  Name = "Geantă Sport 40L",              Description = "Geantă duffel rezistentă la apă cu compartiment separat pentru încălțăminte. Multiple buzunare, curea de umăr reglabilă.",    Price = 150, Stock = 20, IsActive = true },
                new Product { CategoryId = acc.Id,  Name = "Shaker Proteic 700ml",          Description = "Shaker cu grilă mixtoare și capac etanș. Marcat la 200/400/600ml. Compatibil cu mașina de spălat.",                           Price = 25,  Stock = 75, IsActive = true },
                new Product { CategoryId = acc.Id,  Name = "Saltea Yoga 183×61cm",          Description = "Saltea antiderapantă 6mm grosime. Material eco-friendly, suprafață texturată pe ambele fețe. Include geantă de transport.",   Price = 110, Stock = 18, IsActive = true },

                new Product { CategoryId = imb.Id,  Name = "Tricou Fitness Dry-Fit",        Description = "Tricou tehnic din poliester cu tehnologie Dry-Fit. Evacuează transpirația rapid, material ușor și respirabil. Disponibil S-XXL.", Price = 65, Stock = 40, IsActive = true },
                new Product { CategoryId = imb.Id,  Name = "Leggings Sport Femei",          Description = "Leggings de înaltă compresie cu talie înaltă. Material cu 4 căi de elasticitate, opac, rezistent la squats. Buzunar lateral.", Price = 120, Stock = 30, IsActive = true },
                new Product { CategoryId = imb.Id,  Name = "Shorts Antrenament Bărbați",    Description = "Pantaloni scurți cu dublă căptușeală și buzunare cu fermoar. Uscare rapidă, talie elastică cu șiret reglabil.",               Price = 75,  Stock = 35, IsActive = true },
                new Product { CategoryId = imb.Id,  Name = "Top Sport Femei",               Description = "Top cu sutien sport integrat, suport mediu-ridicat. Bretele reglabile, material cu compresie ușoară. Potrivit pentru yoga și pilates.", Price = 90, Stock = 25, IsActive = true },
                new Product { CategoryId = imb.Id,  Name = "Hanorac Oversize Unisex",       Description = "Hanorac din fleece cu glugă și buzunar kangaroo. Croială relaxed, potrivit după antrenament sau pentru activități casual.",     Price = 160, Stock = 20, IsActive = true },
                new Product { CategoryId = imb.Id,  Name = "Jambiere Compresie Bărbați",    Description = "Jambiere de compresie pentru suport muscular optim. Reduc oboseala la efort prelungit și accelerează recuperarea.",              Price = 85,  Stock = 28, IsActive = true },
            };

            foreach (var product in products)
            {
                if (!await context.Products.AnyAsync(p => p.Name == product.Name))
                {
                    if (imageMap.TryGetValue(product.Name, out var url))
                        product.ImageUrl = url;
                    context.Products.Add(product);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
