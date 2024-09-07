using Aidan.EFCore;

namespace Application.Infra.Database;

public class CassinoDbContext : EFCoreSqliteContext
{
    public CassinoDbContext(FileInfo fileInfo)
        : base(fileInfo)
    {

    }
}
