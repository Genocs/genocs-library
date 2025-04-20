using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Context;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options);
