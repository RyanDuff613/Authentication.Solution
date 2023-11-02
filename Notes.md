# Project Roadmap 
## Creating an ASP.NET Core MVC app with MySQL Database and Many-to-Many Relationship:

## Getting Started...
1. Create project directory. Be sure to follow naming conventions. ie: `$ mkdir TodoList.Solution`
2. Navigate into project directory and initialize git 
    - `$ cd ToDoList.Solution`
    - `$ git init`
3. Create Project subdirectory
    - `$ mkdir TodoList`
4. Create `.gitignore` and add list of directories and files git should ignore.
    - `$ touch .gitignore`
      <details><summary><code>ToDoList.Solution/.gitignore</code></summary> 

      ```
      bin
      obj
      appsettings.json
      ```
      </details>
5. Make first commit for `.gitignore` so that Git doesn't track unwanted files.
    - `$ git add .gitignore` 
    - `$ git commit -m "add .gitignore"` 
6. Navigate to project directory
    - `$ cd TodoList` 
7. Create required directories: 
    - `$ mkdir Controllers Models Properties Views wwwroot`
8. Create configuration files: 
    - `$ touch Program.cs ToDoList.csproj appsettings.json Properties/launchSettings.json`
9. Add required content to these files.

      <details><summary><code>ToDoList/ToDoList.csproj</code></summary> 

      ```c#
        <Project Sdk="Microsoft.NET.Sdk.Web">

          <PropertyGroup>
            <TargetFramework>net6.0</TargetFramework>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.0" />
            <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />
            <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.0">
              <IncludeAssets>runtime; build; native; contentfiles; analyzers;buildtransitive</IncludeAssets>
              <PrivateAssets>all</PrivateAssets>
            </PackageReference>
            <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="6.0.0" />
          </ItemGroup>

        </Project>
      ```
      </details>
      
      <details><summary><code>ToDoList/Program.cs</code></summary> 

      ```c#
        using Microsoft.AspNetCore.Builder;
        using Microsoft.EntityFrameworkCore;
        using Microsoft.Extensions.DependencyInjection;
        using ToDoList.Models;
        using Microsoft.AspNetCore.Identity;
        // be sure to change the namespace to match your project
        namespace ToDoList
        {
          class Program
          {
            static void Main(string[] args)
            {
            
              WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

              builder.Services.AddControllersWithViews();
              // be sure to update the line below for your project
              builder.Services.AddDbContext<ToDoListContext>(
                                dbContextOptions => dbContextOptions
                                  .UseMySql(
                                    builder.Configuration["ConnectionStrings:DefaultConnection"],
                                    ServerVersion.AutoDetect(builder.Configuration["ConnectionStrings:DefaultConnection"]
                                  )
                                )
                              );

              // Line below adds Identity
              builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                        .AddEntityFrameworkStores<ToDoListContext>()
                        .AddDefaultTokenProviders();

              // This is where we can determine Password requirements for users.
              builder.Services.Configure<IdentityOptions>(options =>
              {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
              });

              WebApplication app = builder.Build();

              app.UseDeveloperExceptionPage();
              app.UseHttpsRedirection();
              app.UseStaticFiles();

              app.UseRouting();

              // Next two lines below enable authentication and authorization.
              app.UseAuthentication();
              app.UseAuthorization();

              app.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}"
                );

              app.Run();
            }
          }
        }
      ```
      </details>

      <details><summary><code>ToDoList/appsettings.json</code></summary> 

      ```json
      {
        "ConnectionStrings": {
            "DefaultConnection": "Server=localhost;Port=3306;database=[YOUR-DATABASE-NAME];uid=[YOUR-USERNAME];pwd=[YOUR-MySQL-PASSWORD];"
          }
      }
      ```
      </details>

      <details><summary><code>ToDoList/Properties/launchSettings.json</code></summary> 

      ```json
      {
          "profiles": {
            "development": {
            "commandName": "Project",
            "dotnetRunMessages": true,
            "launchBrowser": true,
            "applicationUrl": "https://localhost:5001;http://localhost:5000",
            "environmentVariables": {
              "ASPNETCORE_ENVIRONMENT": "Development"
            }
          },
          "production": {
             "commandName": "Project",
             "dotnetRunMessages": true,
             "launchBrowser": true,
             "applicationUrl": "https://localhost:5001;http://localhost:5000",
             "environmentVariables": {
               "ASPNETCORE_ENVIRONMENT": "Production"
             }
           }
         }
      }
      ```
      </details>

10. Build Models
  - At first, just create the base models required for your app. In this case,I'm building a ToDoList app that just has Items and Tags (Categories areomitted so that we can focus on just the Many-to-Many relationship)

  -  Model Naming Conventions for EF Core:
      - Column names in DB must match property names of Models in the app. These are case-sensitive.
      - For a property to be recognized as a primary key, we need to name theproperty `Id` or `[ClassName]Id`.

  - Examples from ToDoList:
    <details><summary><code>ToDoList/Models/Item.cs</code></summary> 

      ```c#
      namespace ToDoList.Models
      {
       public class Item
       {
         public int ItemId { get; set; } // Primary Key
         public string Description { get; set; }
       }
      }
      ```
    </details>
    <details><summary><code>ToDoList/Models/Tag.cs</code></summary> 

    ```c#
    namespace ToDoList.Models
    {
      public class Tag
      {
        public int TagId { get; set; } // Primary Key
        public string Title { get; set; }
      }
    }
    ```
    </details>

11. Create Database Context for the Models.
    <details><summary><code>ToDoList/Models/ToDoListContext.cs</code></summary> 

    ```c#
      using Microsoft.EntityFrameworkCore;

      namespace ToDoList.Models
      {
        public class ToDoListContext : DbContext
        {
          public DbSet<Item> Items { get; set; }
          public DbSet<Tag> Tags { get; set; }

          public ToDoListContext(DbContextOptions options) : base(options) { }
        }
      }
    ```
    </details>

12. Create migration and update database: 
- `$ dotnet ef migrations add Initial`
-  `$ dotnet ef database update`
- \* Note \* it's best practice to add a migration and update the database everytime you alter a Model.

## CREATING A MANY-TO-MANY RELATIONSHIP BETWEEN TWO MODELS.
1. Create the Join Model that represents the relationship between to Models. Entity uses this model to create the Join Table in the database.
    <details><summary><code>ToDoList/Models/ItemTag.cs</code></summary>

      ```c#
      namespace ToDoList.Models
      {
        public class ItemTag
        {
          public int ItemTagId { get; set; } // Primary Key
          public int ItemId { get; set; } // Foreign Key to the Items table
          public Item Item { get; set; } // Navigation Property for an Item
          public int TagId { get; set; } // Foreign Key to the Tags table
          public Tag Tag { get; set; } // Navigation Property for a Tag
        }
      }
      ```
    </details>

2. Update Database context with new Join Model:

    <details><summary><code>ToDoList/Models/ToDoListContext.cs</code></summary>

      ```c#
      using Microsoft.EntityFrameworkCore;
      namespace ToDoList.Models
      {
        public class ToDoListContext : DbContext
        {
          public DbSet<Item> Items { get; set; }
          public DbSet<Tag> Tags { get; set; }
          public DbSet<ItemTag> ItemTags { get; set; }
          public ToDoListContext(DbContextOptions options) : base(options) { }
        }
      }
      ```
    </details>

3. Add Navigation Properties to each class that reference the Join Entity that represents the relationship in the database.

    <details><summary><code>ToDoList/Models/Item.cs</code></summary>

      ```c#
      using System.Collections.Generic; // Using directed added so that we can create a List
      namespace ToDoList.Models
      {
        public class Item
        {
          public int ItemId { get; set; } // Primary Key
          public string Name { get; set; }
          
          public List<ItemTag> JoinEntities { get; set; } // This is the "collection 
          // navigation property". Note that we don't have to call this property JoinEntities. 
          // We might also call it ItemTags, TaggedItems etc, whatever name you think best represents 
          // the relationship between the two models. 
        }
      }
      ```
    </details>

    <details><summary><code>ToDoList/Models/Tag.cs</code></summary> 

      ```c#
      using System.Collections.Generic; //Using directed added so that we can create a List 
      namespace ToDoList.Models
      {
        public class Tag
        {
          public int TagId { get; set; } // Primary Key
          public string Title { get; set; }
          
          public List<ItemTag> JoinEntities { get; set; } // This is the "collection 
          // navigation property". Note that we don't have to call this property JoinEntities. 
          // We might also call it ItemTags, TaggedItems etc, whatever name you think best represents 
          // the relationship between the two models.
        }
      }
      ```
    </details>

4. Create migration and update database: 
- `$ dotnet ef migrations add Initial`
-  `$ dotnet ef database update`
- \* Note \* it's best practice to add a migration and update the database everytime you alter a Model.

## Updating Controllers and Views to make an association between models
 Next, we need to give users the ability to make an association through the user-interface.

  - Within the `ItemsController` we add a new route to take that delivers a view with a form for adding a `Tag` to an Item:
    <details><summary><code>ToDoList/Controllers/ItemsController.cs</code></summary>
      
      ```c#
        ...
        public ActionResult AddTag(int id)
        {
          Item thisItem = _db.Items.FirstOrDefault(items => items.  ItemId == id);
          ViewBag.TagId = new SelectList(_db.Tags, "TagId",   "Title");
          return View(thisItem);
        }
        ...
      ```
    </details>

  - Our views use the `SelectList` passed down from the controller to create a dropdown selection.  
    <details><summary><code>ToDoList/Views/Items/AddTag.cshtml</code></summary> 
      
      ```c#
        @{
          Layout = "_Layout";
        }

        @model ToDoList.Models.Item

        <h2>Add a tag</h2>

        <h4>Add a tag to this item: @Html.DisplayFor(model => model.Description)</h4>

        @using (Html.BeginForm())
        {
          @Html.HiddenFor(model => model.ItemId)

          @Html.Label("Select tag")
          @Html.DropDownList("TagId")// The SelectList passed from the controller 
          // to the view via ViewBag is used to create a dropdown menu 
          // with an option for a user to select any existing category.

          <input type="submit" value="Save" />
        }

        <p>@Html.ActionLink("Back to list", "Index")</p>
      ```
    </details>

  - A new controller route handles the Post request from the form.  
    <details><summary><code>ToDoList/Controllers/ItemsController.cs</code></summary> 

      ```c#
        [HttpPost]
        public ActionResult AddTag(Item item, int tagId)
        {
          #nullable enable
          ItemTag? joinEntity = _db.ItemTags.FirstOrDefault(join => (join.TagId == tagId && join.ItemId == item.ItemId));
          #nullable disable
          if (joinEntity == null && tagId != 0)
          {
            _db.ItemTags.Add(new ItemTag() { TagId = tagId, ItemId = item.ItemId });
            _db.SaveChanges();
          }
          return RedirectToAction("Details", new { id = item.ItemId });
        }
      ```
    </details>
    - We create a database query with the FirstOrDefault() method that returns the first ItemTag object that contains a matching ItemId and TagId; if a matching ItemTag object can't be found, the default is returned, which is null.

    - Since our joinEntity variable will be either an ItemTag object or null, we need to make it a nullable type. We can turn a type into a nullable type by adding a question mark ? at the end of the type, like ItemTag?.

    - To use nullable reference types in particular, we must also have a nullable annotation context enabled so that our C# compiler can process the nullable reference types. We can enable a nullable annotation context for our entire app via our .csproj file, or for a file or a few lines of code with nullable directives: #nullable enable and #nullable disable. We're opting for the latter in our code because it will require less refactoring across our whole app.

    - To complete the checking process for duplicate join relationships, we simply need to check if joinEntity == null in our conditional. If the result of our search for duplicates is null, it means that we can move forward with creating the new join relationship in our database.

-  See rest of this example repo for references on how to display related entities in a Details page and how to Delete a relationship between two entities.

<hr />

## ADDING AUTHENTICATION AND AUTHORIZATION:
1. Create a User class by Extending an Identity Class
    <details><summary><code>Models/ApplicationUser.cs</code></summary>
      
      ```c#
      using Microsoft.AspNetCore.Identity;
      using System;

      namespace ToDoList.Models
      {
          public class ApplicationUser : IdentityUser
          {
            public string Website { get; set; }
            public string Image { get; set; }
            public DateOnly Birthday { get; set; }
          }
      }
      ```
    </details>

2. Update Database Context Model
    <details><summary><code>Models/ToDoListContext.cs</code></summary>
      
      ```c#
      using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
      using Microsoft.EntityFrameworkCore;

      namespace ToDoList.Models
      {
        public class ToDoListContext : IdentityDbContext<ApplicationUser>
        {
          public DbSet<Item> Items { get; set; }
          public DbSet<Tag> Tags { get; set; }
          public DbSet<ItemTag> ItemTags { get; set; }

          public ToDoListContext(DbContextOptions options) : base(options) { }
        }
      }
      ```
    </details>

3. Create a Migration and Update the Database *Note: do this every time you alter a model
    ```bash
    $ dotnet ef migrations add AddIdentity
    $ dotnet ef database update
    ```

4. Add a ViewModel. This is not required for adding Authentication to an app but we'll do it here to help with our views.

    <details><summary><code>ViewModels/RegisterViewModel.cs</code></summary>
      
      ```c#
      using System.ComponentModel.DataAnnotations;

      namespace ToDoList.ViewModels
      {
        public class RegisterViewModel
        {
          [Required]
          [EmailAddress]
          [Display(Name = "Email Address")]
          public string Email { get; set; }

          [Required]
          [DataType(DataType.Password)]
          [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,}$", ErrorMessage = "Your password must contain at least six characters, a capital letter, a lowercase letter, a number, and a special character.")]
          public string Password { get; set; }

          [Required]
          [DataType(DataType.Password)]
          [Display(Name = "Confirm password")]
          [Compare("Password", ErrorMessage = "The password and confirmation password do      not match.")]
          public string ConfirmPassword { get; set; }
        }
      }
      ```
    </details>

5. Add a Controller for Accounts
    <details><summary><code>Controllers/AccountController.cs</code></summary>
      
      ```c#
      using Microsoft.AspNetCore.Mvc;
      using Microsoft.AspNetCore.Identity;
      using ToDoList.Models;
      using System.Threading.Tasks;
      using ToDoList.ViewModels;

      namespace ToDoList.Controllers
      {
        public class AccountController : Controller
        {
          private readonly ToDoListContext _db;
          private readonly UserManager<ApplicationUser> _userManager;
          private readonly SignInManager<ApplicationUser> _signInManager;

          public AccountController (UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ToDoListContext db)
          {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
          }

          public ActionResult Index()
          {
            return View();
          }

          public IActionResult Register()
          {
            return View();
          }

          [HttpPost]
          public async Task<ActionResult> Register (RegisterViewModel model)
          {
            if (!ModelState.IsValid)
            {
              return View(model);
            }
            else
            {
              ApplicationUser user = new ApplicationUser { UserName = model.Email };
              IdentityResult result = await _userManager.CreateAsync(user, model.Password);
              if (result.Succeeded)
              {
                return RedirectToAction("Index");
              }
              else
              {
                foreach (IdentityError error in result.Errors)
                {
                  ModelState.AddModelError("", error.Description);
                }
                return View(model);
              }
            }
          }
        }
      }
      ```
    </details>

6. Add a Registration View
    <details><summary><code>Views/Account/Register.cshtml</code></summary>
      
      ```c#
      @{
        Layout = "_Layout";
      }

      @using ToDoList.ViewModels

      @model RegisterViewModel

      <h2>Register a new user</h2>
      <hr />

      @if(!ViewData.ModelState.IsValid) 
      {
        @Html.ValidationSummary("Please fix the following error(s) and then resubmit the      form:") 
      }

      @using (Html.BeginForm("Register", "Account", FormMethod.Post))
      {
        @Html.LabelFor(user => user.Email)
        @Html.TextBoxFor(user=> user.Email)

        @Html.LabelFor(user=> user.Password)
        @Html.EditorFor(user=> user.Password)

        @Html.LabelFor(user=> user.ConfirmPassword)
        @Html.EditorFor(user=> user.ConfirmPassword)

        <input type="submit" value="Register" />
      }

      <p>@Html.ActionLink("Back to accounts page", "Index")</p>      
      ```
    </details>

7. Add an Account/Index view.
    <details><summary><code>Views/Account/Index.cshtml</code></summary>
      
      ```c#
      @{
        Layout = "_Layout";
      }

      <h2>Authentication with Identity</h2>
      <hr />
      <p>@Html.ActionLink("Register", "Register")</p>
      <p>@Html.ActionLink("Home", "Index", "Home")</p>
      ```
    </details>

8. Add a link to the Account Index to the homepage.
    <details><summary><code>Views/Home/Index.cshtml</code></summary>
      
      ```c#
      ...
      <p>@Html.ActionLink("Create or manage an account", "Index", "Account")</p>
      ```
    </details>

9. Run app and test registration process. At this point a user should be able to complete the registration form, submit a be directed back to the `Account/Index` page. If you want test further, create a user, and then try to create another user with the exact same email. You should get an error message that says something like "Username 'test@test.com' is already taken".

10. Create Login ViewModel
    <details><summary><code>ViewModels/LoginViewModel.cs</code></summary>

      ```c#
      using System.ComponentModel.DataAnnotations;

      namespace ToDoList.ViewModels
      {
       public class LoginViewModel
       {
         [Required]
         [EmailAddress]
         [Display(Name = "Email Address")]
         public string Email { get; set; }

         [Required]
         [DataType(DataType.Password)]
         public string Password { get; set; }
       }
      }
      ```
    </details>

11. Create Login View
    <details><summary><code>Views/Account/Login.cshtml</code></summary>

      ```c#
      @{
        Layout = "_Layout";
      }

      @using ToDoList.ViewModels

      @model LoginViewModel

      <h2>Log in with your account</h2>
      <hr />

      @if(!ViewData.ModelState.IsValid) 
      {
        @Html.ValidationSummary("Please fix the following error(s) and then resubmit the form:") 
      }

      @using (Html.BeginForm())
      {
        @Html.LabelFor(m => m.Email)
        @Html.TextBoxFor(m => m.Email)

        @Html.LabelFor(m => m.Password)
        @Html.EditorFor(m => m.Password)

        <input type="submit" value="Log in" />
      }

      <p>@Html.ActionLink("Register for an account", "Register")</p>
      <p>@Html.ActionLink("Back to accounts page", "Index")</p>
      ```
    </details>

12. Add Login Actions in the AccountController
    <details><summary><code>AccountController.cs</code></summary>
      
      ```c#
      ...
      public ActionResult Login()
      {
        return View();
      }

      [HttpPost]
      public async Task<ActionResult> Login(LoginViewModel model)
      {
        if (!ModelState.IsValid)
        {
          return View(model);
        }
        else
        {
          Microsoft.AspNetCore.Identity.SignInResult result = await   _signInManager.PasswordSignInAsync(model.Email, model.Password,   isPersistent: true, lockoutOnFailure: false);
          if (result.Succeeded)
          {
            return RedirectToAction("Index");
          }
          else
          {
            ModelState.AddModelError("", "There is something wrong with   your email or username. Please try again.");
            return View(model);
          }
        }
      }
      ...
      ```
    </details>

13. Update Account Index Page to display different info depending on if a user is logged-in or not.
    <details><summary><code>Views/Account/Index.cshtml</code></summary>
      
      ```c#
      @{
        Layout = "_Layout";
      }

      @if (User.Identity.IsAuthenticated)
      {
        <p>Hello @User.Identity.Name!</p>
      }
      else
      {
        <p>@Html.ActionLink("Register", "Register")</p>
        <p>@Html.ActionLink("Log in", "Login")</p>
      }
      <p>@Html.ActionLink("Home", "Index", "Home")</p>
      ```
    </details>

14. Create LogOff action in Account Controller
    <details><summary><code>AccountController.cs</code></summary>
      
      ```c#
      ...
      [HttpPost]
      public async Task<ActionResult> LogOff()
      {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index");
      }
      ...
      ```
    </details>

15. Add form to Account Index view that sends a POST request to Account Controller's LogOff Action
    <details><summary><code>Views/Account/Index.cshtml</code></summary>
      
      ```c#
      @{
        Layout = "_Layout";
      }

      @if (User.Identity.IsAuthenticated)
      {
        <p>Hello @User.Identity.Name!</p>
        @using (Html.BeginForm("LogOff", "Account"))
        {
          <input type="submit" class="btn btn-primary" value="Log out" />
        }
      }
      else
      {
        <p>@Html.ActionLink("Register", "Register")</p>
        <p>@Html.ActionLink("Log in", "Login")</p>
      }
      <p>@Html.ActionLink("Home", "Index", "Home")</p>
      ```
    </details>

16. Run app and test login/logout process. At this point a user should be able to create an account and login/logout and view their own Index page.
    
# ADDITIONAL NOTES:

### HTML HELPER METHODS:
- `@Html.ActionLink` Example:
    - `@Html.ActionLink("See all items", "Index", "Items")`
    - This creates a link. Clicking the link will invoke the "Index" action   in `ItemsController.cs` 
    - The first argument ("See all Items") determines the text that will  appear on the webpage.
    - The second argument ("Index") specifies a controller action.
    - The third argument ("Items") specifies a controller and is optional (defaults to the controller associated with the current View).
    
- `@Html.ActionLink` Example 2:
  - `@Html.ActionLink($"{item.Description}", "Details", new { id = item.ItemId })`
  - Creates a link to the Details action of the controller with the same name as the Views subfolder that contains the view in which the link is rendered. If this link appeared in `Views/Items/Index.cshtml` it would it would invoke the `Details` action in `ItemsController.cs`. If it instead appeared in `Views/Tags/Index.cshtml`, it would invoke the `Details` action in `TagsController.cs`.
  - The code `new { id = item.ItemId }` creates an anonymous object with the property `id`; this is how .NET passes `id` to the `Details()` action. For .NET to route us to the details page for a specific Item, the property names of the object we pass in must match parameter names used by the target controller action. In this case, the `id` property of the anonymous object must match the parameter name in our `public ActionResult Details(int id)` in the `ItemsController`.

### MODEL DIRECTIVES:
  - `@model ToDoList.Models.Item`
    - Tells the view what type of data to expect to be passed from controller.
    - Required whenever using strongly typed HTML Helpers.
    - Only one model directive is allowed in each view.

### STRONGLY TYPED HELPERS:
  - Example:
    <details><summary><code>ToDoList/Views/Items/Create.cshtml</code></summary> 
    
    ```c#
    @using (Html.BeginForm())
    {
      @Html.LabelFor(model => model.Description)
      @Html.TextBoxFor(model => model.Description)
      <input type="submit" value="Add new item" />
    }
    ```
    </details>

  - By default, the form will create a `post` request to the route matching the filename it was called in. In the example above, this would create a `post` request to the `ItemsController` > `Create` action.
  - In the example above `LabelFor()` and `TextBoxFor()` are strongly typed helpers. Strongly typed helpers always have names that end with `For` to remind you that they are for a specific model.
  - `model => model.Description` is a lambda expression. The helpers need them to associate the parts of the form with a Model and its properties.
  - Strongly typed helpers provide error checking at compile time so they are recommended.
  - If you use a strongly typed helper you must include a model directive ie: `@model ToDoList.Models.Item`

### NAVIGATION PROPERTIES:
  - A navigation property is a property on one entity (like Category) that includes a reference to a related entity (like Item). EF Core uses navigation properties to recognize when there is a relationship between two entities.
  - In this case, EF Core sees that the `Tags` have an `JoinEntities` property of the type `List<ItemTags>` and is able to understand that there is a relationship between the `Item` and the `ItemTags` join entity.
  - The `JoinEntities` property is more specifically categorized as a "collection navigation property" because it contains multiple entities. In this case, we have a collection (`List<>`) of multiple `ItemTag` objects.
  - Navigation properties are never saved in the database. Instead, they are populated in our projects by EF Core from the data in the database.

### ViewBag
  - A ViewBag is a tool for sending small amounts of data from a controller to a view. It's just a regular ol' C# object so in a route we can add a key-value pair to the ViewBag object and pass the data into the view similarly to how we pass Model data into a view. Here's a quick example of how you might do that:
    <details><summary><code>MyRandomController.cs</code></summary>

      ```c#
      public ActionResult MyRandomRoute()
      {
        ViewBag.MyFavoriteColor = "green";
        return View();
      }
      ```
    </details>

    <details><summary><code>Views/MyRandomModel/MyRandomRoute.cshtml</code></summary>

      ```c#
        <p>I like the color @ViewBag.MyFavoriteColor.</p>
        // In the browser this would render as:
        // I like the color green. 
      ```
    </details>
  - In the ToDoList we use ViewBag to pass a list of Tags from a controller to a view in order to populate a `SelectList` that allows users to associate `Tags` with `Items` and vice-versa, see below...

### SelectList and @Html.DropDownList()

  - `@Html.DropDownList()` creates an HTML `<select>` dropdown menu and it's `<option>`'s. In order to make this happen we have to provide it with a particular data type, a `SelectList`.
  - We create a `SelectList` in our controllers like this: `SelectList selectList = new SelectList(_db.Tags, "TagId", "Title");`.
    - The first argument specifies the data that will populate our `<select>` dropdown's `<option>`s. In this example, we want to create an `<option>` for every category so we get the entire list of Categories from the database.
    - The second argument determines the `value` property of the every `<option>`.
    - The third argument determines the text of every `<option>` in our dropdown.
    - Altogether, we're saying that we want to create an `<option>` for every category that looks something like this: `<option value="1">HighPriorityTag</option>` 
  - Next we pass the `SelectList` to our view via ViewBag.
  - Lastly in our view, we pass the SelectList object to `@Html.DropDownList()` to create the dropdown menu.

    <details><summary><code>ItemsController.cs</code></summary> 

      ```c#
      public ActionResult AddTag(int id)
      {
        Item thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id);

        SelectList selectList = new SelectList(_db.Tags, "TagId", "Title"); // Create SelectList from the Tags table
        ViewBag.TagId = selectList;// Add a property (TagId) to the ViewBag object 
        // and use it to pass selectList to Views/Items/Create.cshtml
        return View(thisItem);
      }
      ```
    </details>

    <details><summary><code>Views/Items/AddTag.cshtml</code></summary> 

      ```c#
      @{
        Layout = "_Layout";
      }

      @model ToDoList.Models.Item

      <h2>Add a tag</h2>

      <h4>Add a tag to this item: @Html.DisplayFor(model => model.Description)</h4>

      @using (Html.BeginForm())
      {
        @Html.HiddenFor(model => model.ItemId)

        @Html.Label("Select tag")
        @Html.DropDownList("TagId") // Creates the <select> dropdown menu using 
        // the data passed in from the controller: ViewBag.TagId

        <input type="submit" value="Save" />
      }

      <p>@Html.ActionLink("Back to list", "Index")</p>

      ```
    </details>
