using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RecipeShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace RecipeShare.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<RecipeShare.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(RecipeShare.Models.ApplicationDbContext context)
        {
            // Seed a demo user if database is empty
            if (!context.Users.Any())
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true
                };
                userManager.PasswordValidator = new PasswordValidator
                {
                    RequiredLength = 6,
                    RequireNonLetterOrDigit = false,
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireUppercase = false
                };

                var demoUser = new ApplicationUser
                {
                    UserName = "marie@kitchen.com",
                    Email = "marie@kitchen.com",
                    DisplayName = "Marie",
                    Bio = "Pastry chef, home cook, lover of burnt honey and fresh rosemary.",
                    PreferredCategoriesCsv = "Dessert,Baking,Main Course"
                };

                var result = userManager.Create(demoUser, "password123");
                if (result.Succeeded)
                {
                    var recipe = new Recipe
                    {
                        Title = "Burnt Honey & Rosemary Cake",
                        Description = "A sticky, fragrant cake that tastes like a Sunday afternoon. Best served warm with cream.",
                        Category = "Dessert",
                        AuthorId = demoUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<RecipeImage>
                        {
                            new RecipeImage
                            {
                                ImageUrl = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=800&auto=format&fit=crop&q=80",
                                IsPrimary = true
                            }
                        },
                        Ingredients = new List<Ingredient>
                        {
                            new Ingredient { Name = "Wildflower honey", Quantity = "1", Unit = "cup" },
                            new Ingredient { Name = "Fresh rosemary (finely chopped)", Quantity = "2", Unit = "tbsp" },
                            new Ingredient { Name = "All-purpose flour", Quantity = "2", Unit = "cups", Weight = "250g" },
                            new Ingredient { Name = "Unsalted butter (browned)", Quantity = "1/2", Unit = "cup", Weight = "115g" },
                            new Ingredient { Name = "Large eggs", Quantity = "3", Unit = "pcs" }
                        },
                        Steps = new List<RecipeStep>
                        {
                            new RecipeStep { StepNumber = 1, Instruction = "Preheat your oven to 175 C (350 F) and grease an 8-inch round cake pan." },
                            new RecipeStep { StepNumber = 2, Instruction = "Gently caramelize the honey in a saucepan until it turns a deep amber, then stir in the finely chopped rosemary." },
                            new RecipeStep { StepNumber = 3, Instruction = "Whisk browned butter with eggs and folded flour until just combined, pour into the pan, and bake for 35 minutes until golden." }
                        }
                    };

                    context.Recipes.Add(recipe);
                    context.SaveChanges();
                }
            }
        }
    }
}
