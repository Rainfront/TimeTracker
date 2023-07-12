﻿using TimeTracker.Models;

namespace TimeTracker.GraphQL.Types.IdentityTipes.Models
{
    public class LoginOutput
    {
        public int user_id { get; set; }
        public string access_token { get; set; } = null!;
    }
}
