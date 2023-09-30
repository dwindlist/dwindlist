using Microsoft.EntityFrameworkCore;

namespace dwindlist.Models.DB;

public class TodoItemDBContext : DbContext
{
    public TodoItemDBContext() { }
    public TodoItemDBContext(DbContextOptions<TodoItemDBContext> options) : base(options) { }

    public virtual DbSet<TodoItem> TodoItem { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("TodoItems");

            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .HasColumnType("int");

            entity.Property(e => e.UserId)
                .HasColumnName("UserId")
                .HasColumnType("string")
                .HasMaxLength(450);

            entity.Property(e => e.Label)
                .HasColumnName("Label")
                .HasColumnType("string")
                .HasMaxLength(256);

            entity.Property(e => e.ParentId)
                .HasColumnName("ParentId")
                .HasDefaultValue(0)
                .HasColumnType("int");

            entity.Property(e => e.Status)
                .HasColumnName("Status")
                .HasDefaultValue('i')
                .HasColumnType("char(1)");
        });
    }
}
