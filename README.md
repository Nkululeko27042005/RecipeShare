# RecipE - Shared Community Cookbook

RecipE is a modern, community-driven recipe sharing web application built with **ASP.NET MVC 5**, **Entity Framework 6 Code First**, and **ASP.NET Identity 2.0**. It features a cookbook-inspired aesthetic, real-time interactive feeds, a smart recommendation engine, multi-image upload galleries, step-by-step cooking guides, and user profile customization.

---

## Features

### 1. Culinary Feed & Recommendation Engine
- **Smart Recommendations:** Personalized feed based on your category preferences (e.g. Baking, Dessert, Main Course) and your social follow network.
- **Feed Filtering:** Switch between **All** recipes and **Following** to keep up with dishes from cooks you trust.
- **Sidebars & Rails:** Discover trending content through **Top Liked Dishes**, **Rising Cooks**, and quick access to your **Saved Recipes**.
- **Keyword Search:** Find dishes and ingredients using the navigation search bar.

### 2. Rich Recipe Creation & Display
- **Dynamic Ingredient Builder:** Clear distinction between:
  - **Ingredient Name** (e.g., *Brown Sugar*)
  - **Quantity** (e.g., *2*, *1/2*)
  - **Unit** (e.g., *cups*, *tbsp*, *cloves*)
  - **Weight** (e.g., *250g*, *100ml*)
  - Interactive prep checkboxes for home cooks to cross off ingredients while cooking.
- **Interactive Method:** Auto-renumbering step-by-step cooking instructions.
- **Media Uploads:** Drag-and-drop multi-photo gallery preview with cover photo selection and optional dish video.
- **Cook's Notes:** Discussion section on recipe detail pages with real-time comments.
- **Social Interaction:** Real-time like, dislike, and one-click saving to your personal cookbook.

### 3. Profile & Account Customization
- **Profile Photo Uploads:** Upload and customize your chef avatar with instant preview.
- **Chef Bio & Preferences:** Update your public display name, culinary bio, and favorite cooking categories at any time from your settings.
- **Public Profile Pages:** Explore any cook's public profile, follower counts, and dish catalog, with instant follow/unfollow capabilities.

---

## Tech Stack

- **Framework:** ASP.NET MVC 5 (.NET Framework 4.8.1)
- **Data Access:** Entity Framework 6.1 (Code-First)
- **Database:** Microsoft SQL Server / LocalDB (`MSSQLLocalDB`)
- **Authentication:** ASP.NET Identity 2.0 with OWIN Cookie Authentication
- **Frontend:** 
  - Razor Views (`.cshtml`)
  - Tailwind CSS (via CDN) + Custom Cookbook Theme
  - Vanilla JavaScript & AJAX (`fetch` API with Anti-Forgery CSRF integration)
  - Bootstrap 5 (grid & modal components)

---

## Project Structure

```
RecipeShare/
├── App_Start/
│   ├── BundleConfig.cs          # Script & style bundles
│   ├── FilterConfig.cs          # Global MVC filters
│   ├── IdentityConfig.cs        # ApplicationUserManager & SignInManager setup
│   ├── RouteConfig.cs           # URL routing definitions
│   └── Startup.Auth.cs          # OWIN Cookie authentication pipeline
├── Controllers/
│   ├── AccountController.cs     # Auth, registration, profile settings & avatar upload
│   ├── DashboardController.cs   # Feed data & personalized recommendations
│   ├── HomeController.cs        # Landing page & public showcase
│   ├── RecipeController.cs       # Recipe CRUD, ratings, comments, saves, search
│   └── UserController.cs         # Profiles & user follow graph
├── Migrations/
│   └── Configuration.cs         # Code-First database migration & seed logic
├── Models/
│   ├── ApplicationUser.cs       # Identity user extension (Bio, Avatar, Preferences)
│   ├── AppplicationDbContext.cs  # EF DbContext with cascade delete & index mappings
│   ├── Recipe.cs                # Recipe, Ingredient, Step, Image, Rating, Save, Follow
│   ├── Repositories/            # Generic and specialized repositories
│   ├── Services/                # RecipeService, RecommendationService, FileService, FollowService
│   └── ViewModels/              # Strongly-typed models for auth, creation, and showcase
├── Scripts/
│   └── app.js                   # Frontend helper library (CSRF, toast notifications, time formatting)
├── Views/
│   ├── Account/                 # Login, Register, Profile & Preferences
│   ├── Dashboard/               # Main culinary feed
│   ├── Home/                    # Landing page
│   ├── Recipe/                  # Create, Details, My Recipes, Saved, Top Liked, Search
│   ├── Shared/                  # _Layout, _Nav, _Footer, Error (404)
│   └── User/                    # Public profile & Rising cooks
└── Web.config                   # Connection strings, runtime limits & security config
```

---

## Getting Started

### Prerequisites
- Visual Studio 2022 / 2026 with the **ASP.NET and web development** workload.
- **SQL Server LocalDB** (included with Visual Studio) or SQL Server Express.
- .NET Framework 4.8.1 Developer Pack.

### Running the Project
1. **Clone or Open:** Open `RecipeShare.sln` in Visual Studio.
2. **Restore NuGet Packages:** Right-click the Solution and select **Restore NuGet Packages**.
3. **Database Setup:** 
   - The connection string in `Web.config` is configured for LocalDB:
     ```xml
     <connectionStrings>
       <add name="DefaultConnection" connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=RecipeShare;Integrated Security=True;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
     </connectionStrings>
     ```
   - Automatic migrations will initialize the database and populate initial demo data (Chef Marie with an initial recipe) on first run.
4. **Build & Run:** Press `F5` or `Ctrl + F5` to build and launch the application in IIS Express.

---

## Default Demo Credentials

On first run, the database seeds an initial demo chef:
- **Email:** `marie@kitchen.com`
- **Password:** `password123`
