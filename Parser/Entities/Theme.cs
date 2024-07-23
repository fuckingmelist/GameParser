using System;
using System.Collections.Generic;

namespace Parser.Entities;

public partial class Theme
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public int? GameId { get; set; }

    public int? UserId { get; set; }

    public string? UserNickname { get; set; }

    public virtual List<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Game? Game { get; set; }

    public virtual User? User { get; set; }
}
