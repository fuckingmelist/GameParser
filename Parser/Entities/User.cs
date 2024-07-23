using System;
using System.Collections.Generic;

namespace Parser.Entities;

public partial class User
{
    public int Id { get; set; }

    public string? Nickname { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? EMail { get; set; }

    public byte[]? ProfilePicture { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Theme> Themes { get; set; } = new List<Theme>();
}
