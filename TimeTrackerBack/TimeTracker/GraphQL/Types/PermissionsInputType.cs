﻿using GraphQL.Types;
using TimeTracker.Models;

namespace TimeTracker.GraphQL.Types
{
    public class PermissionsInputType : InputObjectGraphType<Permissions>
    {
        public PermissionsInputType()
        {
            Field(i => i.CRUDUsers, type: typeof(BooleanGraphType));
            Field(i => i.EditPermiters, type: typeof(BooleanGraphType));
            Field(i => i.ViewUsers, type: typeof(BooleanGraphType));
            Field(i => i.EditWorkHours, type: typeof(BooleanGraphType));
            Field(i => i.ImportExcel, type: typeof(BooleanGraphType));
            Field(i => i.ControlPresence, type: typeof(BooleanGraphType));
            Field(i => i.ControlDayOffs, type: typeof(BooleanGraphType));
        }
    }
}
