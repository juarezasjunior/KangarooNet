using KangarooNet.Domain;
using Microsoft.AspNetCore.Identity;

public partial class ApplicationUser : IdentityUser<Guid>, IApplicationUser
{
    public string FullName { get; set; }
}