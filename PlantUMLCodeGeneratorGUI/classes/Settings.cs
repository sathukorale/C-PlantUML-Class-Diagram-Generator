namespace PlantUMLCodeGeneratorGUI
{
    struct Settings
    {
        public readonly bool IncludeOverridenMethods;
        public readonly bool IncludePrivateMethodsAndMembers;
        public readonly bool IncludeProtectedMethodsAndMembers;

        public Settings(bool includeOverridenMethods, bool includePrivateMethodsAndMembers, bool includeProtectedMethodsAndMembers)
        {
            IncludeOverridenMethods = includeOverridenMethods;
            IncludePrivateMethodsAndMembers = includePrivateMethodsAndMembers;
            IncludeProtectedMethodsAndMembers = includeProtectedMethodsAndMembers;
        }
    }
}