using System;
using System.Collections.Generic;

namespace Parser.Entities;

public partial class Comment
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? ThemeId { get; set; }

    public string? Text { get; set; }

    public int? CommentId { get; set; }

    public string? Date { get; set; }

    public string? UserNickname { get; set; }

    public virtual Theme? Theme { get; set; }

    public virtual User? User { get; set; }
}
