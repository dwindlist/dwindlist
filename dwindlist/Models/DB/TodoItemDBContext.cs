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
            // warning To protect potentially sensitive information in your connection string,
            // you should move it out of source code.See http://go.microsoft.com/fwlink/?LinkId=723263
            // for guidance on storing connection strings.
            optionsBuilder.UseSqlServer("Server=(localdb)\\dwindlistdb;Database=dwindlist;Trusted_Connection=True;MultipleActiveResultSets=true");
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
