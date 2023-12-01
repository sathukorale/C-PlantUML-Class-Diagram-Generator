namespace PlantUMLCodeGeneratorGUI
{
    class StringifiedContent
    {
        public readonly string ClassContent;
        public readonly string ClassConnectivityContent;

        public StringifiedContent(string classContent, string classRelationshipsContent)
        {
            ClassContent = classContent;
            ClassConnectivityContent = classRelationshipsContent;
        }
    }
}