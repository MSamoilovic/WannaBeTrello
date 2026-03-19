using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Entities;

public class BoardMember
{
    public long BoardId { get; set; }
    public Board? Board { get; set; }
    
    public long UserId { get; set; }
    public User? User { get; set; }
    
    public BoardRole Role { get; set; } 
    
    
}