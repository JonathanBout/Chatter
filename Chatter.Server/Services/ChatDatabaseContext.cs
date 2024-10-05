using Chatter.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Chatter.Server.Services
{
	public class ChatDatabaseContext : DbContext
	{
		public required DbSet<User> Users { get; set; }
		public required DbSet<Message> Messages { get; set; }

		public ChatDatabaseContext(DbContextOptions<ChatDatabaseContext> options) : base(options)
		{
		}

		public ChatDatabaseContext()
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().HasKey(u => u.Id);
			modelBuilder.Entity<Message>().HasKey(m => m.Id);

			modelBuilder.Entity<User>().HasData(new User
			{
				Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
				Username = "Admin",
				PasswordHash = []
			},
			new User
			{
				Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
				Username = "DefaultUser",
				PasswordHash = []
			});

			modelBuilder.Entity<Message>().HasOne(m => m.Receiver).WithMany(u => u.AvailableMessages);
			modelBuilder.Entity<Message>().HasOne(m => m.Sender).WithMany();
		}
	}
}
