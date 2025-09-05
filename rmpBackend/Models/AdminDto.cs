namespace rmpBackend.Models
{
    
    public class AssignRoleDto
    {
        public int UserId { get; set; } = -1;
        public int RoleId { get; set; } = -1;
    }
    public class removeRoleDto
    {
           public int RoleId { get; set; } = -1;
    }
    public class removeUserDto
    {
        public int UserId { get; set; } = -1;
        
    }
    public class NewRoleDto
    {
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
