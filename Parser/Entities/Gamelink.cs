using System;
using System.Collections.Generic;

namespace Parser.Entities;

public partial class Gamelink
{
    public int Id { get; set; }

    public DateTime? Parsingdate { get; set; }

    public DateTime? Requestdate { get; set; }

    public string? Link { get; set; }
}
