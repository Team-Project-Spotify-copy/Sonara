using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Subscription;

public class InviteUserRequestDto
{
    public string TargetUsername { get; set; } = string.Empty;
}