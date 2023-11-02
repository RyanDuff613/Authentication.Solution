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