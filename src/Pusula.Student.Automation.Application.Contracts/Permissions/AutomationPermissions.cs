namespace Pusula.Student.Automation.Permissions
{
    public static class AutomationPermissions
    {
        public const string GroupName = "Automation";

        // ----- Student -----
        public static class Students
        {
            public const string Default = GroupName + ".Students";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        // ----- Teacher -----
        public static class Teachers
        {
            public const string Default = GroupName + ".Teachers";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        // ----- Course -----
        public static class Courses
        {
            public const string Default = GroupName + ".Courses";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        // ----- Enrollment -----
        public static class Enrollments
        {
            public const string Default = GroupName + ".Enrollments";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        // ----- Grade -----
        public static class Grades
        {
            public const string Default = GroupName + ".Grades";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        // ----- Attendance -----
        public static class Attendances
        {
            public const string Default = GroupName + ".Attendances";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
    }
}
