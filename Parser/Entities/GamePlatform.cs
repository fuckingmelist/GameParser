using System;
using System.Collections.Generic;

namespace Parser.Entities;

public partial class GamePlatform
{
    public int Id { get; set; }

    public int? GameId { get; set; }

    public int? PlatformId { get; set; }
}
