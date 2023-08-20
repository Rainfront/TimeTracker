﻿using GraphQL.Types;
using TimeTracker.Models;
using TimeTracker.Repositories;

namespace TimeTracker.GraphQL.Types.UserTypes
{
    public class UserType : ObjectGraphType<User>
    {
        public UserType()
        {
            Field(i => i.Id, type: typeof(IdGraphType));
            Field(i => i.Login);
            Field(i => i.Password);
            Field(i => i.FullName);
            Field(i => i.Email, type: typeof(StringGraphType));
            Field(i => i.ResetCode, type: typeof(StringGraphType));
            Field(t => t.TimeManagedBy, nullable: false);
            Field(i => i.Enabled, type: typeof(BooleanGraphType));
        }
    }
}
