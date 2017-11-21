namespace CornellBox.Models
{
    public struct Face
    {
        public readonly int[] VertsId;
        public readonly int[] NormsId;

        public Face(int[] vertsId, int[] normsId)
        {
            VertsId = vertsId;
            NormsId = normsId;
        }
    }
}
